using UnityEngine;

public class PlayerService
{
    public PlayerController PlayerController { get; private set; }
    public PlayerService(PlayerSO playerSO)
    {
        PlayerController = new PlayerController(playerSO);
    }
}
