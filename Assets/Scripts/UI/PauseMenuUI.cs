using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsMenu;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (KornnGameManager.Instance != null)
            KornnGameManager.Instance.OnGameEnded += ForceUnpause;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isPaused)
            Resume();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        //AudioListener.pause = true;
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        //AudioListener.pause = false;
    }

    private void ForceUnpause()
    {
        if (isPaused)
            Resume();
    }

    void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(Resume);

        if (KornnGameManager.Instance != null)
            KornnGameManager.Instance.OnGameEnded -= ForceUnpause;
    }
}