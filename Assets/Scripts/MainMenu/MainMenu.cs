using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsMenu;
    public GameObject multiplayerMenu;

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
        }
    }

    public void OnOptionsButton()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        optionsMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnQuitButton()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        Application.Quit();
    }
}