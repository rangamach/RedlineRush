using UnityEngine;

public class GameService : GenericMonoSingleton<GameService>
{
    //Services:
    public PlayerService PlayerService { get; private set; }
    public EventService EventService { get; private set; }

    [SerializeField] private UIService uiService;
    public UIService UIService() => uiService;

    //Scriptable Objects:
    [SerializeField] private PlayerSO playerSO;

    //Variables:
    public GameState GameState { get; private set; }

    private void Awake()
    {
        base.Awake();

        SetGameState(GameState.Startmenu);

        CreateServices();
    }
    private void CreateServices()
    {
        PlayerService = new PlayerService(playerSO);
        EventService = new EventService();
    }
    public void SetGameState(GameState state) => this.GameState = state;
}
public enum GameState
{
    Startmenu,
    Gameplay,
    Gamepaused,
    Gameover,
}
