using System;
using System.Linq;

/// <summary>
/// Helper utilities for working with actions.
/// Mirrors the behaviour of the original Python ActionHelper where useful.
/// </summary>
public static class ActionHelper
{
    /// <summary>
    /// Logs a message via the game controller and returns an action status.
    /// 
    /// By default, the status is SUCCESSFUL and the event type is WORLD_RESPONSE,
    /// so you can just call this for the common "log and succeed" pattern.
    /// </summary>
    /// <param name="gameController">The game controller used for logging.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="status">The action status to return (default: SUCCESSFUL).</param>
    /// <param name="eventType">The event type for the log (default: WORLD_RESPONSE).</param>
    /// <returns>The provided <paramref name="status"/>.</returns>
    public static ActionStatus LogActionAndReturnStatus(
        GameController gameController,
        string message,
        ActionStatus status = ActionStatus.SUCCESSFUL,
        EventType eventType = EventType.WORLD_RESPONSE)
    {
        if (gameController == null)
        {
            throw new ArgumentNullException(nameof(gameController));
        }

        if (!string.IsNullOrEmpty(message))
        {
            gameController.LogEvent(
                eventText: message,
                eventType: eventType
            );
        }

        return status;
    }

    /// <summary>
    /// Gets a text response for a given keyword from a BaseObject.
    /// </summary>
    /// <param name="obj">The BaseObject to get the text response from.</param>
    /// <param name="keyword">The keyword to get the text response for.</param>
    /// <returns>The text response for the given keyword.</returns>
    public static string GetTextResponse(BaseObject obj, string keyword)
    {
        return obj?.GetTextResponseFor(keyword);
    }

    /// <summary>
    /// Gets a restriction message for a given keyword from a BaseObject.
    /// Returns null if there is no restriction for that action on the object.
    /// </summary>
    /// <param name="obj">The BaseObject to check for a restriction.</param>
    /// <param name="keyword">The action keyword to check.</param>
    /// <returns>The restriction message, or null if none is defined.</returns>
    public static string GetRestrictionMessage(BaseObject obj, string keyword)
    {
        if (obj?.actionRestrictions == null)
        {
            return null;
        }

        ActionRestriction match = obj.actionRestrictions.FirstOrDefault(r => r.actionKeyword == keyword);
        if (string.IsNullOrWhiteSpace(match?.message))
        {
            return null;
        }

        return match.message;
    }

}


