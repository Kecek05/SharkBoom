using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public sealed class MissingAudioLengthValidator : Validator
    {
        public MissingAudioLengthValidator()
        {
            Type = ValidatorType.DB;
            Name = "Missing Audio Duration";
            Description = "Finds indexed audio files for which the duration has not been determined yet.";
            FixCaption = "Mark for Reindexing";
        }

        public override async Task Validate()
        {
            CurrentState = State.Scanning;

            await Task.Yield();

            // query all asset files that do not have an asset id that is contained in the asset table
            string query = "select * from AssetFile where Length = 0 and Type in ('" + string.Join("','", AI.TypeGroups["Audio"]) + "')";
            DBIssues = DBAdapter.DB.Query<AssetInfo>(query);

            CurrentState = State.Completed;
        }

        public override async Task Fix()
        {
            CurrentState = State.Fixing;

            string audioTypes = "'" + string.Join("','", AI.TypeGroups["Audio"]) + "'";

            List<AssetInfo> assets = DBAdapter.DB.Query<AssetInfo>($"select Distinct(AssetId) from AssetFile where Length = 0 and Type in ({audioTypes})");
            string affectedPackages = string.Join(",", assets.Select(a => a.AssetId));
            DBAdapter.DB.Execute($"update Asset set CurrentState=? where Id in ({affectedPackages})", Asset.State.InProcess);
            
            int fileCount =  DBAdapter.DB.Execute($"update AssetFile set Size = 0 where Length = 0 and Type in ({audioTypes})");
            Debug.Log($"Marked {fileCount} files across {affectedPackages} packages for reindexing.");
            
            EditorUtility.DisplayDialog("Success", $"During the next index update, up to {fileCount} audio files will be reindexed to try to read the length again.", "OK");

            await Validate();
        }
    }
}
