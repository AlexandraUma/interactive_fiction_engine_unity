using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Factory for creating test objects without requiring Unity asset creation.
/// </summary>
public static class TestObjectFactory
{
    /// <summary>
    /// Executes an action through the controller's ExecuteParsedCommand method,
    /// which properly sets up the event buffer context.
    /// </summary>
    public static List<IFEvent> ExecuteAction(GameController controller, CommandParser parser, string command)
    {
        var parseResult = parser.ParseUserInput(command, controller);
        return controller.ExecuteParsedCommand(parseResult);
    }

    /// <summary>
    /// Ensures a room has a HoldsContents property, creating it if it doesn't exist.
    /// </summary>
    public static HoldsContents EnsureRoomHasContents(Room room)
    {
        var contents = room.GetProperty<HoldsContents>();
        if (contents == null)
        {
            room.AddProperty(new HoldsContents());
            contents = room.GetProperty<HoldsContents>();
        }
        return contents;
    }
    /// <summary>
    /// Creates a basic BaseObject for testing.
    /// </summary>
    public static BaseObject CreateBaseObject(string name, string mainName = null, List<string> aliases = null)
    {
        var obj = ScriptableObject.CreateInstance<BaseObject>();
        obj.name = name;
        obj.mainName = mainName ?? name;
        obj.aliases = aliases ?? new List<string>();
        obj.initialAppearance = $"A {obj.mainName}.";
        return obj;
    }

    /// <summary>
    /// Creates a Room for testing.
    /// </summary>
    public static Room CreateRoom(string name, string mainName = null)
    {
        var room = ScriptableObject.CreateInstance<Room>();
        room.name = name;
        room.mainName = mainName ?? name;
        room.aliases = new List<string> { "room", "location" };
        room.initialAppearance = $"You are in the {room.mainName}.";
        room.exits = new List<Exit>();
        room.numVisits = 0;
        
        // Ensure room has required properties (Room.Reset() would do this, but we need to do it manually)
        if (!room.HasProperty<FixedInPlace>())
        {
            room.AddProperty(new FixedInPlace(isFixedInPlace: true));
        }
        if (!room.HasProperty<HoldsContents>())
        {
            room.AddProperty(new HoldsContents());
        }
        if (!room.HasProperty<Lightable>())
        {
            room.AddProperty(new Lightable(isLit: true));
        }
        
        return room;
    }

    /// <summary>
    /// Creates a Creature for testing.
    /// </summary>
    public static Creature CreateCreature(string name, string mainName = null, bool isPlayer = false)
    {
        var creature = ScriptableObject.CreateInstance<Creature>();
        creature.name = name;
        creature.mainName = mainName ?? name;
        creature.aliases = new List<string> { "creature", "person" };
        creature.isPlayer = isPlayer;
        return creature;
    }

    /// <summary>
    /// Creates an Exit for testing.
    /// </summary>
    public static Exit CreateExit(string name, Room destinationRoom, Door door = null)
    {
        var exit = ScriptableObject.CreateInstance<Exit>();
        exit.name = name;
        exit.mainName = name;
        exit.destinationRoom = destinationRoom;
        exit.door = door;
        return exit;
    }

    /// <summary>
    /// Creates a Door for testing.
    /// </summary>
    public static Door CreateDoor(string name, bool isOpen = false, bool isLocked = false)
    {
        var door = ScriptableObject.CreateInstance<Door>();
        door.name = name;
        door.mainName = name;
        door.aliases = new List<string> { "door" };
        door.AddProperty(new FixedInPlace(isFixedInPlace: true));
        door.AddProperty(new Openable(isOpen: isOpen));
        door.AddProperty(new Lockable(isLocked: isLocked));
        return door;
    }
}

