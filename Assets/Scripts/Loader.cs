using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    private static Scene targetScene;

    private static LoadType loadType;

    public enum Scene
    {
        Loading,
        GameNetCodeTest,
        MainMenu,
        AuthBootstrap,
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
        loadType = LoadType.None;

        SceneManager.LoadScene(Scene.Loading.ToString());
    }

    /// <summary>
    /// Called to load the client in dedicated server.
    /// </summary>
    /// <param name="scene"> Scene to go to</param>
    public static void LoadClient()
    {
        loadType = LoadType.Client;

        SceneManager.LoadScene(Scene.Loading.ToString());
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

        NetworkManager.Singleton.SceneManager.LoadScene(Scene.Loading.ToString(), LoadSceneMode.Single);
    }

    public static void LoadCallback()
    {
        switch(loadType)
        {
            case LoadType.None:
                SceneManager.LoadScene(targetScene.ToString());
                break;
            case LoadType.Client:
                ClientSingleton.Instance.GameManager.ConnectClient(); //Connect client in Loading Scene
                break;
            case LoadType.Host:
                NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
                break;
        } 
    }


}
