using NUnit.Framework;
using System.Collections.Generic;

public class UnlockTests
{
    private Unlock unlockAction;
    private GameController controller;
    private Room testRoom;
    private Creature player;
    private BaseObject lockableItem;

    [SetUp]
    public void SetUp()
    {
        unlockAction = new Unlock();
        
        testRoom = TestObjectFactory.CreateRoom("TestRoom", "Test Room");
        player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        lockableItem = TestObjectFactory.CreateBaseObject("LockableItem", "lockable item");
        lockableItem.AddProperty(new Lockable(isLocked: true));

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
    public void Keyword_ReturnsUnlock()
    {
        Assert.AreEqual("unlock", unlockAction.Keyword);
    }

    [Test]
    public void CanAffectWorld_ReturnsTrue()
    {
        Assert.IsTrue(unlockAction.CanAffectWorld);
    }

    [Test]
    public void ItemApplicabilityLevel_ReturnsRequired()
    {
        Assert.AreEqual(ItemApplicabilityLevel.REQUIRED, unlockAction.ItemApplicabilityLevel);
    }

    [Test]
    public void CanApplyToItem_ReturnsTrueForLockableItem()
    {
        Assert.IsTrue(unlockAction.CanApplyToItem(lockableItem));
    }

    [Test]
    public void Execute_SuccessfullyUnlocksItem()
    {
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["lockable item"] = lockableItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "unlock lockable item");

        Assert.Greater(events.Count, 0);
        var lockable = lockableItem.GetProperty<Lockable>();
        Assert.IsFalse(lockable.IsLocked);
    }

    [Test]
    public void Execute_ReturnsIneffectiveWhenAlreadyUnlocked()
    {
        lockableItem.GetProperty<Lockable>().IsLocked = false;
        var roomContents = TestObjectFactory.EnsureRoomHasContents(testRoom);
        roomContents.Contents["lockable item"] = lockableItem;
        controller.objectsManager.SetCurrentRoom(testRoom);

        var parser = new CommandParser();
        var events = TestObjectFactory.ExecuteAction(controller, parser, "unlock lockable item");
        
        bool hasIneffectiveMessage = events.Exists(e => e.eventText.Contains("already unlocked"));
        Assert.IsTrue(hasIneffectiveMessage);
    }
}

