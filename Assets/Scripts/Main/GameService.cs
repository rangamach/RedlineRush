using UnityEngine;

public class GameService : GenericMonoSingleton<GameService>
{
    //Services:
    public PlayerService PlayerService { get; private set; }

    //Scriptable Objects:
    [SerializeField] private PlayerSO playerSO;

    private void Awake()
    {
        PlayerService = new PlayerService(playerSO);
    }
}
