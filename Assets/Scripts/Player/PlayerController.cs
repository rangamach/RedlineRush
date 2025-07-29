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
    //~PlayerController()
    //{
    //    unSunscribeToEvents();
    //}
    public void TogglePlayerView(bool active) => PlayerView.enabled = active;
    public void ResetPlayer()
    {
        PlayerView.ResetPlayer();
        PlayerModel.SetCurrentHealth(100);
    }
    //private void subscribeToEvents()
    //{
    //    GameService.Instance.EventService.OnPlayerDeath.AddListener(onPlayerDeath);
    //}
    //private void unSunscribeToEvents()
    //{
    //    GameService.Instance.EventService.OnPlayerDeath.RemoveListener(onPlayerDeath);
    //}
    //private void onPlayerDeath()
    //{

    //}
    public void TakeDamage(float damage)
    {
        float health = PlayerModel.CurrentHealth - damage;

        if (health <= 0)
        {
            GameService.Instance.SetGameState(GameState.Gameover);
            PlayerView.PlayerDied();
        }
        else
        {
            PlayerModel.SetCurrentHealth(health);
        }
    }

    public float GetCurrentHealth() => PlayerModel.CurrentHealth;
}
