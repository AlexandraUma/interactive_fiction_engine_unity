using NUnit.Framework;
using System.Collections.Generic;
using System;

public class ActionsManagerTests
{
    private ActionsManager actionsManager;
    private List<Action> testActions;

    [SetUp]
    public void SetUp()
    {
        testActions = new List<Action>
        {
            new Take(),
            new Go(),
            new Look()
        };
        actionsManager = new ActionsManager(testActions);
    }

    [Test]
    public void Constructor_ThrowsWhenActionsListIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ActionsManager(null));
    }

    [Test]
    public void Constructor_ThrowsWhenActionIsNull()
    {
        var actions = new List<Action> { null };
        Assert.Throws<ArgumentException>(() => new ActionsManager(actions));
    }

    [Test]
    public void Constructor_ThrowsWhenActionHasEmptyKeyword()
    {
        var invalidAction = new InvalidAction();
        var actions = new List<Action> { invalidAction };
        Assert.Throws<ArgumentException>(() => new ActionsManager(actions));
    }

    [Test]
    public void GetAction_ReturnsActionByKeyword()
    {
        var action = actionsManager.GetAction("take");
        Assert.IsNotNull(action);
        Assert.AreEqual("take", action.Keyword);
    }

    [Test]
    public void GetAction_ReturnsActionByAlias()
    {
        var action = actionsManager.GetAction("grab");
        Assert.IsNotNull(action);
        Assert.AreEqual("take", action.Keyword);
    }

    [Test]
    public void GetAction_ReturnsNullForUnknownKeyword()
    {
        var action = actionsManager.GetAction("xyzzy");
        Assert.IsNull(action);
    }

    [Test]
    public void GetAction_IsCaseInsensitive()
    {
        var action = actionsManager.GetAction("TAKE");
        Assert.IsNotNull(action);
        Assert.AreEqual("take", action.Keyword);
    }

    [Test]
    public void GetAction_ReturnsNullForNullOrEmptyKeyword()
    {
        Assert.IsNull(actionsManager.GetAction(null));
        Assert.IsNull(actionsManager.GetAction(""));
    }

    [Test]
    public void AddGlobalRestriction_AddsRestriction()
    {
        actionsManager.AddGlobalRestriction("take", "You cannot take items.");

        var controller = CreateTestController();
        var restriction = actionsManager.GetRestrictionMessage(
            controller,
            actionsManager.GetAction("take"),
            null
        );

        Assert.AreEqual("You cannot take items.", restriction);
    }

    [Test]
    public void AddGlobalRestriction_RemovesRestrictionWhenMessageIsEmpty()
    {
        actionsManager.AddGlobalRestriction("take", "You cannot take items.");
        actionsManager.AddGlobalRestriction("take", "");

        var controller = CreateTestController();
        var restriction = actionsManager.GetRestrictionMessage(
            controller,
            actionsManager.GetAction("take"),
            null
        );

        Assert.IsNull(restriction);
    }

    [Test]
    public void AddGlobalRestriction_ThrowsWhenKeywordIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => 
            actionsManager.AddGlobalRestriction("", "message"));
    }

    [Test]
    public void GetRestrictionMessage_ReturnsGlobalRestrictionFirst()
    {
        actionsManager.AddGlobalRestriction("take", "Global restriction");

        var controller = CreateTestController();
        var item = TestObjectFactory.CreateBaseObject("Item", "item");
        item.actionRestrictions = new List<ActionRestriction>
        {
            new ActionRestriction { actionKeyword = "take", message = "Item restriction" }
        };

        var restriction = actionsManager.GetRestrictionMessage(
            controller,
            actionsManager.GetAction("take"),
            item
        );

        Assert.AreEqual("Global restriction", restriction);
    }

    [Test]
    public void GetRestrictionMessage_ReturnsRoomRestrictionBeforeItemRestriction()
    {
        var controller = CreateTestController();
        var room = controller.objectsManager.CurrentRoom as Room;
        room.actionRestrictions = new List<ActionRestriction>
        {
            new ActionRestriction { actionKeyword = "take", message = "Room restriction" }
        };

        var item = TestObjectFactory.CreateBaseObject("Item", "item");
        item.actionRestrictions = new List<ActionRestriction>
        {
            new ActionRestriction { actionKeyword = "take", message = "Item restriction" }
        };

        var restriction = actionsManager.GetRestrictionMessage(
            controller,
            actionsManager.GetAction("take"),
            item
        );

        Assert.AreEqual("Room restriction", restriction);
    }

    [Test]
    public void GetRestrictionMessage_ThrowsWhenControllerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => 
            actionsManager.GetRestrictionMessage(null, new Take(), null));
    }

    [Test]
    public void GetRestrictionMessage_ThrowsWhenActionIsNull()
    {
        var controller = CreateTestController();
        Assert.Throws<ArgumentNullException>(() => 
            actionsManager.GetRestrictionMessage(controller, null, null));
    }

    private GameController CreateTestController()
    {
        var room = TestObjectFactory.CreateRoom("TestRoom", "Test Room");
        var player = TestObjectFactory.CreateCreature("Player", "player", isPlayer: true);
        var allRooms = new List<BaseObject> { room };
        var npcs = new List<BaseObject>();
        var allActions = RegisteredActions.Create();

        return new GameController(
            intro: "",
            prologue: "",
            startingRoom: room,
            allRooms: allRooms,
            playerCharacter: player,
            nonPlayerCharacters: npcs,
            allActions: allActions
        );
    }

    // Helper class for testing invalid actions
    private class InvalidAction : Action
    {
        public override string Keyword => "";
        public override List<string> Aliases => new List<string>();
        public override bool CanAffectWorld => false;
        public override ItemApplicabilityLevel ItemApplicabilityLevel => ItemApplicabilityLevel.NA;
        public override bool CanApplyToItem(BaseObject item) => true;
        public override ActionStatus Execute(GameController controller, BaseObject item) => ActionStatus.SUCCESSFUL;
    }
}

