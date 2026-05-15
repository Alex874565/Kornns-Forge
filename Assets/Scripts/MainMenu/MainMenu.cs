using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsMenu;

    private void Start()
    {
        SoundManager.PlayMusic(MusicType.BackgroundMusic);
    }
    public void OnPlayButton()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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