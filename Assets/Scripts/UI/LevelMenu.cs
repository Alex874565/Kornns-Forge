using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class LevelMenu : NetworkBehaviour
{
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private LevelStarDisplay[] starDisplays;


    private void Start()
    {
        RefreshButtons();
        RefreshStars();
    }


    private void RefreshButtons()
    {
        int unlockedLevel =
            PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable =
                (i + 1) <= unlockedLevel;
        }
    }


    private void RefreshStars()
    {
        for (int i = 0; i < starDisplays.Length; i++)
        {
            int stars =
                PlayerPrefs.GetInt(
                    "Level" + (i + 1) + "Stars",
                    0
                );

            starDisplays[i].SetStars(stars);
        }
    }


    public void LoadLevel(string sceneName)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                sceneName,
                LoadSceneMode.Single);
        }
        else
        {
            LoadLevelServerRpc(sceneName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadLevelServerRpc(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single);
    }
}