using System;
#if UNITY_2021_2_OR_NEWER && UNITY_EDITOR_WIN
using System.Drawing.Imaging;
#endif
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetInventory
{
    public sealed class PreviewManager
    {
        private const int MAX_REQUESTS = 50;
        private const int OPEN_REQUESTS = 5;

        public static async Task<bool> Create(AssetInfo info, string sourcePath = null, Action onCreated = null)
        {
            // check if previewable at all
            if (!IsPreviewable(info.FileName, true, info)) return false;

            if (sourcePath == null)
            {
                sourcePath = await AI.EnsureMaterializedAsset(info);
                if (sourcePath == null)
                {
                    if (info.PreviewState != AssetFile.PreviewOptions.Provided)
                    {
                        DBAdapter.DB.Execute("update AssetFile set PreviewState=? where Id=?", AssetFile.PreviewOptions.Error, info.Id);
                    }
                    return false;
                }
            }

            string previewFile = info.GetPreviewFile(AI.GetPreviewFolder());
            string animPreviewFile = info.GetPreviewFile(AI.GetPreviewFolder(), true);
            Texture2D texture = null;
            Texture2D animTexture = null;
            bool directPreview = false;

#if UNITY_2021_2_OR_NEWER && UNITY_EDITOR_WIN
            // from Unity 2021.2+ we can take a shortcut for images since the drawing library is supported in C#
            if (ImageUtils.SYSTEM_IMAGE_TYPES.Contains(info.Type))
            {
                // take shortcut for images and skip Unity importer
                if (ImageUtils.ResizeImage(sourcePath, previewFile, AI.Config.upscaleSize, !AI.Config.upscaleLossless, ImageFormat.Png))
                {
                    StorePreviewResult(new PreviewRequest {DestinationFile = previewFile, Id = info.Id, Icon = Texture2D.grayTexture, SourceFile = sourcePath});
                    onCreated?.Invoke();
                }
                else
                {
                    // try to use original preview
                    string originalPreviewFile = DerivePreviewFile(sourcePath);
                    if (File.Exists(originalPreviewFile))
                    {
                        File.Copy(originalPreviewFile, previewFile, true);
                        StorePreviewResult(new PreviewRequest {DestinationFile = previewFile, Id = info.Id, Icon = Texture2D.grayTexture, SourceFile = originalPreviewFile});
                        info.PreviewState = AssetFile.PreviewOptions.Provided;
                        DBAdapter.DB.Execute("update AssetFile set PreviewState=? where Id=?", AssetFile.PreviewOptions.Provided, info.Id);
                        onCreated?.Invoke();
                    }
                    else if (info.PreviewState != AssetFile.PreviewOptions.Provided)
                    {
                        info.PreviewState = AssetFile.PreviewOptions.Error;
                        DBAdapter.DB.Execute("update AssetFile set PreviewState=? where Id=?", AssetFile.PreviewOptions.Error, info.Id);
                    }
                }
            }
            else
#endif
            if (AI.IsFileType(info.FileName, "Fonts"))
            {
                PreviewRequest req = UnityPreviewGenerator.Localize(info.Id, sourcePath, previewFile);
                texture = FontPreviewGenerator.Create(req.TempFileRel, AI.Config.upscaleSize);
                directPreview = true;
            }
#if UNITY_EDITOR_WIN
            else if (AI.IsFileType(info.FileName, "Videos"))
            {
                PreviewRequest req = UnityPreviewGenerator.Localize(info.Id, sourcePath, previewFile);

                // first static
                texture = await VideoPreviewGenerator.Create(req.TempFileRel, AI.Config.upscaleSize, 1, clip =>
                {
                    info.Width = (int)clip.width;
                    info.Height = (int)clip.height;
                    info.Length = (float)clip.length;
                    DBAdapter.DB.Execute("update AssetFile set Width=?, Height=?, Length=? where Id=?", info.Width, info.Height, info.Length, info.Id);
                });

                // give time for video player cleanup, might result in black textures otherwise when done in quick succession
                await Task.Yield();

                // now animated
                animTexture = await VideoPreviewGenerator.Create(req.TempFileRel, AI.Config.upscaleSize, AI.Config.animationGrid * AI.Config.animationGrid, _ => {});
                await Task.Yield();

                directPreview = true;
            }
#endif
            else
            {
                // import through Unity
                if (DependencyAnalysis.NeedsScan(info.Type))
                {
                    if (info.DependencyState == AssetInfo.DependencyStateOptions.Unknown) await AI.CalculateDependencies(info);
                    if (info.Dependencies.Count > 0 || info.SRPMainReplacement != null) sourcePath = await AI.CopyTo(info, UnityPreviewGenerator.GetPreviewWorkFolder(), true);
                    if (sourcePath == null) // can happen when file system issues occur
                    {
                        if (info.PreviewState != AssetFile.PreviewOptions.Provided)
                        {
                            info.PreviewState = AssetFile.PreviewOptions.Error;
                            DBAdapter.DB.Execute("update AssetFile set PreviewState=? where Id=?", AssetFile.PreviewOptions.Error, info.Id);
                        }
                        return false;
                    }
                }

                UnityPreviewGenerator.RegisterPreviewRequest(info.Id, sourcePath, previewFile, req =>
                {
                    StorePreviewResult(req);
                    if (req.Icon != null)
                    {
                        onCreated?.Invoke();
                    }
                    else
                    {
                        Debug.LogError($"Unity did not return any preview image for '{info.FileName}'.");
                    }
                }, info.Dependencies?.Count > 0);

                await EnsureProgress();
            }
            if (directPreview)
            {
                if (texture != null)
                {
#if UNITY_2021_2_OR_NEWER
                    await File.WriteAllBytesAsync(previewFile, texture.EncodeToPNG());
#else
                    File.WriteAllBytes(previewFile, texture.EncodeToPNG());
#endif
                    StorePreviewResult(new PreviewRequest {DestinationFile = previewFile, Id = info.Id, Icon = Texture2D.grayTexture, SourceFile = sourcePath});
                    onCreated?.Invoke();

                    if (animTexture != null)
                    {
#if UNITY_2021_2_OR_NEWER
                        await File.WriteAllBytesAsync(animPreviewFile, animTexture.EncodeToPNG());
#else
                        File.WriteAllBytes(animPreviewFile, animTexture.EncodeToPNG());
#endif
                    }

                    return true;
                }
                if (info.PreviewState != AssetFile.PreviewOptions.Provided)
                {
                    info.PreviewState = AssetFile.PreviewOptions.Error;
                    DBAdapter.DB.Execute("update AssetFile set PreviewState=? where Id=?", AssetFile.PreviewOptions.Error, info.Id);
                    return false;
                }
            }
            return true;
        }

        public static bool IsPreviewable(string file, bool includeComplex, AssetInfo autoMarkNA = null)
        {
            bool previewable = false;
            if (!file.Contains("__MACOSX"))
            {
                if (includeComplex)
                {
                    previewable = AI.IsFileType(file, "Audio")
                        || AI.IsFileType(file, "Images")
#if UNITY_EDITOR_WIN
                        || AI.IsFileType(file, "Videos")
#endif
                        || AI.IsFileType(file, "Models")
                        || AI.IsFileType(file, "Fonts")
                        || AI.IsFileType(file, "Prefabs")
                        || AI.IsFileType(file, "Materials");
                }
                else
                {
                    previewable = AI.IsFileType(file, "Audio")
                        || AI.IsFileType(file, "Images")
#if UNITY_EDITOR_WIN
                        || AI.IsFileType(file, "Videos")
#endif
                        || AI.IsFileType(file, "Fonts")
                        || AI.IsFileType(file, "Models");
                }
            }
            if (!previewable && autoMarkNA != null)
            {
                if (autoMarkNA.PreviewState != AssetFile.PreviewOptions.Provided)
                {
                    autoMarkNA.PreviewState = AssetFile.PreviewOptions.NotApplicable;
                    DBAdapter.DB.Execute("update AssetFile set PreviewState=? where Id=?", autoMarkNA.PreviewState, autoMarkNA.Id);
                }
            }

            return previewable;
        }

        public static async Task EnsureProgress()
        {
            UnityPreviewGenerator.EnsureProgress();
            if (UnityPreviewGenerator.ActiveRequestCount() > MAX_REQUESTS) await UnityPreviewGenerator.ExportPreviews(OPEN_REQUESTS);
        }

        public static string DerivePreviewFile(string sourcePath)
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(sourcePath)), "preview.png");
        }

        public static void StorePreviewResult(PreviewRequest req)
        {
            AssetFile af = DBAdapter.DB.Find<AssetFile>(req.Id);
            if (af == null) return;

            if (!File.Exists(req.DestinationFile))
            {
                if (af.PreviewState != AssetFile.PreviewOptions.Provided)
                {
                    af.PreviewState = AssetFile.PreviewOptions.Error;
                    DBAdapter.DB.Update(af);
                }
                return;
            }

            if (req.Obj != null)
            {
                if (req.Obj is Texture2D tex)
                {
                    af.Width = tex.width;
                    af.Height = tex.height;
                }
                if (req.Obj is AudioClip clip)
                {
                    af.Length = clip.length;
                }
            }

            // do not remove originally supplied previews even in case of error
            af.PreviewState = req.Icon != null ? AssetFile.PreviewOptions.Custom : (af.PreviewState != AssetFile.PreviewOptions.Provided ? AssetFile.PreviewOptions.Error : AssetFile.PreviewOptions.Provided);
            af.Hue = -1f;

            DBAdapter.DB.Update(af);
        }
    }
}
