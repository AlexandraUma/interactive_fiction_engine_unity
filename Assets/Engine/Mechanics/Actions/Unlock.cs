using System.Collections.Generic;

/// <summary>
/// Handles the "unlock" action in the interactive fiction engine.
///
/// Possible outcomes:
/// - Failure: item is not lockable.
/// - Success: item is unlocked and a response is logged.
/// - Ineffective: item is already unlocked.
/// </summary>
public class Unlock : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "unlock";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "unbolt",
        "unfasten",
    };

    /// <summary>
    /// Unlocking an item changes the world (its locked state).
    /// </summary>
    public override bool CanAffectWorld => true;

    /// <summary>
    /// Unlock requires a specific item to act on.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.REQUIRED;

    /// <summary>
    /// Returns true if the item can be unlocked.
    /// An item is unlockable if it has a <see cref="Lockable"/> property.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item)
    {
        return item.HasProperty<Lockable>();
    }

    /// <summary>
    /// Executes the unlock action on the given item.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // If the item is not lockable, it cannot be unlocked.
        if (!CanApplyToItem(item))
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "You can't unlock that.",
                status: ActionStatus.FAILED
            );
        }

        // Unlock the item using the default behaviour.
        return UnlockItem(controller, item);
    }

    /// <summary>
    /// Default implementation of unlocking an item:
    /// - If already unlocked, return an ineffective result.
    /// - Otherwise, set it to unlocked and log a response.
    /// </summary>
    private ActionStatus UnlockItem(GameController controller, BaseObject item)
    {
        Lockable lockable = item.GetProperty<Lockable>();

        if (!lockable.IsLocked)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "It's already unlocked.",
                status: ActionStatus.INEFFECTIVE
            );
        }

        // Unlock the item.
        lockable.IsLocked = false;

        // Custom "unlock" text response if provided, otherwise a sensible default.
        string textResponse = ActionHelper.GetTextResponse(item, Keyword);
        textResponse ??= $"You unlock the {item.mainName}.";

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse,
            status: ActionStatus.SUCCESSFUL
        );
    }
}
