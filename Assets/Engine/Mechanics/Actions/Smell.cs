using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the "smell" action in the interactive fiction engine.
///
/// - With an item: describe the item's smell, using a custom "smell" text response
///   if present, or falling back to its `scent` field.
/// - Without an item: smell the current room, using its "smell" text response or `scent`.
/// </summary>
[CreateAssetMenu(fileName = "Smell", menuName = "IFEngine/Actions/CoreActions/Smell")]
public class Smell : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "smell";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "sniff"
    };

    /// <summary>
    /// Smell does not change world state; it only describes.
    /// </summary>
    public override bool CanAffectWorld => false;

    /// <summary>
    /// Smell can be performed with or without an item.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.OPTIONAL;

    /// <summary>
    /// Smell can be tried on any item, or none at all.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item) => true;

    /// <summary>
    /// Executes the smell action.
    ///
    /// - No item: smell the current room.
    /// - Item provided: smell that item.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {

        // Determine what we are conceptually smelling: an explicit item or the current room.
        BaseObject target = item ?? controller.objectsManager.CurrentRoom
            ?? throw new InvalidOperationException("No current room is set in the game world.");

        // If the target defines a restriction for "smell", honour it and short‑circuit.
        string restrictionMessage = ActionHelper.GetRestrictionMessage(target, Keyword);
        if (restrictionMessage != null)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: restrictionMessage,
                status: ActionStatus.RESTRICTED
            );
        }

        return item == null ? SmellRoom(controller) : SmellItem(controller, item);
    }

    /// <summary>
    /// Smell the current room by logging its scent description.
    /// </summary>
    private ActionStatus SmellRoom(GameController controller)
    {
        BaseObject currentRoom = controller.objectsManager.CurrentRoom;
        string textResponse = ActionHelper.GetTextResponse(currentRoom, Keyword);
        if (textResponse == null)
        {
            string roomScent = string.IsNullOrEmpty(currentRoom.scent) ||
                            currentRoom.scent == BaseObject.NOTHING
                ? "nothing in particular"
                : currentRoom.scent;
            textResponse = $"The air smells of {roomScent}.";
        }

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse
        );
    }

    /// <summary>
    /// Smell a specific item by logging its scent description.
    /// </summary>
    private ActionStatus SmellItem(GameController controller, BaseObject item)
    {
        string textResponse = ActionHelper.GetTextResponse(item, Keyword);
        if (textResponse == null)
        {
            string itemScent = string.IsNullOrEmpty(item.scent) ||
                            item.scent == BaseObject.NOTHING
                ? "nothing noticeable"
                : item.scent;
            textResponse = $"You smell {itemScent} from the {item.mainName}.";
        }

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse
        );
    }
}


