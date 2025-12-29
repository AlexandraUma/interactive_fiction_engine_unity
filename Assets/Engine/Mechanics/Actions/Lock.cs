using System.Collections.Generic;

/// <summary>
/// Handles the "lock" action in the interactive fiction engine.
///
/// Possible outcomes:
/// - Failure: item is not lockable.
/// - Success: item is locked and a response is logged.
/// - Ineffective: item is already locked.
/// </summary>
public class Lock : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "lock";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "bolt",
        "fasten",
        "secure"
    };

    /// <summary>
    /// Locking an item changes the world (its locked state).
    /// </summary>
    public override bool CanAffectWorld => true;

    /// <summary>
    /// Lock requires a specific item to act on.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.REQUIRED;

    /// <summary>
    /// Returns true if the item can be locked.
    /// An item is lockable if it has a <see cref="Lockable"/> property.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item)
    {
        return item.HasProperty<Lockable>();
    }

    /// <summary>
    /// Executes the lock action on the given item.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // If the item is not lockable, it cannot be locked.
        if (!CanApplyToItem(item))
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "You can't lock that.",
                status: ActionStatus.FAILED
            );
        }

        // Lock the item using the default behaviour.
        return LockItem(controller, item);
    }

    /// <summary>
    /// Default implementation of locking an item:
    /// - If already locked, return an ineffective result.
    /// - Otherwise, set it to locked and log a response.
    /// </summary>
    private ActionStatus LockItem(GameController controller, BaseObject item)
    {
        Lockable lockable = item.GetProperty<Lockable>();

        if (lockable.IsLocked)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "It's already locked.",
                status: ActionStatus.INEFFECTIVE
            );
        }

        // Lock the item.
        lockable.IsLocked = true;

        // Custom "lock" text response if provided, otherwise a sensible default.
        string textResponse = ActionHelper.GetTextResponse(item, Keyword);
        textResponse ??= $"You lock the {item.mainName}.";

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse,
            status: ActionStatus.SUCCESSFUL
        );
    }
}
