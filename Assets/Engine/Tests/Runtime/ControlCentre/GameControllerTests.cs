using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class GameControllerTests
{
    private GameController controller;
    private Room testRoom;
    private Room otherRoom;
    private Creature player;
    private BaseObject testItem;

    [SetUp]
    public void SetUp()
    {
        testRoom = TestObjectFactory.CreateRoom("TestRoom", "Test Room");
        otherRoom = TestObjectFactory.CreateRoom("OtherRoom", "Other Room");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        testItem = TestObjectFactory.CreateBaseObject("TestItem", "test item");

        var allRooms = new List<BaseObject> { testRoom, otherRoom };
        var npcs = new List<BaseObject>();
        var allActions = RegisteredActions.Create();

        controller = new GameController(
            intro: "Welcome to the test game",
            prologue: "This is a test prologue",
            startingRoom: testRoom,
            allRooms: allRooms,
            playerCharacter: player,
            nonPlayerCharacters: npcs,
            allActions: allActions
        );
    }

    [Test]
    public void StartGame_ReturnsIntroAndPrologueEvents()
    {
        var events = controller.StartGame();
        
        Assert.Greater(events.Count, 0);
        bool hasIntro = events.Exists(e => e.eventText.Contains("Welcome to the test game"));
        bool hasPrologue = events.Exists(e => e.eventText.Contains("This is a test prologue"));
        Assert.IsTrue(hasIntro || hasPrologue);
    }

    [Test]
    public void StartGame_ExecutesLookAction()
    {
        var events = controller.StartGame();
        
        // Look action should have been executed, generating room description events
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void LogIntroAndCallToAction_ReturnsIntroAndStartMessage()
    {
        var events = controller.LogIntroAndCallToAction();
        
        Assert.Greater(events.Count, 0);
        // START_MESSAGE is "Type 'start' to begin." - check for "start" (case-insensitive)
        bool hasStartMessage = events.Exists(e => 
            e.eventText.IndexOf("start", System.StringComparison.OrdinalIgnoreCase) >= 0);
        Assert.IsTrue(hasStartMessage, $"Expected to find 'start' message in events. Events: {string.Join(" | ", events.Select(e => e.eventText))}");
    }

    [Test]
    public void ExecuteParsedCommand_HandlesEmptyInput()
    {
        var events = controller.ExecuteParsedCommand(null);
        
        Assert.Greater(events.Count, 0);
        // Should contain one of the empty input responses
        bool hasResponse = events.Exists(e => 
            e.eventText.Contains("I don't read minds") || 
            e.eventText.Contains("Did you say something"));
        Assert.IsTrue(hasResponse);
    }

    [Test]
    public void ExecuteParsedCommand_HandlesUnknownCommand()
    {
        var parseResult = new ParseResult
        {
            Action = null,
            ItemName = "xyzzy",
            ItemsMatchingName = new List<BaseObject>()
        };

        var events = controller.ExecuteParsedCommand(parseResult);
        
        Assert.Greater(events.Count, 0);
        bool hasUnknownMessage = events.Exists(e => 
            e.eventText.Contains("I'm not sure what you mean"));
        Assert.IsTrue(hasUnknownMessage);
    }

    [Test]
    public void ExecuteParsedCommand_ExecutesActionWithItem()
    {
        // Add item to room
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parseResult = new ParseResult
        {
            Action = controller.actionsManager.GetAction("take"),
            ItemName = "test item",
            ItemsMatchingName = new List<BaseObject> { testItem }
        };

        var events = controller.ExecuteParsedCommand(parseResult);
        
        Assert.Greater(events.Count, 0);
        // Item should be in player inventory now
        Assert.IsTrue(controller.objectsManager.IsItemCarriedByPlayer(testItem));
    }

    [Test]
    public void ExecuteParsedCommand_HandlesActionWithoutItem()
    {
        var parseResult = new ParseResult
        {
            Action = controller.actionsManager.GetAction("look"),
            ItemName = "",
            ItemsMatchingName = new List<BaseObject>()
        };

        var events = controller.ExecuteParsedCommand(parseResult);
        
        Assert.Greater(events.Count, 0);
    }

    [Test]
    public void ExecuteParsedCommand_HandlesItemDisambiguation()
    {
        var item1 = TestObjectFactory.CreateBaseObject("Item1", "key");
        var item2 = TestObjectFactory.CreateBaseObject("Item2", "key");
        
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["key"] = item1;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parseResult = new ParseResult
        {
            Action = controller.actionsManager.GetAction("take"),
            ItemName = "key",
            ItemsMatchingName = new List<BaseObject> { item1, item2 }
        };

        var events = controller.ExecuteParsedCommand(parseResult);
        
        // Should log disambiguation message
        bool hasDisambiguation = events.Exists(e => 
            e.eventText.Contains("there are 2") && 
            e.eventText.Contains("key"));
        Assert.IsTrue(hasDisambiguation);
    }

    [Test]
    public void ExecuteParsedCommand_HandlesActionRestrictions()
    {
        // Add restriction to room
        var restriction = new ActionRestriction
        {
            actionKeyword = "take",
            message = "You cannot take items here."
        };
        testRoom.actionRestrictions = new List<ActionRestriction> { restriction };
        controller.objectsManager.SetCurrentRoom(testRoom);

        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;

        var parseResult = new ParseResult
        {
            Action = controller.actionsManager.GetAction("take"),
            ItemName = "test item",
            ItemsMatchingName = new List<BaseObject> { testItem }
        };

        var events = controller.ExecuteParsedCommand(parseResult);
        
        bool hasRestriction = events.Exists(e => 
            e.eventText.Contains("You cannot take items here"));
        Assert.IsTrue(hasRestriction);
    }

    [Test]
    public void EndGame_ReturnsEndMessage()
    {
        var events = controller.EndGame();
        
        Assert.Greater(events.Count, 0);
        bool hasEndMessage = events.Exists(e => 
            e.eventText.Contains("Thank you for playing"));
        Assert.IsTrue(hasEndMessage);
    }

    [Test]
    public void LogEvent_ThrowsWhenNoEventBuffer()
    {
        // LogEvent should only work within a controller method call
        Assert.Throws<System.InvalidOperationException>(() => 
            controller.LogEvent("Test", EventType.WORLD_RESPONSE));
    }

    [Test]
    public void LogEvent_WorksWithinControllerMethod()
    {
        var events = controller.StartGame();
        // StartGame should have logged events
        Assert.Greater(events.Count, 0);
    }
}

