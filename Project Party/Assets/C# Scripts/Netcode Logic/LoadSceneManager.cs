using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class LoadSceneManager : NetworkBehaviour
{

    public void LoadSceneSingleOnServer(string sceneName)
    {
        LoadSceneSingle_ServerRPC(sceneName);
    }
    public void LoadSceneAdditivelyOnServer(string sceneName)
    {
        LoadSceneAdditively_ServerRPC(sceneName);
    }

    [ServerRpc]
    private void LoadSceneSingle_ServerRPC(string sceneName)
    {
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    [ServerRpc]
    private void LoadSceneAdditively_ServerRPC(string sceneName)
    {
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void LoadSceneSingle_ClientSide(string sceneName)
    {
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    public void LoadSceneAdditively_ClientSide(string sceneName)
    {
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
}