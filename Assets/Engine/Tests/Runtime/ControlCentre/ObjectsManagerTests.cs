using NUnit.Framework;
using System.Collections.Generic;
using System;

public class ObjectsManagerTests
{
    private Room startingRoom;
    private Room otherRoom;
    private Creature player;
    private BaseObject testItem;
    private ObjectsManager objectsManager;

    [SetUp]
    public void SetUp()
    {
        startingRoom = TestObjectFactory.CreateRoom("StartingRoom", "Starting Room");
        otherRoom = TestObjectFactory.CreateRoom("OtherRoom", "Other Room");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        testItem = TestObjectFactory.CreateBaseObject("TestItem", "test item", new List<string> { "item" });

        var allRooms = new List<BaseObject> { startingRoom, otherRoom };
        var npcs = new List<BaseObject>();

        objectsManager = new ObjectsManager(
            startingRoom: startingRoom,
            allRooms: allRooms,
            playerCharacter: player,
            nonPlayerCharacters: npcs
        );
    }

    [Test]
    public void Constructor_ThrowsWhenStartingRoomIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ObjectsManager(
            startingRoom: null,
            allRooms: new List<BaseObject>(),
            playerCharacter: player,
            nonPlayerCharacters: new List<BaseObject>()
        ));
    }

    [Test]
    public void Constructor_ThrowsWhenStartingRoomIsNotRoom()
    {
        var notARoom = TestObjectFactory.CreateBaseObject("NotRoom", "not a room");
        Assert.Throws<ArgumentException>(() => new ObjectsManager(
            startingRoom: notARoom,
            allRooms: new List<BaseObject>(),
            playerCharacter: player,
            nonPlayerCharacters: new List<BaseObject>()
        ));
    }

    [Test]
    public void Constructor_ThrowsWhenPlayerCharacterIsNotCreature()
    {
        var notACreature = TestObjectFactory.CreateBaseObject("NotCreature", "not a creature");
        Assert.Throws<ArgumentException>(() => new ObjectsManager(
            startingRoom: startingRoom,
            allRooms: new List<BaseObject>(),
            playerCharacter: notACreature,
            nonPlayerCharacters: new List<BaseObject>()
        ));
    }

    [Test]
    public void CurrentRoom_ReturnsStartingRoom()
    {
        Assert.AreEqual(startingRoom, objectsManager.CurrentRoom);
    }

    [Test]
    public void PlayerCharacter_ReturnsPlayer()
    {
        Assert.AreEqual(player, objectsManager.PlayerCharacter);
    }

    [Test]
    public void SetCurrentRoom_ChangesCurrentRoom()
    {
        objectsManager.SetCurrentRoom(otherRoom);
        Assert.AreEqual(otherRoom, objectsManager.CurrentRoom);
    }

    [Test]
    public void SetCurrentRoom_IncrementsNumVisits()
    {
        int initialVisits = otherRoom.numVisits;
        objectsManager.SetCurrentRoom(otherRoom);
        Assert.AreEqual(initialVisits + 1, otherRoom.numVisits);
    }

    [Test]
    public void SetCurrentRoom_ClearsPreviousRoomCollections()
    {
        // Add item to starting room
        var roomContents = TestObjectFactory.EnsureRoomHasContents(startingRoom);
        roomContents.Contents["test item"] = testItem;
        objectsManager.SetCurrentRoom(startingRoom);

        var items = objectsManager.GetAllItemsMatchingName("test item");
        Assert.Greater(items.Count, 0);

        // Move to other room
        objectsManager.SetCurrentRoom(otherRoom);
        
        // Starting room items should no longer be accessible
        items = objectsManager.GetAllItemsMatchingName("test item");
        Assert.AreEqual(0, items.Count);
    }

    [Test]
    public void AddItemToRoom_AddsItemToRoom()
    {
        objectsManager.SetCurrentRoom(startingRoom);
        objectsManager.AddItemToRoom(testItem);

        var items = objectsManager.GetAllItemsMatchingName("test item");
        Assert.Greater(items.Count, 0);
        Assert.Contains(testItem, items);
    }

    [Test]
    public void RemoveItemFromRoom_RemovesItemFromRoom()
    {
        objectsManager.SetCurrentRoom(startingRoom);
        var roomContents = TestObjectFactory.EnsureRoomHasContents(startingRoom);
        roomContents.Contents["test item"] = testItem;
        objectsManager.SetCurrentRoom(startingRoom);

        objectsManager.RemoveItemFromRoom(testItem);

        var items = objectsManager.GetAllItemsMatchingName("test item");
        Assert.AreEqual(0, items.Count);
    }

    [Test]
    public void AddItemToPlayer_AddsItemToInventory()
    {
        objectsManager.AddItemToPlayer(testItem);

        Assert.IsTrue(objectsManager.IsItemCarriedByPlayer(testItem));
        var carriedItems = objectsManager.GetItemsCarriedByPlayer();
        Assert.Contains(testItem, carriedItems);
    }

    [Test]
    public void RemoveItemFromPlayer_RemovesItemFromInventory()
    {
        objectsManager.AddItemToPlayer(testItem);
        objectsManager.RemoveItemFromPlayer(testItem);

        Assert.IsFalse(objectsManager.IsItemCarriedByPlayer(testItem));
    }

    [Test]
    public void IsItemCarriedByPlayer_ReturnsTrueWhenCarried()
    {
        objectsManager.AddItemToPlayer(testItem);
        Assert.IsTrue(objectsManager.IsItemCarriedByPlayer(testItem));
    }

    [Test]
    public void IsItemCarriedByPlayer_ReturnsFalseWhenNotCarried()
    {
        Assert.IsFalse(objectsManager.IsItemCarriedByPlayer(testItem));
    }

    [Test]
    public void GetAllItemsMatchingName_FindsItemsInRoom()
    {
        objectsManager.SetCurrentRoom(startingRoom);
        var roomContents = TestObjectFactory.EnsureRoomHasContents(startingRoom);
        roomContents.Contents["test item"] = testItem;
        objectsManager.SetCurrentRoom(startingRoom);

        var items = objectsManager.GetAllItemsMatchingName("test item");
        Assert.Greater(items.Count, 0);
        Assert.Contains(testItem, items);
    }

    [Test]
    public void GetAllItemsMatchingName_FindsItemsInInventory()
    {
        objectsManager.AddItemToPlayer(testItem);

        var items = objectsManager.GetAllItemsMatchingName("test item");
        Assert.Greater(items.Count, 0);
        Assert.Contains(testItem, items);
    }

    [Test]
    public void GetAllItemsMatchingName_FindsItemsInBothRoomAndInventory()
    {
        var item2 = TestObjectFactory.CreateBaseObject("TestItem2", "test item");
        
        objectsManager.SetCurrentRoom(startingRoom);
        var roomContents = TestObjectFactory.EnsureRoomHasContents(startingRoom);
        roomContents.Contents["test item"] = testItem;
        objectsManager.SetCurrentRoom(startingRoom);
        objectsManager.AddItemToPlayer(item2);

        var items = objectsManager.GetAllItemsMatchingName("test item");
        Assert.AreEqual(2, items.Count);
    }

    [Test]
    public void GetAllItemsMatchingName_IsCaseInsensitive()
    {
        objectsManager.AddItemToPlayer(testItem);

        var items = objectsManager.GetAllItemsMatchingName("TEST ITEM");
        Assert.Greater(items.Count, 0);
    }

    [Test]
    public void GetAllItemsMatchingName_HandlesAliases()
    {
        objectsManager.AddItemToPlayer(testItem);

        var items = objectsManager.GetAllItemsMatchingName("item");
        Assert.Greater(items.Count, 0);
    }

    [Test]
    public void UnpackRoomItems_UnpacksNestedContainers()
    {
        var container = TestObjectFactory.CreateBaseObject("Container", "container");
        container.AddProperty(new HoldsContents());
        var containerContents = container.GetProperty<HoldsContents>();
        containerContents.Contents["nested item"] = testItem;

        objectsManager.SetCurrentRoom(startingRoom);
        var roomContents = TestObjectFactory.EnsureRoomHasContents(startingRoom);
        roomContents.Contents["container"] = container;
        objectsManager.SetCurrentRoom(startingRoom);

        // Items are indexed by their mainName and aliases, not the dictionary key
        // testItem has mainName "test item", so search for that
        var items = objectsManager.GetAllItemsMatchingName("test item");
        Assert.Greater(items.Count, 0);
        Assert.Contains(testItem, items);
    }

    [Test]
    public void UnpackRoomItems_IncludesExits()
    {
        var exit = TestObjectFactory.CreateExit("north", otherRoom);
        startingRoom.exits = new List<Exit> { exit };
        objectsManager.SetCurrentRoom(startingRoom);

        var items = objectsManager.GetAllItemsMatchingName("north");
        Assert.Greater(items.Count, 0);
    }

    [Test]
    public void ClearCollectionsForNewRoom_ClearsCollections()
    {
        objectsManager.SetCurrentRoom(startingRoom);
        var roomContents = TestObjectFactory.EnsureRoomHasContents(startingRoom);
        roomContents.Contents["test item"] = testItem;
        objectsManager.SetCurrentRoom(startingRoom);

        objectsManager.ClearCollectionsForNewRoom();

        var items = objectsManager.GetAllItemsMatchingName("test item");
        Assert.AreEqual(0, items.Count);
    }
}

