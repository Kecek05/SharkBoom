using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AssetInventory
{
    public static class Tagging
    {
        public static event Action OnTagsChanged;

        internal static IEnumerable<TagInfo> Tags
        {
            get
            {
                if (_tags == null) LoadTagAssignments();
                return _tags;
            }
        }
        private static List<TagInfo> _tags;

        internal static int TagHash { get; private set; }

        public static bool AddTagAssignment(int targetId, string tag, TagAssignment.Target target, bool fromAssetStore = false)
        {
            Tag existingT = AddTag(tag, fromAssetStore);
            if (existingT == null) return false;

            TagAssignment existingA = DBAdapter.DB.Find<TagAssignment>(t => t.TagId == existingT.Id && t.TargetId == targetId && t.TagTarget == target);
            if (existingA != null) return false; // already added

            TagAssignment newAssignment = new TagAssignment(existingT.Id, target, targetId);
            DBAdapter.DB.Insert(newAssignment);

            return true;
        }

        public static bool AddTagAssignment(AssetInfo info, string tag, TagAssignment.Target target, bool byUser = false)
        {
            if (!AddTagAssignment(target == TagAssignment.Target.Asset ? info.Id : info.AssetId, tag, target)) return false;

            LoadTagAssignments(info);
            if (byUser && target == TagAssignment.Target.Asset && info.AssetSource == Asset.Source.AssetManager) AddRemoteTag(info, tag);

            return true;
        }

        public static void RemoveTagAssignment(AssetInfo info, TagInfo tagInfo, bool autoReload = true, bool byUser = false)
        {
            DBAdapter.DB.Delete<TagAssignment>(tagInfo.Id);

            if (autoReload) LoadTagAssignments(info);
            if (byUser && tagInfo.TagTarget == TagAssignment.Target.Asset && info.AssetSource == Asset.Source.AssetManager) RemoveRemoteTag(info, tagInfo.Name);
        }

        public static void RemoveAssetTagAssignment(List<AssetInfo> infos, string name, bool byUser)
        {
            infos.ForEach(info =>
            {
                TagInfo tagInfo = info.AssetTags?.Find(t => t.Name == name);
                if (tagInfo == null) return;
                RemoveTagAssignment(info, tagInfo, false, byUser);
                info.AssetTags.RemoveAll(t => t.Name == name);
                info.SetTagsDirty();
            });
            LoadTagAssignments();
        }

        public static void RemovePackageTagAssignment(List<AssetInfo> infos, string name, bool byUser)
        {
            infos.ForEach(info =>
            {
                TagInfo tagInfo = info.PackageTags?.Find(t => t.Name == name);
                if (tagInfo == null) return;
                RemoveTagAssignment(info, tagInfo, false, byUser);
                info.PackageTags.RemoveAll(t => t.Name == name);
                info.SetTagsDirty();
            });
            LoadTagAssignments();
        }

        private static async void AddRemoteTag(AssetInfo info, string tagName)
        {
#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
            // sync online with AM
            CloudAssetManagement cam = await AI.GetCloudAssetManagement();
            await cam.AddTags(info.ToAsset(), info, new List<string> {tagName});
#else
            Debug.LogWarning("Tag changes will not be synced back to Unity Cloud since this project does not have the Cloud Asset dependencies installed (see Settings).");
            await Task.Yield();
#endif
        }

        private static async void RemoveRemoteTag(AssetInfo info, string tagName)
        {
#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
            // sync online with AM
            CloudAssetManagement cam = await AI.GetCloudAssetManagement();
            await cam.RemoveTags(info.ToAsset(), info, new List<string> {tagName});
#else
            Debug.LogWarning("Tag changes will not be synced back to Unity Cloud since this project does not have the Cloud Asset dependencies installed (see Settings).");
            await Task.Yield();
#endif
        }

        internal static void LoadTagAssignments(AssetInfo info = null, bool triggerEvents = true)
        {
            string dataQuery = "SELECT *, TagAssignment.Id as Id from TagAssignment inner join Tag on Tag.Id = TagAssignment.TagId order by TagTarget, TargetId";
            _tags = DBAdapter.DB.Query<TagInfo>($"{dataQuery}").ToList();
            TagHash = Random.Range(0, int.MaxValue);

            info?.SetTagsDirty();
            if (triggerEvents) OnTagsChanged?.Invoke();
        }

        public static List<TagInfo> GetAssetTags(int assetFileId)
        {
            return Tags?.Where(t => t.TagTarget == TagAssignment.Target.Asset && t.TargetId == assetFileId)
                .OrderBy(t => t.Name).ToList();
        }

        public static List<TagInfo> GetPackageTags(int assetId)
        {
            return Tags?.Where(t => t.TagTarget == TagAssignment.Target.Package && t.TargetId == assetId)
                .OrderBy(t => t.Name).ToList();
        }

        public static void SaveTag(Tag tag)
        {
            DBAdapter.DB.Update(tag);
            LoadTagAssignments();
        }

        public static Tag AddTag(string name, bool fromAssetStore = false)
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name)) return null;

            Tag tag = DBAdapter.DB.Find<Tag>(t => t.Name.ToLower() == name.ToLower());
            if (tag == null)
            {
                tag = new Tag(name);
                tag.FromAssetStore = fromAssetStore;
                DBAdapter.DB.Insert(tag);

                OnTagsChanged?.Invoke();
            }
            else if (!tag.FromAssetStore && fromAssetStore)
            {
                tag.FromAssetStore = true;
                DBAdapter.DB.Update(tag); // don't trigger changed event in such cases, this is just for bookkeeping
            }

            return tag;
        }

        public static void RenameTag(Tag tag, string newName)
        {
            newName = newName.Trim();
            if (string.IsNullOrWhiteSpace(newName)) return;

            tag.Name = newName;
            DBAdapter.DB.Update(tag);
            LoadTagAssignments();
        }

        public static void DeleteTag(Tag tag)
        {
            DBAdapter.DB.Execute("DELETE from TagAssignment where TagId=?", tag.Id);
            DBAdapter.DB.Delete<Tag>(tag.Id);
            LoadTagAssignments();
        }

        public static List<Tag> LoadTags()
        {
            return DBAdapter.DB.Table<Tag>().ToList().OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
