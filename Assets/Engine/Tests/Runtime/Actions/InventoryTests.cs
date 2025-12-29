using NUnit.Framework;
using System.Collections.Generic;

public class InventoryTests
{
    private Inventory inventoryAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject item1;
    private BaseObject item2;

    [SetUp]
    public void SetUp()
    {
        inventoryAction = new Inventory();
        
        testRoom = TestObjectFactory.CreateRoom("TestRoom", "Test Room");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        item1 = TestObjectFactory.CreateBaseObject("Item1", "item one");
        item2 = TestObjectFactory.CreateBaseObject("Item2", "item two");

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
    public void Keyword_ReturnsInventory()
    {
        Assert.AreEqual("inventory", inventoryAction.Keyword);
    }

    [Test]
    public void Aliases_ContainsExpectedAliases()
    {
        Assert.Contains("i", inventoryAction.Aliases);
        Assert.Contains("inv", inventoryAction.Aliases);
    }

    [Test]
    public void CanAffectWorld_ReturnsFalse()
    {
        Assert.IsFalse(inventoryAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsNA()
    {
        Assert.AreEqual(ItemApplicabilityLevel.NA, inventoryAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrue()
    {
        Assert.IsTrue(inventoryAction.CanApplyToItem(item1));
    }

    [Test]
    public void Execute_ShowsEmptyInventoryMessage()
    {
        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "inventory");
        Assert.Greater(events.Count, 0);
        bool hasEmptyMessage = events.Exists(e => e.eventText.Contains("not carrying"));
        Assert.IsTrue(hasEmptyMessage);
    }

    [Test]
    public void Execute_ShowsInventoryWithItems()
    {
        controller.objectsManager.AddItemToPlayer(item1);
        controller.objectsManager.AddItemToPlayer(item2);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "inventory");
        Assert.Greater(events.Count, 0);
        bool hasInventoryList = events.Exists(e => e.eventText.Contains("carrying"));
        Assert.IsTrue(hasInventoryList);
    }
}

