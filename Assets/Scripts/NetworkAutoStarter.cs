using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkAutoStarter : MonoBehaviour
{
    public static bool ShouldAutoStartHost = false;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ShouldAutoStartHost)
        {
            ShouldAutoStartHost = false;
            StartCoroutine(StartHostAfterDelay());
        }
    }

    private IEnumerator StartHostAfterDelay()
    {
        // Wait one frame to ensure NetworkManager is ready
        yield return null;

        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host automatically restarted after scene reload.");
        }
        else
        {
            Debug.LogWarning("NetworkManager is already listening or missing – auto-start failed.");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}