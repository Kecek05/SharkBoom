using System;
using System.Reflection;

namespace AssetInventory
{
    [Serializable]
    public sealed class AssetOrigin
    {
        public int productId;
        public string packageVersion;
        public string packageName;
        public string assetPath;
        public int uploadId;

        public AssetOrigin(int productId = 0, string packageName = "", string packageVersion = "", int uploadId = 0)
        {
            this.productId = productId;
            this.packageVersion = packageVersion;
            this.packageName = packageName;
            this.uploadId = uploadId;
        }

        public AssetOrigin() {}

        public object ToUnity()
        {
            Type assetOriginType = Type.GetType("UnityEditor.AssetOrigin, UnityEditor.CoreModule");
            if (assetOriginType != null)
            {
                object assetOriginInstance = Activator.CreateInstance(assetOriginType);

                FieldInfo productIdProp = assetOriginType.GetField("productId");
                FieldInfo packageVersionProp = assetOriginType.GetField("packageVersion");
                FieldInfo packageNameProp = assetOriginType.GetField("packageName");
                FieldInfo assetPathProp = assetOriginType.GetField("assetPath");
                FieldInfo uploadIdProp = assetOriginType.GetField("uploadId");

                if (productIdProp != null) productIdProp.SetValue(assetOriginInstance, productId);
                if (packageVersionProp != null) packageVersionProp.SetValue(assetOriginInstance, packageVersion);
                if (packageNameProp != null) packageNameProp.SetValue(assetOriginInstance, packageName);
                if (assetPathProp != null) assetPathProp.SetValue(assetOriginInstance, assetPath); // will be overridden by Unity
                if (uploadIdProp != null) uploadIdProp.SetValue(assetOriginInstance, uploadId);

                return assetOriginInstance;
            }

            return null;
        }

        public override string ToString()
        {
            return $"Asset Origin '{packageName}' ({packageVersion}, id: {productId})";
        }
    }
}
