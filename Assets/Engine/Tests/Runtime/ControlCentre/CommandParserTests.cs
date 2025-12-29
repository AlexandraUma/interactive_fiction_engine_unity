using NUnit.Framework;
using System.Collections.Generic;

public class CommandParserTests
{
    private CommandParser parser;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject testItem;

    [SetUp]
    public void SetUp()
    {
        parser = new CommandParser();
        
        testRoom = TestObjectFactory.CreateRoom("TestRoom", "Test Room");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        testItem = TestObjectFactory.CreateBaseObject("TestItem", "test item", new List<string> { "item" });

        var allRooms = new List<BaseObject> { testRoom };
        var npcs = new List<BaseObject>();
        var allActions = RegisteredActions.Create();

        controller = new GameController(
            intro: "Test intro",
            prologue: "Test prologue",
            startingRoom: testRoom,
            allRooms: allRooms,
            playerCharacter: player,
            nonPlayerCharacters: npcs,
            allActions: allActions
        );

        // Add item to room
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["test item"] = testItem;
        controller.objectsManager.SetCurrentRoom(testRoom);
    }

    [Test]
    public void ParseUserInput_EnrichesDirectionKeywords()
    {
        var result = parser.ParseUserInput("north", controller);
        Assert.IsNotNull(result.Action);
        Assert.AreEqual("go", result.Action.Keyword);
    }

    [Test]
    public void ParseUserInput_HandlesShortDirectionKeywords()
    {
        var result = parser.ParseUserInput("n", controller);
        Assert.IsNotNull(result.Action);
        Assert.AreEqual("go", result.Action.Keyword);
    }

    [Test]
    public void ParseUserInput_ExtractsActionKeyword()
    {
        var result = parser.ParseUserInput("take test item", controller);
        Assert.IsNotNull(result.Action);
        Assert.AreEqual("take", result.Action.Keyword);
    }

    [Test]
    public void ParseUserInput_ExtractsMultiWordActionKeyword()
    {
        var result = parser.ParseUserInput("pick up test item", controller);
        Assert.IsNotNull(result.Action);
        Assert.AreEqual("take", result.Action.Keyword); // "pick up" is an alias for "take"
    }

    [Test]
    public void ParseUserInput_ExtractsItemName()
    {
        var result = parser.ParseUserInput("take test item", controller);
        Assert.AreEqual("test item", result.ItemName);
        Assert.IsNotNull(result.ItemsMatchingName);
        Assert.Greater(result.ItemsMatchingName.Count, 0);
    }

    [Test]
    public void ParseUserInput_HandlesEmptyInput()
    {
        var result = parser.ParseUserInput("", controller);
        Assert.IsNull(result.Action);
        Assert.AreEqual("", result.ItemName);
    }

    [Test]
    public void ParseUserInput_HandlesWhitespaceOnlyInput()
    {
        var result = parser.ParseUserInput("   ", controller);
        Assert.IsNull(result.Action);
    }

    [Test]
    public void ParseUserInput_HandlesUnknownAction()
    {
        var result = parser.ParseUserInput("xyzzy test item", controller);
        Assert.IsNull(result.Action);
        Assert.AreEqual("xyzzy test item", result.ItemName);
    }

    [Test]
    public void ParseUserInput_HandlesUnknownItem()
    {
        var result = parser.ParseUserInput("take nonexistent item", controller);
        Assert.IsNotNull(result.Action);
        Assert.AreEqual("nonexistent item", result.ItemName);
        Assert.AreEqual(0, result.ItemsMatchingName.Count);
    }

    [Test]
    public void ParseUserInput_IsCaseInsensitive()
    {
        var result = parser.ParseUserInput("TAKE TEST ITEM", controller);
        Assert.IsNotNull(result.Action);
        Assert.AreEqual("take", result.Action.Keyword);
        Assert.Greater(result.ItemsMatchingName.Count, 0);
    }

    [Test]
    public void ParseUserInput_HandlesMultiWordItemNames()
    {
        var multiWordItem = TestObjectFactory.CreateBaseObject("MultiWordItem", "red leather book");
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["red leather book"] = multiWordItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var result = parser.ParseUserInput("take red leather book", controller);
        Assert.AreEqual("red leather book", result.ItemName);
        Assert.Greater(result.ItemsMatchingName.Count, 0);
    }

    [Test]
    public void ParseUserInput_HandlesActionWithoutItem()
    {
        var result = parser.ParseUserInput("look", controller);
        Assert.IsNotNull(result.Action);
        Assert.AreEqual("look", result.Action.Keyword);
        Assert.AreEqual("", result.ItemName);
    }

    [Test]
    public void ParseUserInput_HandlesAllDirectionVariants()
    {
        string[] directions = { "north", "south", "east", "west", "northeast", "northwest", 
                               "southeast", "southwest", "up", "down", "n", "s", "e", "w", 
                               "ne", "nw", "se", "sw", "u", "d" };
        
        foreach (var direction in directions)
        {
            var result = parser.ParseUserInput(direction, controller);
            Assert.IsNotNull(result.Action, $"Direction '{direction}' should be enriched");
            Assert.AreEqual("go", result.Action.Keyword, $"Direction '{direction}' should map to 'go'");
        }
    }
}

