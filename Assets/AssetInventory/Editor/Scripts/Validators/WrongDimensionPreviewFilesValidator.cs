using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetInventory
{
    public sealed class WrongDimensionPreviewFilesValidator : Validator
    {
        public WrongDimensionPreviewFilesValidator()
        {
            Type = ValidatorType.DB;
            Name = "Preview Images with Incorrect Dimensions";
            Description = "Scans all preview images if they have the correct dimensions as specified under Settings/Previews. Will report them if they can be fixed (currently only image files in higher resolutions than requested for preview).";
            FixCaption = "Schedule Recreation";
        }

        public override async Task Validate()
        {
            CurrentState = State.Scanning;
            DBIssues = await CheckPreviewDimensions();
            CurrentState = State.Completed;
        }

        public override async Task Fix()
        {
            CurrentState = State.Fixing;

            string query = "update AssetFile set PreviewState = ? where Id = ?";
            foreach (AssetInfo info in DBIssues)
            {
                DBAdapter.DB.Execute(query, AssetFile.PreviewOptions.Redo, info.Id);
            }
            await Task.Yield();

            CurrentState = State.Idle;
        }

        private static async Task<List<AssetInfo>> CheckPreviewDimensions()
        {
            List<AssetInfo> result = new List<AssetInfo>();

            // check if original file is actually larger than requested preview size before doing any expensive file system checks
            // this check will for performance reasons not cater for the case when requested preview is e.g. 512, source file is 256 and preview is 128 where the preview could at least be doubled 
            string typeStr = string.Join("\",\"", AI.TypeGroups["Images"]);
            string query = "select * from AssetFile where (PreviewState = ? or PreviewState = ?) and Type in (\"" + typeStr + "\") and (Width >= ? or Height >= ?)";
            List<AssetInfo> files = DBAdapter.DB.Query<AssetInfo>(query, AssetFile.PreviewOptions.Provided, AssetFile.PreviewOptions.Custom, AI.Config.upscaleSize, AI.Config.upscaleSize).ToList();
            string previewFolder = AI.GetPreviewFolder();

            int progress = 0;
            int count = files.Count;
            int progressId = MetaProgress.Start("Checking preview dimensions");

            foreach (AssetInfo file in files)
            {
                progress++;
                MetaProgress.Report(progressId, progress, count, file.FileName);
                if (progress % 500 == 0) await Task.Yield();

                string previewFile = file.GetPreviewFile(previewFolder);
                if (!File.Exists(previewFile)) continue;

                Tuple<int, int> dimensions = ImageUtils.GetDimensions(previewFile);
                if (dimensions == null || dimensions.Item1 <= 0 || dimensions.Item2 <= 0) continue;

                // one dimension must fit
                if (dimensions.Item1 != AI.Config.upscaleSize && dimensions.Item2 != AI.Config.upscaleSize)
                {
                    result.Add(file);
                }
            }
            MetaProgress.Remove(progressId);

            return result;
        }
    }
}