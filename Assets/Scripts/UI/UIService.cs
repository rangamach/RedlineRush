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

    [Header("Gameover UI")]
    [SerializeField] private RectTransform gameoverUI;
    [SerializeField] private Button playButtonGO;

    private void Awake()
    {
        DisableAllUIs();

        playButtonSM.onClick.AddListener(onPlayButtonClickedSM);
        playButtonGO.onClick.AddListener(onPlayButtonClickedGO);

        informationButton.onClick.AddListener(onInformationButtonClicked);
        helpButton.onClick.AddListener(onHelpButtonClicked);
        quitButton.onClick.AddListener(onQuitButtonClickedSM);
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
                    DisableAllUIs();
                    startMenuUI.gameObject.SetActive(true);
                    TogglePlayerView(false);
                }
                break;
            case GameState.Gameplay:
                if (!gameplayUI.gameObject.activeInHierarchy)
                {
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

    #region Start UI
    private void onPlayButtonClickedSM()
    {
        GameService.Instance.SetGameState(GameState.Gameplay);
    }
    private void onInformationButtonClicked()
    {
        mainmenuUI.gameObject.SetActive(false);
        informationUI.gameObject.SetActive(true);
    }
    private void onHelpButtonClicked()
    {
        mainmenuUI.gameObject.SetActive(false);
        helpUI.gameObject.SetActive(true);
    }
    private void onQuitButtonClickedSM()
    {
        if(mainmenuUI.gameObject.activeInHierarchy)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else if(informationUI.gameObject.activeInHierarchy)
        {
            informationUI.gameObject.SetActive(false);
            mainmenuUI.gameObject.SetActive(true);
        }
        else if(helpUI.gameObject.activeInHierarchy)
        {
            helpUI.gameObject.SetActive(false);
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

    #region Gameover UI
    private void onPlayButtonClickedGO()
    {
        GameService.Instance.PlayerService.ResetPlayer();
        GameService.Instance.SetGameState(GameState.Gameplay);
    }
    #endregion
}
