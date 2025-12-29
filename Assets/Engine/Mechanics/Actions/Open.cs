using System.Collections.Generic;

/// <summary>
/// Handles the "open" action in the interactive fiction engine.
///
/// Possible outcomes:
/// - Failure: item is not openable.
/// - Success: item is opened and a response is logged.
/// - Ineffective: item is already open.
/// </summary>
public class Open : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "open";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// Python needed a different filename because "open" is reserved,
    /// but here we can keep it simple.
    /// </summary>
    public override List<string> Aliases { get; } = new();

    /// <summary>
    /// Opening an item changes the world (its open state).
    /// </summary>
    public override bool CanAffectWorld => true;

    /// <summary>
    /// Open requires a specific item to act on.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.REQUIRED;

    /// <summary>
    /// Returns true if the item can be opened.
    /// An item is openable if it has an <see cref="Openable"/> property.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item)
    {
        return item.HasProperty<Openable>();
    }

    /// <summary>
    /// Executes the open action on the given item.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // You can't open a non-openable item.
        if (!CanApplyToItem(item))
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "You can't open that.",
                status: ActionStatus.FAILED
            );
        }

        // Open the item using the default behaviour.
        return OpenItem(controller, item);
    }

    /// <summary>
    /// Default implementation of opening an item:
    /// - If already open, return an ineffective result.
    /// - If locked, return a failed result.
    /// - Otherwise, set it to open and log a response.
    /// </summary>
    private ActionStatus OpenItem(GameController controller, BaseObject item)
    {
        Openable openable = item.GetProperty<Openable>();

        if (openable.IsOpen)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "It's already open.",
                status: ActionStatus.INEFFECTIVE
            );
        }

        // Check if the item is locked.
        if (item.HasProperty<Lockable>())
        {
            Lockable lockable = item.GetProperty<Lockable>();
            if (lockable.IsLocked)
            {
                return ActionHelper.LogActionAndReturnStatus(
                    gameController: controller,
                    message: $"The {item.mainName} is locked.",
                    status: ActionStatus.FAILED
                );
            }
        }

        // Open the item.
        openable.IsOpen = true;

        // Custom "open" text response if provided, otherwise a sensible default.
        string textResponse = ActionHelper.GetTextResponse(item, Keyword);
        textResponse ??= $"You open the {item.mainName}.";

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse,
            status: ActionStatus.SUCCESSFUL
        );
    }
}
