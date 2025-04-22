using System;

namespace AssetInventory
{
    [Serializable]
    public sealed class ProjectMetadata
    {
        public DateTime CreatedAt;
        public DateTime UpdatedAt;
        public string GenesisId;
        public string DefaultEnvironmentId;
        public bool KidsStoreCompliance;
        public string OrganizationGenesisId;
        public string OrganizationId;
        public string Id;

        public override string ToString()
        {
            return $"Project Metadata '{Id}'";
        }
    }
}
