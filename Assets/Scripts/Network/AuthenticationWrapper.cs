using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

#if UNITY_ANDROID
    using GooglePlayGames;
    using GooglePlayGames.BasicApi;
#endif

public static class AuthenticationWrapper
{

    public static event Action OnSignInFail;

    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static string GooglePlayToken;

    public static async Task<AuthState> DoAuthUnity()
    {
        if (AuthState == AuthState.Authenticated) return AuthState;

        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticating...");
            await Authenticating();
            return AuthState;
        }

        await SignInUnityAsync();

        return AuthState;

    }

    public static async Task<AuthState> DoAuthAnonymously(int maxTries = 5)
    {

        if (AuthState == AuthState.Authenticated) return AuthState;

        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticating.");
            await Authenticating();
            return AuthState;
        }

        PlayerPrefs.DeleteKey("AccessToken");

        await SignInAnonymouslyAsync(maxTries);

        return AuthState;
    }


    public static async Task<AuthState> DoAuthAndroid()
    {
#if UNITY_ANDROID
        if (AuthState == AuthState.Authenticated) return AuthState;

        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticating.");
            await Authenticating();
            return AuthState;
        }

        await SignInAndroidAsync();

        return AuthState;
#endif
        Debug.LogWarning("Trying to authenticate with Android on a non-Android platform.");
        return AuthState.Error; // Not Android and trying to authenticate with Android
    }

#if UNITY_ANDROID
    private static async Task SignInAndroidAsync()
    {
        AuthState = AuthState.Authenticating;
        PlayGamesPlatform.Activate();

        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("Login with Google was successful");

                PlayGamesPlatform.Instance.RequestServerSideAccess(false, code =>
                {
                    Debug.Log($"{code} <-Auth code");
                    GooglePlayToken = code;
                });
            }
            else
            {
                AuthState = AuthState.Error;
                OnSignInFail?.Invoke();
                GooglePlayToken = "Failed to retrive GPG auth code";
                Debug.LogWarning("SignIn failed.");
            }

        });

        if (AuthState == AuthState.Error) return;

        await CallAndroidWithUnity();
    }


    private static async Task CallAndroidWithUnity()
    {
        while(GooglePlayToken == null)
        {
            await Task.Delay(100);
        }

        await AuthAndroidWithUnity();
    }


    private static async Task AuthAndroidWithUnity()
    {

        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(GooglePlayToken);

            AuthState = AuthState.Authenticated;
            Debug.Log($"AUTHENTICATED WITH GOOGLE PLAY GAMES UNITY, CODE: {GooglePlayToken}");
            return;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }

        OnSignInFail?.Invoke();
    }

#endif

    private static async Task SignInUnityAsync()
    {
        AuthState = AuthState.Authenticating;

        //if (PlayerPrefs.HasKey("AccessToken"))
        //{
        //    Debug.Log("PlayerPrefs");
        //    string accessTokenPlayerPrefs = PlayerPrefs.GetString("AccessToken");

        //    await SignInWithUnityAsync(accessTokenPlayerPrefs);

        //}
        //else
        //{

        //    PlayerAccountService.Instance.SignedIn += SignedIn;

        //    PlayerAccountService.Instance.SignInFailed += SignInFailed;

        //    await PlayerAccountService.Instance.StartSignInAsync();

        //}




        PlayerAccountService.Instance.SignedIn += SignedIn;

        PlayerAccountService.Instance.SignInFailed += SignInFailed;

        await PlayerAccountService.Instance.StartSignInAsync();


    }

    private static async Task<AuthState> Authenticating()
    {
        while (AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }

        return AuthState;
    }

    private static void SignInFailed(RequestFailedException exception)
    {
        AuthState = AuthState.Error;
        OnSignInFail?.Invoke();

        Debug.LogWarning("SignIn failed.");
    }

    private static async void SignedIn()
    {
        try
        {
            var accessToken = PlayerAccountService.Instance.AccessToken;
            await SignInWithUnityAsync(accessToken);


        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        PlayerAccountService.Instance.SignedIn -= SignedIn;
    }





    private static async Task SignInWithUnityAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
            Debug.Log("SignIn successfull.");

            PlayerPrefs.SetString("AccessToken", accessToken);

            AuthState = AuthState.Authenticated;

            return;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
        OnSignInFail?.Invoke();
    }

    private static async Task SignInAnonymouslyAsync(int maxTries = 5)
    {
        AuthState = AuthState.Authenticating;

        int tries = 0;

        while (AuthState == AuthState.Authenticating && tries < maxTries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();


                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated;

                    break;
                }

            }
            catch (AuthenticationException authEx)
            {
                Debug.LogError(authEx);
                AuthState = AuthState.Error;
            }
            catch (RequestFailedException requestEx)
            {
                Debug.LogError(requestEx);
                AuthState = AuthState.Error;
            }


            tries++;

            await Task.Delay(1000);
        }

        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"Player could not authenticate after {tries} tries.");
            AuthState = AuthState.TimeOut;
            OnSignInFail?.Invoke();
        }
    }

}



public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut,
}
