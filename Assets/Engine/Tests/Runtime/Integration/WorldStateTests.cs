using NUnit.Framework;
using System.Collections.Generic;

public class WorldStateTests
{
    private GameController controller;
    private Room room1;
    private Room room2;
    private Creature player;
    private BaseObject item1;
    private BaseObject item2;

    [SetUp]
    public void SetUp()
    {
        room1 = TestObjectFactory.CreateRoom("Room1", "Room 1");
        room2 = TestObjectFactory.CreateRoom("Room2", "Room 2");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        item1 = TestObjectFactory.CreateBaseObject("Item1", "item one");
        item2 = TestObjectFactory.CreateBaseObject("Item2", "item two");

        var allRooms = new List<BaseObject> { room1, room2 };
        var npcs = new List<BaseObject>();
        var allActions = RegisteredActions.Create();

        controller = new GameController(
            intro: "",
            prologue: "",
            startingRoom: room1,
            allRooms: allRooms,
            playerCharacter: player,
            nonPlayerCharacters: npcs,
            allActions: allActions
        );
    }

    [Test]
    public void WorldState_ItemConsistencyAfterTake()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(room1);
        roomContents.Contents["item one"] = item1;
        controller.objectsManager.SetCurrentRoom(room1);

        // Item should be in room
        var items = controller.objectsManager.GetAllItemsMatchingName("item one");
        Assert.Contains(item1, items);

        // Take item
        var parser = new CommandParser();
        var parseResult = parser.ParseUserInput("take item one", controller);
        controller.ExecuteParsedCommand(parseResult);

        // Item should be in inventory, not in room
        Assert.IsTrue(controller.objectsManager.IsItemCarriedByPlayer(item1));
        items = controller.objectsManager.GetAllItemsMatchingName("item one");
        // Item should still be findable (in inventory)
        Assert.Greater(items.Count, 0);
    }

    [Test]
    public void WorldState_RoomTransitionPreservesInventory()
    {
        controller.objectsManager.AddItemToPlayer(item1);
        controller.objectsManager.AddItemToPlayer(item2);

        // Move to different room
        controller.objectsManager.SetCurrentRoom(room2);

        // Items should still be in inventory
        Assert.IsTrue(controller.objectsManager.IsItemCarriedByPlayer(item1));
        Assert.IsTrue(controller.objectsManager.IsItemCarriedByPlayer(item2));
    }

    [Test]
    public void WorldState_ContainerNesting()
    {
        var outerContainer = TestObjectFactory.CreateBaseObject("OuterContainer", "outer container");
        outerContainer.AddProperty(new HoldsContents());
        var outerContents = outerContainer.GetProperty<HoldsContents>();

        var innerContainer = TestObjectFactory.CreateBaseObject("InnerContainer", "inner container");
        innerContainer.AddProperty(new HoldsContents());
        var innerContents = innerContainer.GetProperty<HoldsContents>();
        innerContents.Contents["item one"] = item1;

        outerContents.Contents["inner container"] = innerContainer;

        var roomContents = TestObjectFactory.EnsureRoomHasContents(room1);
        roomContents.Contents["outer container"] = outerContainer;
        controller.objectsManager.SetCurrentRoom(room1);

        // Nested item should be accessible
        var items = controller.objectsManager.GetAllItemsMatchingName("item one");
        Assert.Greater(items.Count, 0);
    }

    [Test]
    public void WorldState_RoomVisitCounting()
    {
        int initialVisits = room2.numVisits;
        controller.objectsManager.SetCurrentRoom(room2);
        Assert.AreEqual(initialVisits + 1, room2.numVisits);

        controller.objectsManager.SetCurrentRoom(room1);
        controller.objectsManager.SetCurrentRoom(room2);
        Assert.AreEqual(initialVisits + 2, room2.numVisits);
    }
}

