using NUnit.Framework;
using System.Collections.Generic;

public class ExamineTests
{
    private Examine examineAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject testItem;

    [SetUp]
    public void SetUp()
    {
        examineAction = new Examine();
        
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
    public void Keyword_ReturnsExamine()
    {
        Assert.AreEqual("examine", examineAction.Keyword);
    }

    [Test]
    public void Aliases_ContainsExpectedAliases()
    {
        Assert.Contains("x", examineAction.Aliases);
        Assert.Contains("look at", examineAction.Aliases);
        Assert.Contains("inspect", examineAction.Aliases);
    }

    [Test]
    public void CanAffectWorld_ReturnsFalse()
    {
        Assert.IsFalse(examineAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsOptional()
    {
        Assert.AreEqual(ItemApplicabilityLevel.OPTIONAL, examineAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrueForAnyItem()
    {
        Assert.IsTrue(examineAction.CanApplyToItem(testItem));
        Assert.IsTrue(examineAction.CanApplyToItem(null));
    }

    [Test]
    public void Execute_RedirectsToLookWhenNoItem()
    {
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "examine");
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_ExaminesItem()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);
        
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "examine test item");
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_LogsCustomTextResponse()
    {
        testItem.textResponses = new List<TextResponse>
        {
            new TextResponse { actionKeyword = "examine", response = "It's a mysterious item." }
        };
        testItem.RebuildActionLookupsPublic();
        
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "examine test item");
        bool hasCustomResponse = events.Exists(e => e.eventText.Contains("It's a mysterious item"));
        Assert.IsTrue(hasCustomResponse);
    }

    [Test]
    public void Execute_LogsDefaultTextResponse()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);
        
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "examine test item");
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_RespectsRestrictions()
    {
        testItem.actionRestrictions = new List<ActionRestriction>
        {
            new ActionRestriction { actionKeyword = "examine", message = "You cannot examine that." }
        };
        
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "examine test item");
        bool hasRestriction = events.Exists(e => e.eventText.Contains("You cannot examine that"));
        Assert.IsTrue(hasRestriction);
    }
}

