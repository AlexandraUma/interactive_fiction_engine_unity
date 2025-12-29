using System;
using System.Collections.Generic;

/// <summary>
/// Handles the "look" action in the interactive fiction engine.
///
/// - If an item is given, delegate to the "examine" action on that item.
/// - Otherwise, show the current room's name and description.
/// </summary>
public class Look : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "look";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "look around",
        "observe",
        "watch"
    };

    /// <summary>
    /// Look does not change world state; it only describes.
    /// </summary>
    public override bool CanAffectWorld => false;

    /// <summary>
    /// Look is treated as not requiring an item.
    /// If an item is supplied, we still interpret it as "examine".
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.NA;

    /// <summary>
    /// Look can conceptually be applied to any item (it will be forwarded to Examine).
    /// </summary>
    public override bool CanApplyToItem(BaseObject item) => true;

    /// <summary>
    /// Executes the look action.
    ///
    /// - If an item is provided, find the "examine" action and run it on that item.
    /// - If no item is provided, log the current room's name and description.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // If the player provided an item, treat like a mistake, and run "examine <item>" directly.
        if (item != null)
        {
            Action examineAction = controller.actionsManager.GetAction("examine")
                ?? throw new InvalidOperationException("No 'examine' action registered in this game.");

            return examineAction.Execute(controller, item);
        }

        // Otherwise, we look at the current room.
        BaseObject currentRoom = controller.objectsManager.CurrentRoom
            ?? throw new InvalidOperationException("No current room is set in the game world.");

        // Try to find a specific "look" text response for the room; otherwise use a fallback description.
        string textResponse = ActionHelper.GetTextResponse(currentRoom, Keyword);
        if (textResponse == null)
        {
            string roomName = string.IsNullOrEmpty(currentRoom.mainName)
                ? currentRoom.name
                : currentRoom.mainName;

            textResponse = $"You are in {roomName}.";
        }

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse
        );
    }
}


