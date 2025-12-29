/**************************************************
Uses the Text Formatter to format text as an event.
**************************************************/
using UnityEngine;

public static class TextFormatter
{

    public static string FormatEvent(IFEvent gameEvent)
    {
        string formattedString;
        string eventText = gameEvent.eventText;

        switch (gameEvent.eventType)
        {
            case EventType.PROLOGUE:
                formattedString = PrologueAsEvent(eventText);
                break;

            case EventType.PLAYER_INPUT:
                formattedString = PlayerInputAsEvent(eventText);
                break;

            case EventType.ROOM_NAME:
                formattedString = RoomNameAsEvent(eventText);
                break;

            case EventType.UNKNOWN_EVENT:
                Debug.LogWarning($"Unknown event type <{gameEvent.eventType}>. Using default text formatter.");
                formattedString = TextAsDefaultEvent(eventText);
                break;

            // all other events
            default:
                Debug.Log($"Using default text formatter for event type <{gameEvent.eventType}>");
                formattedString = TextAsDefaultEvent(eventText);
                break;
        }

        return formattedString;

    }

    /***************** Formatting Functions *****************/
    public static string Bold(string text) =>
        $"<b>{text}</b>";

    public static string Italic(string text) =>
        $"<i>{text}</i>";

    public static string Color(string text, string colorCode) =>
        $"<color={colorCode}>{text}</color>";

    public static string Size(string text, int size) =>
        $"<size={size}>{text}</size>";

    public static string Underline(string text) =>
        $"<u>{text}</u>";


    /***************** Formatting Functions *****************/
    private static string PrologueAsEvent(string prologue)
    {
        return $"{TextFormatter.Italic(prologue)}\n\n";
    }

    private static string PlayerInputAsEvent(string playerInput)
    {
        return $"> {TextFormatter.Bold(playerInput)}\n";
    }

    private static string TextAsDefaultEvent(string event_text)
    {
        return $"{event_text}\n\n";
    }

    private static string RoomNameAsEvent(string roomName)
    {
        return $"{TextFormatter.Bold(roomName)}\n";
    }
}