using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuUI : NetworkBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsMenu;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;

    [Header("First Selected Buttons")]
    [SerializeField] private GameObject firstPauseButton;
    [SerializeField] private GameObject firstOptionsButton;

    public PlayerInputController Controls { get; set; }

    [SerializeField] private string levelSelectScene = "LevelSelect";

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

    public void OpenSettings()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsMenu != null)
            optionsMenu.SetActive(true);

        SelectButton(firstOptionsButton);

        if (Controls != null && Controls.IsOwner)
            Controls.SetUIMode(true, firstOptionsButton);
    }

    private void ApplyPauseState(bool paused)
    {
        if (KornnGameManager.Instance.TutorialOn.Value) return;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        if (!paused && optionsMenu != null)
            optionsMenu.SetActive(false);

        Time.timeScale = paused ? 0f : 1f;

        if (Controls != null && Controls.IsOwner)
        {
            GameObject selectedButton = paused
                ? (firstPauseButton != null ? firstPauseButton : resumeButton.gameObject)
                : null;

            Controls.SetUIMode(paused, selectedButton);

            if (paused)
                SelectButton(selectedButton);

            Controls.OnCancel -= TogglePause;

            if (paused)
                Controls.OnCancel += TogglePause;
        }
    }

    private void SelectButton(GameObject button)
    {
        if (button == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }

    public void ReturnToLevelSelect()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);
        ReturnToLevelSelectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReturnToLevelSelectServerRpc()
    {
        if (KornnGameManager.Instance != null)
            KornnGameManager.Instance.IsPaused.Value = false;

        NetworkManager.Singleton.SceneManager.LoadScene(
            levelSelectScene,
            LoadSceneMode.Single
        );
    }

    public void TogglePause()
    {
        if (KornnGameManager.Instance == null) return;

        SetPausedServerRpc(!KornnGameManager.Instance.IsPaused.Value);
    }

    public void Resume()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);
        SetPausedServerRpc(false);
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

    [ServerRpc(RequireOwnership = false)]
    private void SetPausedServerRpc(bool paused)
    {
        if (KornnGameManager.Instance == null) return;
        if (KornnGameManager.Instance.TutorialOn.Value) return;

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

    public void CloseSettings()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        optionsMenu.SetActive(false);
        pausePanel.SetActive(true);

        SelectButton(firstPauseButton);

        if (Controls != null && Controls.IsOwner)
            Controls.SetUIMode(true, firstPauseButton);
    }
}