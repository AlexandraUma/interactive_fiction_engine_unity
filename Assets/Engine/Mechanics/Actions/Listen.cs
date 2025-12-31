using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the "listen" action in the interactive fiction engine.
///
/// - With an item: describe the sound from that item, or a default if silent.
/// - Without an item: describe the ambient sound in the current room.
/// </summary>
[CreateAssetMenu(fileName = "Listen", menuName = "IFEngine/Actions/CoreActions/Listen")]
public class Listen : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "listen";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "hear",
        "listen to"
    };

    /// <summary>
    /// Listen does not change world state; it only describes.
    /// </summary>
    public override bool CanAffectWorld => false;

    /// <summary>
    /// Listen can be performed with or without an item.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.OPTIONAL;

    /// <summary>
    /// Listen can be tried on any item, or none at all.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item) => true;

    /// <summary>
    /// Executes the listen action.
    ///
    /// - No item: listen to the current room.
    /// - Item provided: listen to that item.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {

        // Determine what we are conceptually listening to: an explicit item or the current room.
        BaseObject target = item ?? controller.objectsManager.CurrentRoom
            ?? throw new InvalidOperationException("No current room is set in the game world.");

        // If the target defines a restriction for "listen", honour it and short‑circuit.
        string restrictionMessage = ActionHelper.GetRestrictionMessage(target, Keyword);
        if (restrictionMessage != null)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: restrictionMessage,
                status: ActionStatus.RESTRICTED
            );
        }

        if (item == null)
        {
            return ListenToRoom(controller);
        }

        return ListenToItem(controller, item);
    }

    /// <summary>
    /// Listen to the current room by logging its sound.
    /// </summary>
    private ActionStatus ListenToRoom(GameController controller)
    {
        BaseObject currentRoom = controller.objectsManager.CurrentRoom
            ?? throw new InvalidOperationException("No current room is set in the game world.");

        // Custom "listen" text response if provided.
        string textResponse = ActionHelper.GetTextResponse(currentRoom, Keyword);
        if (textResponse == null)
        {
            string roomSound = string.IsNullOrEmpty(currentRoom.sound) ||
                            currentRoom.sound == BaseObject.SILENCE
                ? "nothing unusual"
                : currentRoom.sound;

            textResponse = $"You hear {roomSound}.";
        }

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse
        );
    }

    /// <summary>
    /// Listen to a specific item by logging its sound.
    /// </summary>
    private ActionStatus ListenToItem(GameController controller, BaseObject item)
    {
        string textResponse = ActionHelper.GetTextResponse(item, Keyword);
        if (textResponse == null)
        {
            string itemSound = string.IsNullOrEmpty(item.sound) ||
                            item.sound == BaseObject.SILENCE
                ? "no noticeable sound"
                : item.sound;

            string itemName = !string.IsNullOrEmpty(item.mainName)
                ? item.mainName
                : item.name;

            textResponse = $"From the {itemName}, you hear {itemSound}.";
        }
    
        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse
        );
    }
}


