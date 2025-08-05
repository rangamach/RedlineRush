public class EventService
{
    //Events:
    public EventController OnPlayerDeath { get; private set; }

    public EventService()
    {
        OnPlayerDeath = new EventController();
    }
}
