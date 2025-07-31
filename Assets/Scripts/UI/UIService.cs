using UnityEngine;
using UnityEngine.UI;

public class UIService : MonoBehaviour
{
    [Header("Start Menu UI")]
    [SerializeField] private RectTransform startMenuUI;
    [SerializeField] private RectTransform mainmenuUI;
    [SerializeField] private RectTransform informationUI;
    [SerializeField] private RectTransform helpUI;
    [SerializeField] private Button playButtonSM;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button informationButton;
    [SerializeField] private Button quitButton;


    [Header("Gameplay UI")]
    [SerializeField] private RectTransform gameplayUI;
    [SerializeField] private Image fillImage;
    [SerializeField] private float healthBarSpeed;

    [Header("Gamepaused UI")]
    [SerializeField] private RectTransform gamepausedUI;
    [SerializeField] private Button exitButton;

    [Header("Gameover UI")]
    [SerializeField] private RectTransform gameoverUI;
    [SerializeField] private Button playButtonGO;
    [SerializeField] private Button exitButtonGO;

    private void Awake()
    {
        DisableAllUIs();

        playButtonSM.onClick.AddListener(onPlayButtonClickedSM);
        playButtonGO.onClick.AddListener(onPlayButtonClickedGO);

        informationButton.onClick.AddListener(onInformationButtonClicked);
        helpButton.onClick.AddListener(onHelpButtonClicked);
        quitButton.onClick.AddListener(onQuitButtonClickedSM);

        exitButton.onClick.AddListener(onExitButtonClicked);

        playButtonGO.onClick.AddListener(onPlayButtonClickedGO);
        exitButtonGO.onClick.AddListener(onExitButtonClicked);
    }

    private void Update()
    {
        UpdateCurrentUI();
    }

    private void UpdateCurrentUI()
    {
        switch (GameService.Instance.GameState)
        {
            case GameState.Startmenu:
                if (!startMenuUI.gameObject.activeInHierarchy)
                {
                    GameService.Instance.SoundService.MuteNonBGM(true);
                    DisableAllUIs();
                    startMenuUI.gameObject.SetActive(true);
                    TogglePlayerView(false);
                }
                break;
            case GameState.Gameplay:
                if (!gameplayUI.gameObject.activeInHierarchy)
                {
                    GameService.Instance.SoundService.MuteNonBGM(false);
                    DisableAllUIs();
                    TogglePlayerView(true);
                    gameplayUI.gameObject.SetActive(true);
                }
                UpdateHealthBarUI();
                break;
            case GameState.Gamepaused:
                DisableAllUIs();
                gamepausedUI.gameObject.SetActive(true);
                break;
            case GameState.Gameover:
                if (!gameoverUI.gameObject.activeInHierarchy)
                {
                    GameService.Instance.SoundService.MuteNonBGM(true);
                    DisableAllUIs();
                    TogglePlayerView(false);
                    gameoverUI.gameObject.SetActive(true);
                }
                break;
        }
    }
    private void DisableAllUIs()
    {
        startMenuUI.gameObject.SetActive(false);
        gameplayUI.gameObject.SetActive(false);
        gamepausedUI.gameObject.SetActive(false);
        gameoverUI.gameObject.SetActive(false);
    }
    private void TogglePlayerView(bool active) => GameService.Instance.PlayerService.TogglePlayerView(active);
    private void StartCarEngine()
    {
        float delay = GameService.Instance.SoundService.GetAudioClipLength(SoundTypes.CarStart);
        GameService.Instance.SoundService.StartCarEngine();
        Invoke(nameof(StartEngineLoop), delay - 0.5f);
    }

    #region Start UI
    private void onPlayButtonClickedSM()
    {
        GameService.Instance.SoundService.PlaySFXMusic(SoundTypes.ButtonClick);
        GameService.Instance.SetGameState(GameState.Gameplay);

        StartCarEngine();
    }
    private void StartEngineLoop() => GameService.Instance.SoundService.StartCarEngineLoop();
    private void onInformationButtonClicked()
    {
        GameService.Instance.SoundService.PlaySFXMusic(SoundTypes.ButtonClick);
        mainmenuUI.gameObject.SetActive(false);
        informationUI.gameObject.SetActive(true);
    }
    private void onHelpButtonClicked()
    {
        GameService.Instance.SoundService.PlaySFXMusic(SoundTypes.ButtonClick);
        mainmenuUI.gameObject.SetActive(false);
        helpUI.gameObject.SetActive(true);
    }
    private void onQuitButtonClickedSM()
    {
        GameService.Instance.SoundService.PlaySFXMusic(SoundTypes.ButtonClick);
        if (mainmenuUI.gameObject.activeInHierarchy)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else if(informationUI.gameObject.activeInHierarchy || helpUI.gameObject.activeInHierarchy)
        {
            helpUI.gameObject.SetActive(false);
            informationUI.gameObject.SetActive(false);
            mainmenuUI.gameObject.SetActive(true);
        }
    }
#endregion

    #region Gameplay UI
    private void UpdateHealthBarUI()
    {
        float health = GameService.Instance.PlayerService.GetCurrentHealth() / 100;

        if (fillImage.fillAmount != health)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, health, healthBarSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Gamepaused UI
    private void onExitButtonClicked()
    {
        GameService.Instance.SoundService.PlaySFXMusic(SoundTypes.ButtonClick);
        GameService.Instance.PlayerService.ResetPlayer();
        GameService.Instance.SetGameState(GameState.Startmenu);
        if (GameService.Instance.GameState == GameState.Gamepaused)
        {
            Time.timeScale = 1f;
        }
    }
    #endregion

    #region Gameover UI
    private void onPlayButtonClickedGO()
    {
        GameService.Instance.SoundService.PlaySFXMusic(SoundTypes.ButtonClick);
        GameService.Instance.PlayerService.ResetPlayer();
        GameService.Instance.SetGameState(GameState.Gameplay);

        StartCarEngine();
    }
    #endregion
}
