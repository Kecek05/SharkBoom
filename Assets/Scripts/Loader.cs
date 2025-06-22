using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public static event Action<Scene> OnCurrentSceneChanged;

    private static Scene targetScene;

    private static Scene currentScene;

    private static LoadType loadType;
    public static Scene CurrentScene => currentScene;

    public enum Scene
    {
        Loading,
        GameNetCodeTest,
        MainMenu,
        AuthBootstrap,
        SaveBootstrap,
        NameBootstrap,
        GameTutorial,
    }

    public enum LoadType
    {
        None,
        Client,
        Host,
        DS,
    }

    /// <summary>
    /// Called to load a scene.
    /// </summary>
    /// <param name="scene"> Scene to go to</param>
    public static void Load(Scene scene)
    {
        targetScene = scene;
        currentScene = Scene.Loading;

        loadType = LoadType.None;

        SceneManager.LoadScene(Scene.Loading.ToString());
        OnCurrentSceneChanged?.Invoke(currentScene);
    }

    /// <summary>
    /// Called to load the client in dedicated server.
    /// </summary>
    /// <param name="scene"> Scene to go to</param>
    public static void LoadClient()
    {
        loadType = LoadType.Client;
        currentScene = Scene.Loading;

        SceneManager.LoadScene(Scene.Loading.ToString());
        OnCurrentSceneChanged?.Invoke(currentScene);
    }

    /// <summary>
    /// Called to load the dedicated server.
    /// </summary>
    /// <param name="scene"> Scene to go to</param>
    /// <returns></returns>
    public static AsyncOperation LoadDSAsync(Scene scene)
    {
        //Dedicated Server
        loadType = LoadType.DS;

        return SceneManager.LoadSceneAsync(scene.ToString());
    }


    /// <summary>
    /// Called from host to load the scene.
    /// </summary>
    /// <param name="scene"> Scene to go to</param>
    public static void LoadHostNetwork(Scene scene)
    {
        loadType = LoadType.Host;
        targetScene = scene;
        currentScene = Scene.Loading;

        NetworkManager.Singleton.SceneManager.LoadScene(Scene.Loading.ToString(), LoadSceneMode.Single);
        OnCurrentSceneChanged?.Invoke(currentScene);
    }

    [Command("loadScene")]
    public static void LoadNoLoadingScreen(Scene scene)
    {
        loadType = LoadType.None;
        currentScene = scene;

        SceneManager.LoadScene(scene.ToString());
        OnCurrentSceneChanged?.Invoke(currentScene);
    }

    public static void LoadCallback()
    {
        switch(loadType)
        {
            case LoadType.None:
                SceneManager.LoadScene(targetScene.ToString());
                currentScene = targetScene;
                break;
            case LoadType.Client:
                Debug.Log($"Load Callback Client Connect");
                ClientSingleton.Instance.GameManager.ConnectClient(); //Connect client in Loading Scene
                currentScene = Scene.GameNetCodeTest;
                break;
            case LoadType.Host:
                NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
                currentScene = targetScene;
                break;
        }

        OnCurrentSceneChanged?.Invoke(currentScene);
    }


}
