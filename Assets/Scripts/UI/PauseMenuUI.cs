using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : NetworkBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsMenu;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;

    public PlayerInputController Controls { get; set; }

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsMenu != null)
            optionsMenu.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (KornnGameManager.Instance != null)
        {
            KornnGameManager.Instance.IsPaused.OnValueChanged += OnPauseChanged;
            KornnGameManager.Instance.OnGameEnded += ForceUnpause;

            ApplyPauseState(KornnGameManager.Instance.IsPaused.Value);
        }
    }

    public void TogglePause()
    {
        if (KornnGameManager.Instance == null) return;

        SetPausedServerRpc(!KornnGameManager.Instance.IsPaused.Value);
    }

    public void Resume()
    {
        SetPausedServerRpc(false);
    }

    public void OpenSettings()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsMenu != null)
            optionsMenu.SetActive(true);
    }

    private void ForceUnpause()
    {
        SetPausedServerRpc(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPausedServerRpc(false);
    }

    private void OnPauseChanged(bool oldValue, bool newValue)
    {
        ApplyPauseState(newValue);
    }

    private void ApplyPauseState(bool paused)
    {
        if (pausePanel != null)
            pausePanel.SetActive(paused);

        if (!paused && optionsMenu != null)
            optionsMenu.SetActive(false);

        Time.timeScale = paused ? 0f : 1f;

        if (Controls != null && Controls.IsOwner)
        {
            Controls.SetUIMode(paused, resumeButton.gameObject);

            Controls.OnCancel -= TogglePause;

            if (paused)
                Controls.OnCancel += TogglePause;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPausedServerRpc(bool paused)
    {
        if (KornnGameManager.Instance == null) return;

        KornnGameManager.Instance.IsPaused.Value = paused;
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(Resume);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OpenSettings);

        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (KornnGameManager.Instance != null)
        {
            KornnGameManager.Instance.IsPaused.OnValueChanged -= OnPauseChanged;
            KornnGameManager.Instance.OnGameEnded -= ForceUnpause;
        }

        if (Controls != null)
            Controls.OnCancel -= TogglePause;
    }
}