using NUnit.Framework;
using System.Collections.Generic;

public class GoTests
{
    private Go goAction;
    private GameController controller;
    private Room room1;
    private Room room2;
    private Creature player;
    private Exit exit;
    private Door door;

    [SetUp]
    public void SetUp()
    {
        goAction = new Go();
        
        room1 = TestObjectFactory.CreateRoom("Room1", "Room 1");
        room2 = TestObjectFactory.CreateRoom("Room2", "Room 2");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        exit = TestObjectFactory.CreateExit("north", room2);
        door = TestObjectFactory.CreateDoor("door", isOpen: false, isLocked: false);

        var allRooms = new List<BaseObject> { room1, room2 };
        var npcs = new List<BaseObject>();
        var allActions = RegisteredActions.Create();

        controller = new GameController(
            intro: "",
            prologue: "",
            startingRoom: room1,
            allRooms: allRooms,
            playerCharacter: player,
            nonPlayerCharacters: npcs,
            allActions: allActions
        );
    }

    [Test]
    public void Keyword_ReturnsGo()
    {
        Assert.AreEqual("go", goAction.Keyword);
    }

    [Test]
    public void Aliases_ContainsExpectedAliases()
    {
        Assert.Contains("move", goAction.Aliases);
        Assert.Contains("walk", goAction.Aliases);
        Assert.Contains("move me", goAction.Aliases);
    }

    [Test]
    public void CanAffectWorld_ReturnsTrue()
    {
        Assert.IsTrue(goAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsRequired()
    {
        Assert.AreEqual(ItemApplicabilityLevel.REQUIRED, goAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrueForExit()
    {
        Assert.IsTrue(goAction.CanApplyToItem(exit));
    }

    [Test]
    public void CanApplyToItem_ReturnsFalseForNonExit()
    {
        var item = TestObjectFactory.CreateBaseObject("Item", "item");
        Assert.IsFalse(goAction.CanApplyToItem(item));
    }

    [Test]
    public void Execute_SuccessfullyMovesThroughExit()
    {
        room1.exits = new List<Exit> { exit };
        controller.objectsManager.SetCurrentRoom(room1);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "go north");

        Assert.Greater(events.Count, 0);
        Assert.AreEqual(room2, controller.objectsManager.CurrentRoom);
    }

    [Test]
    public void Execute_ReturnsFailedForNonExit()
    {
        var item = TestObjectFactory.CreateBaseObject("Item", "item");
        var roomContents = TestObjectFactory.EnsureRoomHasContents(room1);
        roomContents.Contents["item"] = item;
        controller.objectsManager.SetCurrentRoom(room1);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "go item");
        
        bool hasFailedMessage = events.Exists(e => e.eventText.Contains("not a valid exit") || e.eventText.Contains("no exit"));
        Assert.IsTrue(hasFailedMessage);
    }

    [Test]
    public void Execute_OpensDoorBeforeMoving()
    {
        // CreateDoor already adds Openable property, just ensure it's closed
        door = TestObjectFactory.CreateDoor("door", isOpen: false, isLocked: false);
        exit.door = door;
        room1.exits = new List<Exit> { exit };
        controller.objectsManager.SetCurrentRoom(room1);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "go north");

        Assert.Greater(events.Count, 0);
        Assert.IsTrue(door.IsOpen);
        Assert.AreEqual(room2, controller.objectsManager.CurrentRoom);
    }

    [Test]
    public void Execute_FailsWhenDoorCannotBeOpened()
    {
        // Create a door that is closed and locked (CreateDoor already adds Openable and Lockable)
        door = TestObjectFactory.CreateDoor("door", isOpen: false, isLocked: true);
        exit.door = door;
        room1.exits = new List<Exit> { exit };
        controller.objectsManager.SetCurrentRoom(room1);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "go north");

        Assert.Greater(events.Count, 0);
        // Should still be in room1 if door couldn't be opened
        Assert.AreEqual(room1, controller.objectsManager.CurrentRoom);
    }
}

