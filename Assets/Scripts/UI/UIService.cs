using System.Runtime.InteropServices;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI bestTimeSM;


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
    [SerializeField] private TextMeshProUGUI bestTimeTextGO;
    [SerializeField] private TextMeshProUGUI timerTextGO;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    private float timeElapsed;
    private bool isRunning;
    private string Best = "best";

    private void Start()
    {
        ResetTimer();
    }

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

        if(isRunning)
        {
            timeElapsed += Time.deltaTime;
            timerText.text = FormatTimerText(timeElapsed);
        }
    }
    private void UpdateCurrentUI()
    {
        switch (GameService.Instance.GameState)
        {
            case GameState.Startmenu:
                if (!startMenuUI.gameObject.activeInHierarchy)
                {
                    GameService.Instance.SoundService.NonBGMAudios(true);
                    DisplayBestTimeAtStart();
                    DisableAllUIs();
                    startMenuUI.gameObject.SetActive(true);
                    TogglePlayerView(false);
                }
                break;
            case GameState.Gameplay:
                if (!gameplayUI.gameObject.activeInHierarchy)
                {
                    if(gamepausedUI.gameObject.activeInHierarchy)
                    {
                        ToggleTimer(true);
                    }
                    else
                    {
                        ResetTimer();
                        ToggleTimer(true);
                    }
                    DisableAllUIs();
                    TogglePlayerView(true);
                    gameplayUI.gameObject.SetActive(true);
                }
                UpdateHealthBarUI();
                break;
            case GameState.Gamepaused:
                ToggleTimer(false);
                DisableAllUIs();
                gamepausedUI.gameObject.SetActive(true);
                break;
            case GameState.Gameover:
                if (!gameoverUI.gameObject.activeInHierarchy)
                {
                    ToggleTimer(false);
                    GameService.Instance.SoundService.NonBGMAudios(true);
                    DisableAllUIs();
                    TogglePlayerView(false);
                    SaveBestTime();
                    DisplayBestTime();
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
    private bool HasFinishedRace() => GameService.Instance.PlayerService.GetFinishedRace();

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
    private void DisplayBestTimeAtStart() => bestTimeSM.text = FormatTimerText(GetBestTime());
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
        if (GameService.Instance.GameState == GameState.Gamepaused)
        {
            Time.timeScale = 1f;
        }
        GameService.Instance.SetGameState(GameState.Startmenu);
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
    private void DisplayBestTime()
    {
        float bestTime = GetBestTime();

        if(bestTime > 0 && timeElapsed < GetBestTime())
        {
            bestTimeTextGO.text = "New Best Time";
        }
        else
        {
            bestTimeTextGO.text = "Best Time";
        }
        timerTextGO.text = FormatTimerText(bestTime);
    }
    #endregion

    #region Timer
    private void ToggleTimer(bool toggle) => isRunning = toggle;
    private string FormatTimerText(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600);
        int minutes = Mathf.FloorToInt((time % 3600) / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        if(hours > 0)
        {
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
        else
        {
            return $"{minutes:00}:{seconds:00}";
        }
    }
    private void ResetTimer()
    {
        timeElapsed = 0;
        timerText.text = FormatTimerText(timeElapsed);
    }
    private void SaveBestTime()
    {
        float bestTime = GetBestTime();

        if(HasFinishedRace() && FinishedLapAlive())
        {
            PlayCheer();
            if (IsNewBestTime())
            {
                PlayerPrefs.SetFloat(Best, timeElapsed);
                PlayerPrefs.Save();
            }
        }
    }
    private float GetBestTime()
    {
        return PlayerPrefs.HasKey(Best) ? PlayerPrefs.GetFloat(Best) : 0f;
    }
    private bool FinishedLapAlive() => GameService.Instance.PlayerService.GetCurrentHealth() > 0;
    private bool IsNewBestTime()
    {
        float time = GetBestTime();

        return time > 0 ? timeElapsed < time : true;
    }
    private void PlayCheer()
    {
        if (FinishedLapAlive())
        {
            GameService.Instance.SoundService.PlaySFXMusic(SoundTypes.Cheer);
        }
    }
    #endregion
}
