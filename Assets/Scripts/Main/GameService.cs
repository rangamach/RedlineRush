using UnityEngine;

public class GameService : GenericMonoSingleton<GameService>
{
    //Services:
    public PlayerService PlayerService { get; private set; }

    [SerializeField] private UIService uiService;
    public UIService UIService() => uiService;

    //Scriptable Objects:
    [SerializeField] private PlayerSO playerSO;

    //Variables:
    public GameState GameState { get; private set; }

    private void Awake()
    {
        SetGameState(GameState.Gameplay);

        PlayerService = new PlayerService(playerSO);
    }
    public void SetGameState(GameState state) => this.GameState = state;
}
public enum GameState
{
    Gameplay,
}
