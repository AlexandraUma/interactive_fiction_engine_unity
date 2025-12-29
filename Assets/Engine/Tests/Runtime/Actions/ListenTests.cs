using NUnit.Framework;
using System.Collections.Generic;

public class ListenTests
{
    private Listen listenAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject testItem;

    [SetUp]
    public void SetUp()
    {
        listenAction = new Listen();
        
        testRoom = TestObjectFactory.CreateRoom("TestRoom", "Test Room");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        testItem = TestObjectFactory.CreateBaseObject("TestItem", "test item");

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
    public void Keyword_ReturnsListen()
    {
        Assert.AreEqual("listen", listenAction.Keyword);
    }

    [Test]
    public void CanAffectWorld_ReturnsFalse()
    {
        Assert.IsFalse(listenAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsOptional()
    {
        Assert.AreEqual(ItemApplicabilityLevel.OPTIONAL, listenAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrue()
    {
        Assert.IsTrue(listenAction.CanApplyToItem(testItem));
    }

    [Test]
    public void Execute_ListensToRoomWhenNoItem()
    {
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "listen");
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_ListensToItem()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);
        
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "listen test item");
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_UsesCustomSound()
    {
        testItem.sound = "humming";
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);
        
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "listen test item");
        bool hasSound = events.Exists(e => e.eventText.Contains("humming"));
        Assert.IsTrue(hasSound);
    }

    [Test]
    public void Execute_RespectsRestrictions()
    {
        testItem.actionRestrictions = new List<ActionRestriction>
        {
            new ActionRestriction { actionKeyword = "listen", message = "You cannot listen to that." }
        };
        
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "listen test item");
        bool hasRestriction = events.Exists(e => e.eventText.Contains("You cannot listen to that"));
        Assert.IsTrue(hasRestriction);
    }
}

