using NUnit.Framework;
using System.Collections.Generic;

public class CloseTests
{
    private Close closeAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject openableItem;

    [SetUp]
    public void SetUp()
    {
        closeAction = new Close();
        
        testRoom = TestObjectFactory.CreateRoom("TestRoom", "Test Room");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        openableItem = TestObjectFactory.CreateBaseObject("OpenableItem", "openable item");
        openableItem.AddProperty(new Openable(isOpen: true));

        var allRooms = new List<BaseObject> { testRoom };
        var npcs = new List<BaseObject>();
        var allActions = RegisteredActions.Create();

        controller = new GameController(
            intro: "",
            prologue: "",
            startingRoom: testRoom,
            allRooms: new List<BaseObject> { testRoom },
            playerCharacter: player,
            nonPlayerCharacters: new List<BaseObject>(),
            allActions: allActions
        );
    }

    [Test]
    public void Keyword_ReturnsClose()
    {
        Assert.AreEqual("close", closeAction.Keyword);
    }

    [Test]
    public void CanAffectWorld_ReturnsTrue()
    {
        Assert.IsTrue(closeAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsRequired()
    {
        Assert.AreEqual(ItemApplicabilityLevel.REQUIRED, closeAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrueForOpenableItem()
    {
        Assert.IsTrue(closeAction.CanApplyToItem(openableItem));
    }

    [Test]
    public void Execute_SuccessfullyClosesItem()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["openable item"] = openableItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "close openable item");

        Assert.Greater(events.Count, 0);
        var openable = openableItem.GetProperty<Openable>();
        Assert.IsFalse(openable.IsOpen);
    }

    [Test]
    public void Execute_ReturnsIneffectiveWhenAlreadyClosed()
    {
        openableItem.GetProperty<Openable>().IsOpen = false;
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["openable item"] = openableItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "close openable item");
        
        bool hasIneffectiveMessage = events.Exists(e => e.eventText.Contains("already closed"));
        Assert.IsTrue(hasIneffectiveMessage);
    }
}

