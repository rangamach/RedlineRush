using UnityEngine;

public class PlayerModel
{
    private PlayerController playerController;
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    public PlayerModel(PlayerController playerController, PlayerSO so)
    {
        this.playerController = playerController;

        this.MaxHealth = so.MaxHealth;

        SetCurrentHealth(MaxHealth);
    }
    public void SetCurrentHealth(float health) => this.CurrentHealth = health;
}
