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
    public void TogglePlayerView(bool active) => PlayerView.enabled = active;
    public void ResetPlayer()
    {
        PlayerView.ResetPlayer();
        PlayerModel.SetCurrentHealth(100);
    }
    public void TakeDamage(float damage)
    {
        float health = PlayerModel.CurrentHealth - damage;

        if (health <= 0)
        {
            GameService.Instance.EventService.OnPlayerDeath.InvokeEvent();
        }
        else
        {
            PlayerModel.SetCurrentHealth(health);
        }
    }

    public float GetCurrentHealth() => PlayerModel.CurrentHealth;
}
