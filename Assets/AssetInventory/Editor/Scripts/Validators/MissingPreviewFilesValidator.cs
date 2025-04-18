using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetInventory
{
    public sealed class MissingPreviewFilesValidator : Validator
    {
        public MissingPreviewFilesValidator()
        {
            Type = ValidatorType.DB;
            Name = "Missing Preview Files";
            Description = "Scans all indexed files that are supposed to have previews if the files actually still exist on the file system.";
            FixCaption = "Schedule Recreation";
        }

        public override async Task Validate()
        {
            CurrentState = State.Scanning;
            DBIssues = await GatherOrphanedPreviews();
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

        private static async Task<List<AssetInfo>> GatherOrphanedPreviews()
        {
            List<AssetInfo> result = new List<AssetInfo>();

            string query = "select * from AssetFile where PreviewState = ? or PreviewState = ?";
            List<AssetInfo> files = DBAdapter.DB.Query<AssetInfo>(query, AssetFile.PreviewOptions.Provided, AssetFile.PreviewOptions.Custom).ToList();
            string previewFolder = AI.GetPreviewFolder();

            int progress = 0;
            int count = files.Count;
            int progressId = MetaProgress.Start("Gathering orphaned preview references");

            foreach (AssetInfo file in files)
            {
                progress++;
                MetaProgress.Report(progressId, progress, count, file.FileName);
                if (progress % 5000 == 0) await Task.Yield();

                string previewFile = file.GetPreviewFile(previewFolder);
                if (!File.Exists(previewFile)) result.Add(file);
            }
            MetaProgress.Remove(progressId);

            return result;
        }
    }
}