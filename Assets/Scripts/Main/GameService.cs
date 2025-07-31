using UnityEngine;

public class GameService : GenericMonoSingleton<GameService>
{
    //Services:
    public PlayerService PlayerService { get; private set; }
    public SoundService SoundService { get; private set; }
    public EventService EventService { get; private set; }

    [SerializeField] private UIService uiService;
    public UIService UIService() => uiService;

    //Scriptable Objects:
    [Header("Scriptable Objects")]
    [SerializeField] private PlayerSO playerSO;
    [SerializeField] private SoundSO soundSO;

    //Variables:
    public GameState GameState { get; private set; }

    //Audio Sources:
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        base.Awake();

        SetGameState(GameState.Startmenu);

        CreateServices();
    }
    private void CreateServices()
    {
        PlayerService = new PlayerService(playerSO);
        SoundService = new SoundService(soundSO,bgSource,sfxSource);
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
