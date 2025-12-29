using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The controller manages the game. It knows, logs and controls, everything.
/// </summary>
public class GameController
{
    public static readonly string START_MESSAGE = $"Type '{SystemCommands.START}' to begin.";
    public static readonly string END_MESSAGE = "Thank you for playing!";

    public static readonly string[] EMPTY_INPUT_RESPONSES =
    {
        "I don't read minds you know.",
        "Did you say something?"
    };

    public static readonly string UNKNOWN_COMMAND_RESPONSE = "I'm not sure what you mean.";

    public string intro;
    public string prologue;
    public ActionsManager actionsManager;
    public ObjectsManager objectsManager;

    // During a controller call (StartGame, ExecuteParsedCommand, etc.) this
    // collects the IFEvents produced by actions and helpers. It is set by
    // the public API entrypoints and cleared when they return.
    private List<IFEvent> _currentEventBuffer;

    /// <summary>
    /// Initializes the game controller.
    /// </summary>
    /// <param name="intro">The introduction text.</param>
    /// <param name="prologue">The prologue text.</param>
    /// <param name="startingRoom">The starting room.</param>
    /// <param name="allRooms">List of all rooms.</param>
    /// <param name="playerCharacter">The player character.</param>
    /// <param name="nonPlayerCharacters">List of non-player characters.</param>
    /// <param name="allActions">List of all actions.</param>
    public GameController(
        string intro,
        string prologue,
        BaseObject startingRoom,
        List<BaseObject> allRooms,
        BaseObject playerCharacter,
        List<BaseObject> nonPlayerCharacters,
        List<Action> allActions)
    {
        this.intro = intro;
        this.prologue = prologue;

        // accept and validate all actions
        actionsManager = new ActionsManager(allActions: allActions);

        // accept and validate all objects
        objectsManager = new ObjectsManager(
            startingRoom: startingRoom,
            allRooms: allRooms,
            playerCharacter: playerCharacter,
            nonPlayerCharacters: nonPlayerCharacters
        );
    }

    /// <summary>
    /// Builds the intro and call-to-action events without mutating UI.
    /// The orchestrator is expected to display these events.
    /// </summary>
    public List<IFEvent> LogIntroAndCallToAction()
    {
        var events = new List<IFEvent>();
        _currentEventBuffer = events;
        try
        {
            Debug.Log("[GameController] Initialising...");
            if (!string.IsNullOrEmpty(intro))
            {
                LogEvent(eventText: intro, eventType: EventType.INTRO);
            }

            LogEvent(
                eventText: START_MESSAGE,
                eventType: EventType.CALL_TO_ACTION
            );

            return events;
        }
        finally
        {
            _currentEventBuffer = null;
        }
    }

    /// <summary>
    /// Starts the adventure by emitting intro, prologue and initial room
    /// description events. The orchestrator is responsible for displaying them.
    /// </summary>
    public List<IFEvent> StartGame()
    {
        var events = new List<IFEvent>();
        _currentEventBuffer = events;
        try
        {
            Debug.Log("[GameController] Starting the adventure...");

            if (!string.IsNullOrEmpty(intro))
            {
                LogEvent(eventText: intro, eventType: EventType.INTRO);
            }

            if (!string.IsNullOrEmpty(prologue))
            {
                LogEvent(eventText: prologue, eventType: EventType.PROLOGUE);
            }

            // The default "look" action will emit room description events.
            actionsManager.GetAction("look")?.Execute(this, item: null);

            return events;
        }
        finally
        {
            _currentEventBuffer = null;
        }
    }

    /// <summary>
    /// Currently just says 'Thank you for playing!'.
    /// </summary>
    public List<IFEvent> EndGame()
    {
        var events = new List<IFEvent>();
        _currentEventBuffer = events;
        try
        {
            Debug.Log("[GameController] Ending the game...");
            LogEvent(eventText: END_MESSAGE, eventType: EventType.END_MESSAGE);
            return events;
        }
        finally
        {
            _currentEventBuffer = null;
        }
    }

    /// <summary>
    /// Executes a parsed command and calls the appropriate action,
    /// returning all world events generated as a result.
    /// </summary>
    /// <param name="parserResponse">Result of parsing the user's input.</param>
    public List<IFEvent> ExecuteParsedCommand(ParseResult parserResponse)
    {
        var events = new List<IFEvent>();
        _currentEventBuffer = events;

        try
        {
            Debug.Log("[GameController] Processing parsed command...");

            // 1. First we see if the parser deemed the input empty, and if so, we handle it.
            if (parserResponse == null)
            {
                HandleEmptyInput();
                return events;
            }

            // 2. If the input was not empty, we extract the item and action from the parser response.
            (BaseObject item, string userGivenItemName) = ExtractItemInfo(parserResponse);
            Action action = parserResponse.Action;

            // 3. If the action is null, process it as a unknown command.
            if (action == null)
            {
                HandleUnknownCommand();
                return events;
            }

            // 4. If the action is not null, we check if it has any restrictions, and if so,
            // the restriction is the event that is logged.
            string restrictionMessage = actionsManager.GetRestrictionMessage(
                this, action, item
            );
            if (!string.IsNullOrEmpty(restrictionMessage))
            {
                LogEvent(
                    eventText: restrictionMessage,
                    eventType: EventType.WORLD_RESPONSE
                );
                return events;
            }

            // 5. If the action has no restrictions, we run the action.
            RunAction(action, item, userGivenItemName);
            return events;
        }
        finally
        {
            _currentEventBuffer = null;
        }
    }

    private void HandleEmptyInput()
    {
        var random = new System.Random();
        string[] responses = EMPTY_INPUT_RESPONSES;
        string response = responses[random.Next(responses.Length)];
        LogEvent(
            eventText: response,
            eventType: EventType.WORLD_RESPONSE
        );
    }

    // For now, we only allow for one item to be selected.
    // TODO: allow for multiple items to be selected.
    private (BaseObject item, string userGivenItemName) ExtractItemInfo(ParseResult parserResponse)
    {
        List<BaseObject> items = parserResponse.ItemsMatchingName ?? new List<BaseObject>();
        string userGivenItemName = parserResponse.ItemName ?? "";
        BaseObject selectedItem = items.Count > 0 ? items[0] : null;

        if (items.Count > 1)
        {
            LogEvent(
                eventText: (
                    $"(there are {items.Count} {userGivenItemName}. " +
                    $"I'm assuming you mean the '{selectedItem.name}'."
                ),
                eventType: EventType.WORLD_RESPONSE
            );
        }
        return (selectedItem, userGivenItemName);
    }

    private void HandleUnknownCommand()
    {
        LogEvent(
            eventText: UNKNOWN_COMMAND_RESPONSE,
            eventType: EventType.WORLD_RESPONSE
        );
    }


    private void RunAction(
        Action action, BaseObject item, string itemReference)
    {
        if (item != null)
        {
            ExecuteActionWithItem(action, item);
        }
        else
        {
            ExecuteActionWithoutItem(action, itemReference);
        }
    }

    // Executes action with item.
    private void ExecuteActionWithItem(Action action, BaseObject item)
    {
        if (action.ItemApplicabilityLevel == ItemApplicabilityLevel.NA)
        {
            ExecuteActionNoneApplicableItem(action);
        }
        else
        {
            ExecuteActionOnItem(action, item);
        }
    }

    // Executes action when item is not applicable.
    private void ExecuteActionNoneApplicableItem(Action action)
    {
        Debug.Log($"[GameController] Responding to {action.Keyword} action but ignoring item reference.");
        LogEvent(
            $"(Interpreting that command as simply to <{action.Keyword}>)",
            eventType: EventType.WORLD_RESPONSE
        );
        action.Execute(this, null);
    }

    private void ExecuteActionOnItem(Action action, BaseObject item)
    {
        Action overrideAction = item.GetOverrideFor(action.Keyword);
        Action actionToRun = overrideAction ?? action;

        ActionStatus? actionStatus = actionToRun.Execute(this, item);
        if (actionStatus == ActionStatus.SUCCESSFUL)
        {
            HandleActionResponse(actionToRun, item);
        }
    }

    private void HandleActionResponse(Action action, BaseObject item)
    {
        List<ActionResponseLogic> responses = item.GetActionResponsesFor(action.Keyword);
        if (responses == null || responses.Count == 0)
        {
            return;
        }

        Debug.Log($"[GameController] Running {responses.Count} action response(s) for <{action.Keyword}> on <{item.name}>");
        foreach (ActionResponseLogic response in responses)
        {
            if (response == null)
            {
                continue;
            }

            response.Execute(this, item);
        }
    }

    private void ExecuteActionWithoutItem(Action action, string itemReference)
    {
        if (action.ItemApplicabilityLevel == ItemApplicabilityLevel.REQUIRED)
        {
            HandleMissingRequiredItem(action, itemReference);
        }
        else
        {
            ExecuteActionNoItemRequired(action);
        }
    }

    private void HandleMissingRequiredItem(
        Action action, string itemReference)
    {
        if (!string.IsNullOrEmpty(itemReference))
        {
            LogItemRequiredButNotFound(action, itemReference);
        }
        else
        {
            LogItemRequiredButNotProvided(action);
        }
    }

    // Logs event when item required for action is not found.
    private void LogItemRequiredButNotFound(
        Action action, string itemReference)
    {
        Debug.Log($"[GameController] An item required by the {action.Keyword} action was not found: {itemReference}");
        string response = action.Keyword == "go"
            ? $"There's no exit to the {itemReference}."
            : $"There's no {itemReference} here.";
        LogEvent(eventText: response, eventType: EventType.WORLD_RESPONSE);
    }

    // Logs event when item required for action is not provided.
    private void LogItemRequiredButNotProvided(Action action)
    {
        Debug.Log($"[GameController] An item required by the {action.Keyword} action was not provided.");
        string question = action.Keyword == "go" ? "Where" : "What";
        LogEvent(
            $"{question} exactly do you want to {action.Keyword}?",
            eventType: EventType.WORLD_RESPONSE
        );
    }

    // Executes action without item required.
    private void ExecuteActionNoItemRequired(Action action)
    {
        Debug.Log($"[GameController] Responding to {action.Keyword} action. No item was given and none was required.");
        action.Execute(controller: this, item: null);
    }

    /// <summary>
    /// Internal helper used by actions and helpers to record game events.
    /// This does not touch UI; it only appends to the active event buffer.
    /// </summary>
    public void LogEvent(string eventText, EventType eventType)
    {
        if (_currentEventBuffer == null)
        {
            throw new InvalidOperationException(
                "LogEvent was called with no active event buffer. " +
                "GameController public methods must manage event collection.");
        }

        if (!string.IsNullOrEmpty(eventText))
        {
            _currentEventBuffer.Add(new IFEvent(eventType, eventText));
        }
    }
}
