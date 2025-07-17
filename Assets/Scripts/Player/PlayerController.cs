using UnityEngine;

public class PlayerController
{
    public PlayerView1 PlayerView { get; private set; }
    public PlayerModel PlayerModel { get; private set; }
    public PlayerController(PlayerSO playerSO)
    {
        PlayerView = playerSO.PlayerView;
        PlayerView.SetController(this);
        PlayerModel = new PlayerModel(this);
    }
}
