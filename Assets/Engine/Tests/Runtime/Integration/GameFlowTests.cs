using NUnit.Framework;
using System.Collections.Generic;

public class GameFlowTests
{
    private GameController controller;
    private CommandParser parser;
    private Room room1;
    private Room room2;
    private Creature player;
    private BaseObject item;
    private Exit exit;

    [SetUp]
    public void SetUp()
    {
        parser = new CommandParser();
        
        room1 = TestObjectFactory.CreateRoom("Room1", "Room 1");
        room2 = TestObjectFactory.CreateRoom("Room2", "Room 2");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        item = TestObjectFactory.CreateBaseObject("Item", "key");
        exit = TestObjectFactory.CreateExit("north", room2);

        var allRooms = new List<BaseObject> { room1, room2 };
        var npcs = new List<BaseObject>();
        var allActions = RegisteredActions.Create();

        controller = new GameController(
            intro: "Welcome",
            prologue: "Prologue",
            startingRoom: room1,
            allRooms: allRooms,
            playerCharacter: player,
            nonPlayerCharacters: npcs,
            allActions: allActions
        );
    }

    [Test]
    public void CompleteFlow_TakeItemAndMove()
    {
        // Add item to room
        var roomContents = TestObjectFactory.EnsureRoomHasContents(room1);
        roomContents.Contents["key"] = item;
        controller.objectsManager.SetCurrentRoom(room1);

        // Parse and execute "take key"
        var parseResult = parser.ParseUserInput("take key", controller);
        var events = controller.ExecuteParsedCommand(parseResult);

        Assert.Greater(events.Count, 0);
        Assert.IsTrue(controller.objectsManager.IsItemCarriedByPlayer(item));

        // Parse and execute "go north"
        room1.exits = new List<Exit> { exit };
        controller.objectsManager.SetCurrentRoom(room1);
        parseResult = parser.ParseUserInput("go north", controller);
        events = controller.ExecuteParsedCommand(parseResult);

        Assert.Greater(events.Count, 0);
        Assert.AreEqual(room2, controller.objectsManager.CurrentRoom);
    }

    [Test]
    public void CompleteFlow_OpenContainerAndTakeItem()
    {
        var container = TestObjectFactory.CreateBaseObject("Container", "chest");
        container.AddProperty(new Openable(isOpen: false));
        container.AddProperty(new HoldsContents());
        var containerContents = container.GetProperty<HoldsContents>();
        containerContents.Contents["key"] = item;

        var roomContents = TestObjectFactory.EnsureRoomHasContents(room1);
        roomContents.Contents["chest"] = container;
        controller.objectsManager.SetCurrentRoom(room1);

        // Open container
        var parseResult = parser.ParseUserInput("open chest", controller);
        var events = controller.ExecuteParsedCommand(parseResult);
        Assert.Greater(events.Count, 0);
        Assert.IsTrue(container.GetProperty<Openable>().IsOpen);

        // Take item from container
        parseResult = parser.ParseUserInput("take key", controller);
        events = controller.ExecuteParsedCommand(parseResult);
        Assert.Greater(events.Count, 0);
        Assert.IsTrue(controller.objectsManager.IsItemCarriedByPlayer(item));
    }

    [Test]
    public void CompleteFlow_ActionChaining()
    {
        var door = TestObjectFactory.CreateDoor("door", isOpen: false, isLocked: true);
        exit.door = door;
        room1.exits = new List<Exit> { exit };
        controller.objectsManager.SetCurrentRoom(room1);

        // Try to go through locked door (should fail)
        var parseResult = parser.ParseUserInput("go north", controller);
        var events = controller.ExecuteParsedCommand(parseResult);
        Assert.AreEqual(room1, controller.objectsManager.CurrentRoom);

        // Unlock door
        parseResult = parser.ParseUserInput("unlock door", controller);
        events = controller.ExecuteParsedCommand(parseResult);
        Assert.IsFalse(door.IsLocked);

        // Open door
        parseResult = parser.ParseUserInput("open door", controller);
        events = controller.ExecuteParsedCommand(parseResult);
        Assert.IsTrue(door.IsOpen);

        // Go through door
        parseResult = parser.ParseUserInput("go north", controller);
        events = controller.ExecuteParsedCommand(parseResult);
        Assert.AreEqual(room2, controller.objectsManager.CurrentRoom);
    }
}

