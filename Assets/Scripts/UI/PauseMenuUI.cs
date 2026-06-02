using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsMenu;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    private bool isPaused = false;
    public PlayerInputController Controls { get; set; }
    
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
        Controls.SetUIMode(true);
        Controls.OnCancel += TogglePause;
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
        Controls.SetUIMode(false);
        Controls.OnCancel -= TogglePause;
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