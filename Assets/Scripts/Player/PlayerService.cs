using UnityEngine;

public class PlayerService
{
    public PlayerController PlayerController { get; private set; }
    public PlayerService(PlayerSO playerSO)
    {
        PlayerController = new PlayerController(playerSO);
    }
    public float GetCurrentHealth() => PlayerController.GetCurrentHealth();
    public void TogglePlayerView(bool active) => PlayerController.TogglePlayerView(active);
    public void ResetPlayer() => PlayerController.ResetPlayer();
}
