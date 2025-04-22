// adapted from https://docs.unity3d.com/Packages/com.unity.cloud.assets@1.2/manual/get-started-management.html
#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
using System;
using System.Threading.Tasks;
using Unity.Cloud.AppLinking.Runtime;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Common.Runtime;
using Unity.Cloud.Identity;
using Unity.Cloud.Identity.Editor;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public static class PlatformServices
    {
        private const double AUTH_TIMEOUT = 30;

        /// <summary>
        /// Returns a <see cref="ICompositeAuthenticator"/>.
        /// </summary>
        public static UnityEditorServiceAuthorizer Authenticator { get; private set; }

        /// <summary>
        /// Returns an <see cref="IOrganizationRepository"/>.
        /// </summary>
        public static IOrganizationRepository OrganizationRepository => Authenticator;

        /// <summary>
        /// Returns an <see cref="IAssetRepository"/>.
        /// </summary>
        public static IAssetRepository AssetRepository { get; private set; }

        private static void Create()
        {
            UnityHttpClient httpClient = new UnityHttpClient();
            IServiceHostResolver serviceHostResolver = UnityRuntimeServiceHostResolverFactory.Create();
            UnityCloudPlayerSettings playerSettings = UnityCloudPlayerSettings.Instance;
            ServiceHttpClient serviceHttpClient = new ServiceHttpClient(httpClient, Authenticator, playerSettings);

            AssetRepository = AssetRepositoryFactory.Create(serviceHttpClient, serviceHostResolver);
        }

        public static async Task InitOnDemand()
        {
            CloudAssetManagement.IncBusyCount();
            if (AssetRepository == null || Authenticator == null)
            {
                if (Authenticator == null)
                {
                    // login will happen automatically upon instance creation
                    Authenticator = UnityEditorServiceAuthorizer.instance;
                    DateTime startTime = DateTime.Now;
                    while (Authenticator.AuthenticationState != AuthenticationState.LoggedIn && DateTime.Now - startTime < TimeSpan.FromSeconds(AUTH_TIMEOUT))
                    {
                        await Task.Delay(100);
                    }
                }
                if (Authenticator.AuthenticationState == AuthenticationState.LoggedIn)
                {
                    Create();
                }
                else if (Authenticator.AuthenticationState == AuthenticationState.AwaitingLogout)
                {
                    Debug.LogError("Please log into your Unity Cloud account to be able to sync from Asset Manager.");
                    CloudProjectSettings.ShowLogin();
                }
                else
                {
                    Debug.LogError("Could not log into Unity Cloud. Please try again.");
                }
            }
            else if (Authenticator.AuthenticationState == AuthenticationState.LoggedOut)
            {
                Debug.LogError("Not logged into Unity Cloud. Please log in to use the Asset Manager sync feature.");
            }
            CloudAssetManagement.DecBusyCount();
        }
    }
}
#endif
