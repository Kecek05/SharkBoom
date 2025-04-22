using System;

namespace AssetInventory
{
    [Serializable]
    public sealed class Dependency
    {
        public string location;
        public int id;
        public string name;

        public Dependency()
        {
        }

        public override string ToString()
        {
            return $"Dependency '{name}' ({id}, '{location}')";
        }
    }
}