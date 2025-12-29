using NUnit.Framework;
using System.Collections.Generic;

public class LookTests
{
    private Look lookAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject testItem;

    [SetUp]
    public void SetUp()
    {
        lookAction = new Look();
        
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
    public void Keyword_ReturnsLook()
    {
        Assert.AreEqual("look", lookAction.Keyword);
    }

    [Test]
    public void CanAffectWorld_ReturnsFalse()
    {
        Assert.IsFalse(lookAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsNA()
    {
        Assert.AreEqual(ItemApplicabilityLevel.NA, lookAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrue()
    {
        Assert.IsTrue(lookAction.CanApplyToItem(testItem));
    }

    [Test]
    public void Execute_ShowsRoomDescriptionWhenNoItem()
    {
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "look");
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_RedirectsToExamineWhenItemProvided()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);
        
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "look test item");
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_UsesCustomTextResponse()
    {
        testRoom.textResponses = new List<TextResponse>
        {
            new TextResponse { actionKeyword = "look", response = "You see a test room." }
        };
        testRoom.RebuildActionLookupsPublic();

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "look");
        bool hasCustomResponse = events.Exists(e => e.eventText.Contains("You see a test room"));
        Assert.IsTrue(hasCustomResponse);
    }
}

