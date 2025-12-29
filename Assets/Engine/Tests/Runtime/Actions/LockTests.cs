using NUnit.Framework;
using System.Collections.Generic;

public class LockTests
{
    private Lock lockAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject lockableItem;
    private BaseObject nonLockableItem;

    [SetUp]
    public void SetUp()
    {
        lockAction = new Lock();
        
        testRoom = TestObjectFactory.CreateRoom("TestRoom", "Test Room");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        lockableItem = TestObjectFactory.CreateBaseObject("LockableItem", "lockable item");
        lockableItem.AddProperty(new Lockable(isLocked: false));
        nonLockableItem = TestObjectFactory.CreateBaseObject("NonLockableItem", "non-lockable item");

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
    public void Keyword_ReturnsLock()
    {
        Assert.AreEqual("lock", lockAction.Keyword);
    }

    [Test]
    public void CanAffectWorld_ReturnsTrue()
    {
        Assert.IsTrue(lockAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsRequired()
    {
        Assert.AreEqual(ItemApplicabilityLevel.REQUIRED, lockAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrueForLockableItem()
    {
        Assert.IsTrue(lockAction.CanApplyToItem(lockableItem));
    }

    [Test]
    public void CanApplyToItem_ReturnsFalseForNonLockableItem()
    {
        Assert.IsFalse(lockAction.CanApplyToItem(nonLockableItem));
    }

    [Test]
    public void Execute_SuccessfullyLocksItem()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["lockable item"] = lockableItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "lock lockable item");

        Assert.Greater(events.Count, 0);
        var lockable = lockableItem.GetProperty<Lockable>();
        Assert.IsTrue(lockable.IsLocked);
    }

    [Test]
    public void Execute_ReturnsIneffectiveWhenAlreadyLocked()
    {
        lockableItem.GetProperty<Lockable>().IsLocked = true;
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["lockable item"] = lockableItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "lock lockable item");
        
        bool hasIneffectiveMessage = events.Exists(e => e.eventText.Contains("already locked"));
        Assert.IsTrue(hasIneffectiveMessage);
    }

    [Test]
    public void Execute_ReturnsFailedForNonLockableItem()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["non-lockable item"] = nonLockableItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "lock non-lockable item");
        
        bool hasFailedMessage = events.Exists(e => e.eventText.Contains("can't lock"));
        Assert.IsTrue(hasFailedMessage);
    }
}

