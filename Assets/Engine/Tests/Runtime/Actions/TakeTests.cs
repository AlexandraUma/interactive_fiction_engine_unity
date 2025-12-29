using NUnit.Framework;
using System.Collections.Generic;

public class TakeTests
{
    private Take takeAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject testItem;
    private BaseObject fixedItem;

    [SetUp]
    public void SetUp()
    {
        takeAction = new Take();
        
        testRoom = TestObjectFactory.CreateRoom("TestRoom", "Test Room");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        testItem = TestObjectFactory.CreateBaseObject("TestItem", "test item");
        fixedItem = TestObjectFactory.CreateBaseObject("FixedItem", "fixed item");
        fixedItem.AddProperty(new FixedInPlace(isFixedInPlace: true));

        var allRooms = new List<BaseObject> { testRoom };
        var npcs = new List<BaseObject>();
        var allActions = RegisteredActions.Create();

        controller = new GameController(
            intro: "",
            prologue: "",
            startingRoom: testRoom,
            allRooms: allRooms,
            playerCharacter: player,
            nonPlayerCharacters: npcs,
            allActions: allActions
        );
    }

    [Test]
    public void Keyword_ReturnsTake()
    {
        Assert.AreEqual("take", takeAction.Keyword);
    }

    [Test]
    public void Aliases_ContainsExpectedAliases()
    {
        Assert.Contains("grab", takeAction.Aliases);
        Assert.Contains("pick up", takeAction.Aliases);
    }

    [Test]
    public void CanAffectWorld_ReturnsTrue()
    {
        Assert.IsTrue(takeAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsRequired()
    {
        Assert.AreEqual(ItemApplicabilityLevel.REQUIRED, takeAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrueForMovableItem()
    {
        Assert.IsTrue(takeAction.CanApplyToItem(testItem));
    }

    [Test]
    public void CanApplyToItem_ReturnsFalseForFixedItem()
    {
        Assert.IsFalse(takeAction.CanApplyToItem(fixedItem));
    }

    [Test]
    public void CanApplyToItem_ReturnsTrueForFixedItemThatIsNotFixed()
    {
        var item = TestObjectFactory.CreateBaseObject("Item", "item");
        item.AddProperty(new FixedInPlace(isFixedInPlace: false));
        Assert.IsTrue(takeAction.CanApplyToItem(item));
    }

    [Test]
    public void Execute_SuccessfullyTakesItem()
    {
        // Add item to room
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "take test item");

        Assert.Greater(events.Count, 0);
        Assert.IsTrue(controller.objectsManager.IsItemCarriedByPlayer(testItem));
    }

    [Test]
    public void Execute_ReturnsIneffectiveWhenItemAlreadyCarried()
    {
        // Add item to player inventory
        controller.objectsManager.AddItemToPlayer(testItem);
        
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "take test item");

        Assert.Greater(events.Count, 0);
        bool hasIneffectiveMessage = events.Exists(e => e.eventText.Contains("already have"));
        Assert.IsTrue(hasIneffectiveMessage);
    }

    [Test]
    public void Execute_ReturnsFailedForFixedItem()
    {
        // Add fixed item to room
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["fixed item"] = fixedItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "take fixed item");

        Assert.Greater(events.Count, 0);
        bool hasFailedMessage = events.Exists(e => e.eventText.Contains("can't take"));
        Assert.IsTrue(hasFailedMessage);
        Assert.IsFalse(controller.objectsManager.IsItemCarriedByPlayer(fixedItem));
    }

    [Test]
    public void Execute_LogsCustomTextResponse()
    {
        testItem.textResponses = new List<TextResponse>
        {
            new TextResponse { actionKeyword = "take", response = "You grab the item quickly." }
        };
        testItem.RebuildActionLookupsPublic();

        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "take test item");

        Assert.Greater(events.Count, 0);
        bool hasCustomResponse = events.Exists(e => e.eventText.Contains("You grab the item quickly"));
        Assert.IsTrue(hasCustomResponse);
    }

    [Test]
    public void Execute_LogsDefaultTextResponseWhenNoCustomResponse()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "take test item");

        Assert.Greater(events.Count, 0);
        Assert.IsTrue(controller.objectsManager.IsItemCarriedByPlayer(testItem));
    }
}

