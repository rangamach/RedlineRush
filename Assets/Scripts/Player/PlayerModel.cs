using UnityEngine;

public class PlayerModel
{
    private PlayerController playerController;

    public float CurrentHealth { get; private set; }
    public PlayerModel(PlayerController playerController, PlayerSO so)
    {
        this.playerController = playerController;
        this.CurrentHealth = so.MaxHealth;
    }
}
