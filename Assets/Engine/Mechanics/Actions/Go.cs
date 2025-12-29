using System.Collections.Generic;

/// <summary>
/// Handles the "go" action in the interactive fiction engine.
///
/// The player moves from the current room through an exit, optionally via a door.
///
/// Possible outcomes:
/// - Failure:
///   - Given item is not an exit.
///   - Door cannot be opened.
/// - Success:
///   - Move to the destination room (with or without opening a door first).
/// </summary>
public class Go : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "go";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "move",
        "walk",
        "move me"
    };

    /// <summary>
    /// Going somewhere changes the world (current room).
    /// </summary>
    public override bool CanAffectWorld => true;

    /// <summary>
    /// Go requires a specific item (an exit) to act on.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.REQUIRED;

    /// <summary>
    /// The player is only allowed to go through exits.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item)
    {
        return item is Exit;
    }

    /// <summary>
    /// Executes the go action via the given exit.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // If the supposed exit object is not an Exit, return a failure message.
        if (!CanApplyToItem(item))
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "That's not a valid exit.",
                status: ActionStatus.FAILED
            );
        }

        Exit exitObject = (Exit)item;

        // If there is an associated door, try to go through it first.
        Door door = exitObject.door as Door;
        Room destinationRoom = exitObject.destinationRoom;

        if (door != null)
        {
            return GoThroughDoor(controller, door, destinationRoom);
        }

        // Otherwise, go straight to the destination room.
        return GoToRoom(controller, destinationRoom);
    }

    /// <summary>
    /// Go to a new room through a door, first attempting to open it if needed.
    /// </summary>
    private ActionStatus GoThroughDoor(GameController controller, Door door, Room destinationRoom)
    {
        // Check if the door is already open.
        if (door.IsOpen)
        {
            // Door is already open: go straight to the destination room.
            return GoToRoom(controller, destinationRoom);
        }

        // Log the action of going through the door.
        controller.LogEvent(
            eventText: $"(first opening the {door.name})",
            eventType: EventType.WORLD_RESPONSE
        );

        // Get the "open" action.
        Action openAction = controller.actionsManager.GetAction("open");
        if (openAction == null)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "That's strange. You can't open the door.",
                status: ActionStatus.FAILED
            );
        }

        // Try to open the door.
        ActionStatus openActionStatus = openAction.Execute(controller, door);

        // If the door did not open successfully, propagate that status.
        // Note: INEFFECTIVE means it's already open, which we already checked, so treat as success.
        if (openActionStatus == ActionStatus.FAILED || !door.IsOpen)
        {
            return openActionStatus;
        }

        // Door is open: go to the destination room.
        return GoToRoom(controller, destinationRoom);
    }

    /// <summary>
    /// Go to the given room by changing the current room and then looking.
    /// </summary>
    private ActionStatus GoToRoom(GameController controller, Room room)
    {
        controller.objectsManager.SetCurrentRoom(room);
        controller.actionsManager.GetAction("look")?.Execute(controller, item: null);
        return ActionStatus.SUCCESSFUL;
    }
}
