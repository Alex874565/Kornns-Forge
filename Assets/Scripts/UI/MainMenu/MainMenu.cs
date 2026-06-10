using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    public static bool OpenMultiplayerOnLoad;
    
    public GameObject optionsMenu;
    public GameObject multiplayerMenu;

    [SerializeField] private GameObject firstMainMenuButton;
    [SerializeField] private GameObject firstMultiplayerButton;
    [SerializeField] private GameObject firstOptionsButton;
    
    private void OnEnable()
    {
        if (OpenMultiplayerOnLoad)
        {
            OpenMultiplayerOnLoad = false;

            multiplayerMenu.SetActive(true);
            gameObject.SetActive(false);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMultiplayerButton);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMainMenuButton);
        }
    }

    private void Start()
    {
        SoundManager.PlayMusic(MusicType.BackgroundMusic);
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        OpenMultiplayerOnLoad = false;
    }
    
    public void OnPlayButton()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        if (multiplayerMenu != null)
        {
            OpenMultiplayerOnLoad = true;
            multiplayerMenu.SetActive(true);
            gameObject.SetActive(false);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMultiplayerButton);
        }
    }

    public void OnOptionsButton()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        optionsMenu.SetActive(true);
        gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstOptionsButton);
    }

    public void OnQuitButton()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        Application.Quit();
    }
}