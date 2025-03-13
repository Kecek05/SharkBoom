using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public static class AuthenticationWrapper
{

    public static event Action OnSignInFail;

    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    private static string playerName;
    public static string PlayerName => playerName;

    public static string GooglePlayToken;
    public static string GooglePlayError;

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
        if (AuthState == AuthState.Authenticated) return AuthState;

        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticating.");
            await Authenticating();
            return AuthState;
        }

        await SignInAndroidAsync();

        return AuthState;
    }

    private static async Task SignInAndroidAsync()
    {
        AuthState = AuthState.Authenticating;
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);

        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(GooglePlayToken);


        } catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            throw;
        } catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }

    internal static void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("Login with Google was successful");

            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
            {
                Debug.Log($"{code} <-Auth code");
                GooglePlayToken = code;
            });

            Loader.Load(Loader.Scene.MainMenu);
        }
        else
        {
            AuthState = AuthState.Error;
            OnSignInFail?.Invoke();
            GooglePlayError = "Failed to retrive GPG auth code";
            Debug.LogWarning("SignIn failed.");
        }
    }

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


            playerName = await AuthenticationService.Instance.GetPlayerNameAsync();

            Debug.Log(playerName + AuthState);

            Loader.Load(Loader.Scene.MainMenu);

        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
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

                    playerName = await AuthenticationService.Instance.GetPlayerNameAsync();

                    Debug.Log(playerName + AuthState);

                    Loader.Load(Loader.Scene.MainMenu);

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
