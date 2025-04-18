using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public sealed class AssetUsage : AssetProgress
    {
        public async Task<List<AssetInfo>> Calculate()
        {
            ResetState(false);
            ReadOnly = true;

            List<AssetInfo> result = new List<AssetInfo>();

            // identify asset packages through guids lookup
            CurrentMain = "Phase 1/2: Gathering guids";
            string[] guids = AssetDatabase.FindAssets("", new[] {"Assets"});

            CurrentMain = "Phase 2/2: Looking up assets";
            MainCount = guids.Length;
            MainProgress = 0;

            // check if current project is indexed via additional folder
            // exclude as otherwise that would result in current project being reported as source
            string curPath = Path.GetDirectoryName(Application.dataPath);
            curPath = AI.MakeRelative(curPath);
            List<int> ids = DBAdapter.DB.Query<Asset>("select Id from Asset where Location = ?", curPath).Select(a => a.Id).ToList();

            int batchSize = AI.Config.reportingBatchSize;
            for (int i = 0; i < guids.Length; i += batchSize)
            {
                if (CancellationRequested) break;

                List<string> batch = guids.Skip(i).Take(batchSize).ToList();
                MainProgress += batch.Count;

                await Task.Yield();

                Dictionary<string, List<AssetInfo>> batchFiles = AssetUtils.Guids2Files(batch, true, ids);
                foreach (KeyValuePair<string, List<AssetInfo>> kvp in batchFiles)
                {
                    List<AssetInfo> files = kvp.Value;
                    if (files.Count > 1)
                    {
                        Debug.LogWarning($"Multiple origin candidates for found for guid {kvp.Key}: \n\n" + string.Join("\n", files.Select(ai => $"{ai.Path} ({ai.GetDisplayName()} - {ai.Id})")) + "\n");
                        continue;
                    }
                    result.Add(files[0]);
                }
            }
            ResetState(true);

            return result;
        }
    }
}
