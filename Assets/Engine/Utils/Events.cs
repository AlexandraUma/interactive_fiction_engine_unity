public enum EventType
{
    PROLOGUE,
    PLAYER_INPUT,
    ROOM_NAME,
    ROOM_DESCRIPTION,
    WORLD_RESPONSE,
    INTERPRETER_OUTPUT,
    UNKNOWN_EVENT,
    INVALID_COMMAND,
    END_GAME,
    INTRO,
    CALL_TO_ACTION,
    END_MESSAGE
}

/* An event is something that happens within a game. */
public class IFEvent
{
    public EventType eventType;
    public string eventText;

    public IFEvent(EventType type, string text)
    {
        eventType = type;
        eventText = text;
    }

    public override string ToString() => $"{eventType}: {eventText}";
}
