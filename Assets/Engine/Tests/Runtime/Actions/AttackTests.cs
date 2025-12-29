using NUnit.Framework;
using System.Collections.Generic;

public class AttackTests
{
    private Attack attackAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject testItem;

    [SetUp]
    public void SetUp()
    {
        attackAction = new Attack();
        
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
    public void Keyword_ReturnsAttack()
    {
        Assert.AreEqual("attack", attackAction.Keyword);
    }

    [Test]
    public void CanAffectWorld_ReturnsTrue()
    {
        Assert.IsTrue(attackAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsOptional()
    {
        Assert.AreEqual(ItemApplicabilityLevel.OPTIONAL, attackAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrue()
    {
        Assert.IsTrue(attackAction.CanApplyToItem(testItem));
    }

    [Test]
    public void Execute_ShowsDefaultMessageWhenNoItem()
    {
        var parser = new CommandParser();
        var parseResult = parser.ParseUserInput("attack", controller);
        var events = controller.ExecuteParsedCommand(parseResult);
        
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void Execute_ShowsCustomTextResponse()
    {
        testItem.textResponses = new List<TextResponse>
        {
            new TextResponse { actionKeyword = "attack", response = "You attack the item." }
        };
        testItem.RebuildActionLookupsPublic();
        
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var parseResult = parser.ParseUserInput("attack test item", controller);
        var events = controller.ExecuteParsedCommand(parseResult);
        
        Assert.Greater(events.Count, 0);
        bool hasCustomResponse = events.Exists(e => e.eventText.Contains("You attack the item"));
        Assert.IsTrue(hasCustomResponse);
    }

    [Test]
    public void Execute_RespectsRestrictions()
    {
        testItem.actionRestrictions = new List<ActionRestriction>
        {
            new ActionRestriction { actionKeyword = "attack", message = "You cannot attack that." }
        };
        
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var parseResult = parser.ParseUserInput("attack test item", controller);
        var events = controller.ExecuteParsedCommand(parseResult);
        
        Assert.Greater(events.Count, 0);
        bool hasRestriction = events.Exists(e => e.eventText.Contains("You cannot attack that"));
        Assert.IsTrue(hasRestriction);
    }
}

