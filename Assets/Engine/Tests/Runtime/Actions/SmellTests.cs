using NUnit.Framework;
using System.Collections.Generic;

public class SmellTests
{
    private Smell smellAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject testItem;

    [SetUp]
    public void SetUp()
    {
        smellAction = new Smell();
        
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
    public void Keyword_ReturnsSmell()
    {
        Assert.AreEqual("smell", smellAction.Keyword);
    }

    [Test]
    public void CanAffectWorld_ReturnsFalse()
    {
        Assert.IsFalse(smellAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsOptional()
    {
        Assert.AreEqual(ItemApplicabilityLevel.OPTIONAL, smellAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrue()
    {
        Assert.IsTrue(smellAction.CanApplyToItem(testItem));
    }

    [Test]
    public void Execute_SmellsRoomWhenNoItem()
    {
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "smell");
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_SmellsItem()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);
        
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "smell test item");
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_UsesCustomScent()
    {
        testItem.scent = "roses";
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);
        
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "smell test item");
        bool hasScent = events.Exists(e => e.eventText.Contains("roses"));
        Assert.IsTrue(hasScent);
    }

    [Test]
    public void Execute_RespectsRestrictions()
    {
        testItem.actionRestrictions = new List<ActionRestriction>
        {
            new ActionRestriction { actionKeyword = "smell", message = "You cannot smell that." }
        };
        
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "smell test item");
        bool hasRestriction = events.Exists(e => e.eventText.Contains("You cannot smell that"));
        Assert.IsTrue(hasRestriction);
    }
}

