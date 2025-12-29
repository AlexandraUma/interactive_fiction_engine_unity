using System.Collections.Generic;

/// <summary>
/// Handles the "close" action in the interactive fiction engine.
///
/// Possible outcomes:
/// - Failure: item is not openable.
/// - Success: item is closed and a response is logged.
/// - Ineffective: item is already closed.
/// </summary>
public class Close : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "close";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "shut"
    };

    /// <summary>
    /// Closing an item changes the world (its open state).
    /// </summary>
    public override bool CanAffectWorld => true;

    /// <summary>
    /// Close requires a specific item to act on.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.REQUIRED;

    /// <summary>
    /// Returns true if the item can be closed.
    /// An item is closeable if it has an <see cref="Openable"/> property.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item)
    {
        return item.HasProperty<Openable>();
    }

    /// <summary>
    /// Executes the close action on the given item.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // You can't close a non-openable item.
        if (!CanApplyToItem(item))
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "You can't close that.",
                status: ActionStatus.FAILED
            );
        }

        // Close the item using the default behaviour.
        return CloseItem(controller, item);
    }

    /// <summary>
    /// Default implementation of closing an item:
    /// - If already closed, return an ineffective result.
    /// - Otherwise, set it to closed and log a response.
    /// </summary>
    private ActionStatus CloseItem(GameController controller, BaseObject item)
    {
        Openable openable = item.GetProperty<Openable>();

        if (!openable.IsOpen)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "It's already closed.",
                status: ActionStatus.INEFFECTIVE
            );
        }

        // Close the item.
        openable.IsOpen = false;

        // Custom "close" text response if provided, otherwise a sensible default.
        string textResponse = ActionHelper.GetTextResponse(item, Keyword);
        textResponse ??= $"You close the {item.mainName}.";

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse,
            status: ActionStatus.SUCCESSFUL
        );
    }
}
