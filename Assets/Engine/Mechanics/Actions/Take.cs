using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the "take" action in the interactive fiction engine.
///
/// Possible outcomes:
/// - Success: item is added to the inventory and removed from the room.
/// - Failure: item cannot be taken (e.g. fixed in place).
/// - Ineffective: item is already carried by the player.
/// </summary>
[CreateAssetMenu(fileName = "Take", menuName = "IFEngine/Actions/CoreActions/Take")]
public class Take : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "take";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "grab",
        "pick up"
    };

    /// <summary>
    /// Taking an item changes the world (moves it from room to inventory).
    /// </summary>
    public override bool CanAffectWorld => true;

    /// <summary>
    /// Take requires a specific item to act on.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.REQUIRED;

    /// <summary>
    /// Returns true if the item can be taken.
    ///
    /// An item is considered takeable if either:
    /// - It has no <see cref="FixedInPlace"/> property, or
    /// - It has that property but <see cref="FixedInPlace.IsFixedInPlace"/> is false.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item)
    {
        return !item.HasProperty<FixedInPlace>() || !item.GetProperty<FixedInPlace>().IsFixedInPlace;
    }

    /// <summary>
    /// Executes the take action on the given item.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // If the item is already in the inventory, you can't take it again.
        if (controller.objectsManager.IsItemCarriedByPlayer(item))
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "You already have that.",
                status: ActionStatus.INEFFECTIVE
            );
        }

        // If the item is not takeable, log the appropriate text response.
        if (!CanApplyToItem(item))
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "You obviously can't take that.",
                status: ActionStatus.FAILED
            );
        }

        // Take the item using the default behaviour.
        return TakeItem(controller, item);
    }

    /// <summary>
    /// Default implementation of taking a movable item:
    /// - Add the item to the player's inventory.
    /// - Remove the item from the current room.
    /// - Log a custom or default text response.
    /// </summary>
    private ActionStatus TakeItem(GameController controller, BaseObject item)
    {
        // Put item in inventory.
        controller.objectsManager.AddItemToPlayer(item);

        // Remove item from the room.
        controller.objectsManager.RemoveItemFromRoom(item);

        // Log the text response: custom "take" text, or a sensible default.
        string textResponse = ActionHelper.GetTextResponse(item, Keyword);
        textResponse ??= $"You take the {item.mainName}.";

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse,
            status: ActionStatus.SUCCESSFUL
        );
    }
}
