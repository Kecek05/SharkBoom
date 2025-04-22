using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using Object = UnityEngine.Object;

namespace AssetInventory
{
    public static class VideoPreviewGenerator
    {
        public static Task<Texture2D> Create(VideoClip videoClip, int size = 128, int frameCount = 4, Action<VideoClip> onSuccess = null)
        {
            TaskCompletionSource<Texture2D> tcs = new TaskCompletionSource<Texture2D>();

            if (videoClip == null)
            {
                Debug.LogError("VideoClip is null.");
                tcs.SetResult(null);
                return tcs.Task;
            }

            // Create a new GameObject to hold the VideoPlayer
            GameObject tempGO = new GameObject("TempVideoPlayer");
            VideoPlayer videoPlayer = tempGO.AddComponent<VideoPlayer>();

            // Configure the VideoPlayer
            videoPlayer.renderMode = VideoRenderMode.APIOnly; // Use APIOnly render mode
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.skipOnDrop = false;
            videoPlayer.sendFrameReadyEvents = true;

            // Start the coroutine to handle the video processing
            EditorCoroutineUtility.StartCoroutineOwnerless(ProcessVideoPlayer(videoPlayer, tempGO, size, frameCount, tcs, onSuccess));

            return tcs.Task;
        }

        private static IEnumerator ProcessVideoPlayer(VideoPlayer videoPlayer, GameObject tempGO, int size, int frameCount, TaskCompletionSource<Texture2D> tcs, Action<VideoClip> onSuccess = null)
        {
            // Prepare the video and wait until it's prepared
            bool isPrepared = false;
            videoPlayer.prepareCompleted += (vp) => { isPrepared = true; };
            videoPlayer.Prepare();

            while (!isPrepared)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
            }

            // Get video duration and calculate frame times
            double duration = videoPlayer.clip.length;
            List<double> frameTimes = new List<double>();

            for (int i = 0; i < frameCount; i++)
            {
                double time = (duration * i) / Mathf.Max(frameCount - 1, 1); // Evenly spaced times
                frameTimes.Add(time);
            }

            // Adjust first and last frames to ensure they are within bounds
            frameTimes[0] = Math.Max(frameTimes[0], 0);
            frameTimes[frameCount - 1] = Math.Min(frameTimes[frameCount - 1], duration - 0.1);

            // List to hold captured frames
            List<Texture2D> frames = new List<Texture2D>();

            // Capture frames at specified times
            for (int i = 0; i < frameCount; i++)
            {
                double time = frameTimes[i];

                // Seek to the target time
                bool isSeeking = true;
                bool frameReady = false;

                videoPlayer.seekCompleted += (vp) => { isSeeking = false; };
                videoPlayer.frameReady += (vp, frameIdx) => { frameReady = true; };

                // videoPlayer.Stop();  // required on Mac
                videoPlayer.time = time;
                videoPlayer.Play();

                // If time is 0, manually set isSeeking to false since event will not trigger on Mac
                // if (time == 0) isSeeking = false;

                // Wait for seek to complete
                while (isSeeking) // Mac: && !videoPlayer.isPaused) 
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    yield return null;
                }

                // Wait for frame to be ready
                while (!frameReady)
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    yield return null;
                }

                // Extract the texture from VideoPlayer
                Texture texture = videoPlayer.texture;
                if (texture != null)
                {
                    int originalWidth = texture.width;
                    int originalHeight = texture.height;

                    // Handle invalid dimensions
                    if (originalWidth == 0 || originalHeight == 0)
                    {
                        Debug.LogError("VideoPlayer texture has invalid dimensions.");
                        continue;
                    }

                    // Calculate the scale factor to maintain aspect ratio
                    float widthScale = (float)size / originalWidth;
                    float heightScale = (float)size / originalHeight;
                    float scale = Mathf.Min(widthScale, heightScale, 1.0f); // Ensure scale is not greater than 1

                    int newWidth = Mathf.RoundToInt(originalWidth * scale);
                    int newHeight = Mathf.RoundToInt(originalHeight * scale);

                    // Create a RenderTexture with the new dimensions
                    RenderTexture renderTexture = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32);
                    RenderTexture.active = renderTexture;

                    // Copy the VideoPlayer texture to the RenderTexture with scaling
                    Graphics.Blit(texture, renderTexture);

                    // Read the pixels from the RenderTexture into a Texture2D
                    Texture2D frameTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
                    frameTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
                    frameTexture.Apply();

                    // Add the frame to the list
                    frames.Add(frameTexture);

                    // Cleanup
                    RenderTexture.active = null;
                    RenderTexture.ReleaseTemporary(renderTexture);

                    // Reset flags and events
                    frameReady = false;
                    videoPlayer.frameReady -= (vp, frameIdx) => { frameReady = true; };
                    videoPlayer.seekCompleted -= (vp) => { isSeeking = false; };
                }
                else
                {
                    Debug.LogError("VideoPlayer texture is null.");
                }

                videoPlayer.Pause();

                // Yield to ensure the Editor updates
                EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
            }

            // Assemble frames into a texture sheet
            if (frames.Count > 0)
            {
                Texture2D textureSheet = AssembleTextureSheet(frames);

                onSuccess?.Invoke(videoPlayer.clip);

                // Cleanup
                Cleanup(videoPlayer, tempGO);

                tcs.SetResult(textureSheet);
            }
            else
            {
                // Cleanup
                Cleanup(videoPlayer, tempGO);
                tcs.SetResult(null);
                Debug.LogError("No frames were captured.");
            }
        }

        private static Texture2D AssembleTextureSheet(List<Texture2D> frames)
        {
            // Determine grid size (e.g., number of columns and rows)
            int frameCount = frames.Count;
            int columns = Mathf.CeilToInt(Mathf.Sqrt(frameCount));
            int rows = Mathf.CeilToInt((float)frameCount / columns);

            // Get frame dimensions (assuming all frames have the same dimensions)
            int frameWidth = frames[0].width;
            int frameHeight = frames[0].height;

            // Create a new Texture2D to hold all frames
            Texture2D textureSheet = new Texture2D(frameWidth * columns, frameHeight * rows, TextureFormat.RGBA32, false);

            // Copy frames into the texture sheet
            for (int i = 0; i < frameCount; i++)
            {
                int x = (i % columns) * frameWidth;
                int y = ((rows - 1) - (i / columns)) * frameHeight; // Start from bottom

                textureSheet.SetPixels(x, y, frameWidth, frameHeight, frames[i].GetPixels());
            }

            textureSheet.Apply();

            return textureSheet;
        }

        private static void Cleanup(VideoPlayer videoPlayer, GameObject tempGO)
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                Object.DestroyImmediate(videoPlayer);
            }

            if (tempGO != null)
            {
                Object.DestroyImmediate(tempGO);
            }
        }

        public static Task<Texture2D> Create(string file, int size = 128, int frameCount = 4, Action<VideoClip> onSuccess = null)
        {
            VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(file);
            if (clip == null)
            {
                Debug.LogError($"Failed to load video clip from: {file}");
                return Task.FromResult<Texture2D>(null);
            }

            return Create(clip, size, frameCount, onSuccess);
        }
    }
}