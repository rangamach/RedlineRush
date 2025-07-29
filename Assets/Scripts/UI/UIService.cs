using UnityEngine;
using UnityEngine.UI;

public class UIService : MonoBehaviour
{
    [Header("Start Menu UI")]
    [SerializeField] private RectTransform startMenuUI;
    [SerializeField] private Button playButtonSM;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button informationButton;
    [SerializeField] private Button quitButton;

    [Header("Gameplay UI")]
    [SerializeField] private RectTransform gameplayUI;
    [SerializeField] private Image fillImage;
    [SerializeField] private float healthBarSpeed;

    [Header("Gameover UI")]
    [SerializeField] private RectTransform gameoverUI;
    [SerializeField] private Button playButtonGO;

    private void Awake()
    {
        DisableAllUIs();

        playButtonSM.onClick.AddListener(onPlayButtonClickedSM);
        playButtonGO.onClick.AddListener(onPlayButtonClickedGO);
    }

    private void Update()
    {
        UpdateCurrentUI();
    }

    private void UpdateCurrentUI()
    {
        switch(GameService.Instance.GameState)
        {
            case GameState.Startmenu:
                if(!startMenuUI.gameObject.activeInHierarchy)
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
            case GameState.Gameover:
                if(!gameoverUI.gameObject.activeInHierarchy)
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
        gameoverUI.gameObject.SetActive(false);
    }
    private void TogglePlayerView(bool active) => GameService.Instance.PlayerService.TogglePlayerView(active);

    #region Start UI
    private void onPlayButtonClickedSM()
    {
        GameService.Instance.SetGameState(GameState.Gameplay);
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
