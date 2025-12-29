using System;
using System.Collections.Generic;

/// <summary>
/// This class manages the input actions, and any restrictions on these actions.
/// </summary>
public class ActionsManager
{
    private readonly Dictionary<string, Action> actions = new();

    // Global per-action restrictions that apply before any room or item logic.
    // Case-insensitive on the action keyword for robustness.
    private readonly Dictionary<string, string> globalRestrictions =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initialize the input action manager and populate input actions.
    /// All actions are expected to be valid and fully configured; if not,
    /// this constructor throws so configuration bugs are caught in testing.
    /// </summary>
    /// <param name="allActions">List of all actions to manage.</param>
    public ActionsManager(List<Action> allActions)
    {
        if (allActions == null)
        {
            throw new ArgumentNullException(nameof(allActions));
        }

        PopulateInputActions(allActions);
    }

    private void PopulateInputActions(List<Action> actions)
    {
        foreach (Action action in actions)
        {
            if (action == null)
            {
                throw new ArgumentException("Action list contains a null entry.", nameof(actions));
            }

            if (string.IsNullOrWhiteSpace(action.Keyword))
            {
                throw new ArgumentException("All actions must define a non-empty Keyword.", nameof(actions));
            }

            this.actions[action.Keyword.ToLower()] = action;

            if (action.Aliases == null)
            {
                continue;
            }

            foreach (string alias in action.Aliases)
            {
                if (string.IsNullOrWhiteSpace(alias))
                {
                    throw new ArgumentException(
                        $"Action '{action.Keyword}' defines an empty or whitespace alias.",
                        nameof(actions));
                }

                this.actions[alias.ToLower()] = action;
            }
        }
    }

    /// <summary>
    /// Return the action associated with the given keyword, or null if not found.
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <returns>The action if found, null otherwise.</returns>
    public Action GetAction(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return null;
        }

        return actions.TryGetValue(keyword.ToLower(), out Action action) ? action : null;
    }

    /// <summary>
    /// Add a global restriction to the input action.
    /// </summary>
    /// <param name="actionKeyword">The action keyword to restrict.</param>
    /// <param name="restrictionMessage">The restriction message to display.</param>
    public void AddGlobalRestriction(string actionKeyword, string restrictionMessage)
    {
        if (string.IsNullOrWhiteSpace(actionKeyword))
        {
            throw new ArgumentException("Action keyword must be non-empty.", nameof(actionKeyword));
        }

        // Normalise the key and update or remove the restriction.
        if (string.IsNullOrWhiteSpace(restrictionMessage))
        {
            globalRestrictions.Remove(actionKeyword);
            return;
        }

        globalRestrictions[actionKeyword] = restrictionMessage;
    }

    /// <summary>
    /// Return the restriction message for the given input action, or
    /// null if no restriction applies.
    /// </summary>
    /// <param name="controller">The game controller.</param>
    /// <param name="action">The action to check.</param>
    /// <param name="item">The item to check (can be null).</param>
    /// <returns>Restriction message if action cannot be performed, null otherwise.</returns>
    public string GetRestrictionMessage(GameController controller, Action action, BaseObject item)
    {
        if (controller == null)
        {
            throw new ArgumentNullException(nameof(controller));
        }
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        // Check for global restrictions first. Any global restriction overrides all others.
        if (globalRestrictions.TryGetValue(action.Keyword, out string globalRestriction))
        {
            return globalRestriction;
        }

        // A restriction in the room overrides any actions on the item.
        BaseObject currentRoom = controller.objectsManager.CurrentRoom;
        if (currentRoom == null)
        {
            throw new InvalidOperationException(
                "CurrentRoom is null when evaluating action restrictions. " +
                "The world should be fully initialised before processing actions.");
        }

        // Room-level restriction for this action keyword.
        string restrictionForRoom = ActionHelper.GetRestrictionMessage(
            currentRoom,
            action.Keyword
        );
        if (!string.IsNullOrEmpty(restrictionForRoom))
        {
            return restrictionForRoom;
        }

        // Check for restrictions on the specific item, if any.
        if (item != null)
        {
            string restrictionMessage = ActionHelper.GetRestrictionMessage(
                item,
                action.Keyword
            );
            if (!string.IsNullOrEmpty(restrictionMessage))
            {
                return restrictionMessage;
            }
        }

        return null;
    }
}

