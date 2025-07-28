using UnityEngine;

public class PlayerController
{
    public PlayerView1 PlayerView { get; private set; }
    public PlayerModel PlayerModel { get; private set; }
    public PlayerController(PlayerSO playerSO)
    {
        PlayerModel = new PlayerModel(this, playerSO);
        PlayerView = playerSO.PlayerView.Spawn(playerSO.positon,playerSO.rotation,playerSO.scale);

        PlayerView.SetController(this);
    }
}
