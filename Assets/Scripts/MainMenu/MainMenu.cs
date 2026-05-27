using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsMenu;
    public GameObject multiplayerMenu;

    [SerializeField] private GameObject firstMainMenuButton;
    [SerializeField] private GameObject firstMultiplayerButton;
    [SerializeField] private GameObject firstOptionsButton;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstMainMenuButton);
    }

    private void Start()
    {
        SoundManager.PlayMusic(MusicType.BackgroundMusic);
    }
    public void OnPlayButton()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        if (multiplayerMenu != null)
        {
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