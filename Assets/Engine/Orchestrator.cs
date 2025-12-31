using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Orchestrator : MonoBehaviour
{
    // For typing effect
    private Queue<string> messageQueue = new Queue<string>();
    private bool isTyping = false;
    public float typeSpeed = 0.02f;

    [Header("UI References")]
    public TMP_Text historyText;
    public TMP_InputField inputField;
    public ScrollRect scrollRect;

    [Header("Story Elements")]
    [TextArea]
    public string intro = "";
    [TextArea]
    public string prologue = "";

    [Header("Rooms")]
    public BaseObject startingRoom;
    public List<BaseObject> allRooms;

    [Header("Characters")]
    public BaseObject playerCharacter;
    public List<BaseObject> nonPlayerCharacters;

    [Header("Actions")]
    private List<Action> registeredActions = RegisteredActions.Create();
    public List<Action> customActions = new();

    [Header("Control Centre")]
    private CommandParser parser;
    private GameController controller;

    public void Start()
    {
        Debug.Log("Starting the application...");

        // When play starts, clear the text area and display the intro
        historyText.text = "";

        // Setup: add listeners
        inputField.onSubmit.AddListener(_ => ProcessPlayerInput(inputField.text));

        // Initialise the controller and parser
        parser = new CommandParser();
        controller = new GameController(
            intro: intro,
            prologue: prologue,
            startingRoom: startingRoom,
            allRooms: allRooms,
            playerCharacter: playerCharacter,
            nonPlayerCharacters: nonPlayerCharacters,
            allActions: registeredActions.Concat(customActions).ToList()
        );

        List<IFEvent> initialEvents = controller.StartGame();
        foreach (IFEvent gameEvent in initialEvents)
        {
            DisplayEvent(gameEvent);
        }

        // Activate the input field
        inputField.ActivateInputField();
    }

    public void ProcessPlayerInput(string input)
    {
        Debug.Log($"Processing player input: {input}");

        // Mirror the player's input back to them
        var playerInputEvent = new IFEvent(type: EventType.PLAYER_INPUT, text: input);
        DisplayEvent(playerInputEvent);

        // Check for system commands first
        string trimmedInput = input.Trim().ToLower();
        if (trimmedInput == SystemCommands.QUIT || trimmedInput == "exit" || trimmedInput == "q")
        {
            List<IFEvent> endEvents = controller.EndGame();
            foreach (IFEvent gameEvent in endEvents)
            {
                DisplayEvent(gameEvent);
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }

        // Command parser generates a parsed command based on the player's input.
        // The parser "speaks for the player" and consults the controller (who speaks
        // for the game) when it needs to resolve item references.
        ParseResult parserResponse = parser.ParseUserInput(
            input,
            controller
        );

        // Game controller processes the parsed command and returns resulting events
        List<IFEvent> events = controller.ExecuteParsedCommand(parserResponse);
        foreach (IFEvent gameEvent in events)
        {
            DisplayEvent(gameEvent);
        }

        // Reset the input field
        inputField.text = "";
        inputField.ActivateInputField();

        // Force scroll to bottom
        StartCoroutine(ScrollToBottom());
    }

    public void DisplayEvent(IFEvent gameEvent)
    {
        string formattedEvent = TextFormatter.FormatEvent(gameEvent);
        LogText(formattedEvent);
    }

    // Use this to log everything now
    public void LogText(string message)
    {
        messageQueue.Enqueue(message);
        if (!isTyping)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    IEnumerator ProcessQueue()
    {
        isTyping = true;
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            historyText.text += "\n";

            foreach (char letter in message.ToCharArray())
            {
                historyText.text += letter;
                // Auto-scroll logic
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
                yield return new WaitForSeconds(typeSpeed);
            }
            // Small pause between paragraphs
            yield return new WaitForSeconds(0.1f);
        }
        isTyping = false;
    }

    IEnumerator ScrollToBottom()
    {
        // Wait for the end of the frame so TMP can update the text height
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }

}
