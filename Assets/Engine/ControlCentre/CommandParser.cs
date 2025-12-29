using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* The CommandParser class is responsible for parsing the user input and
returning the actions and items if any.

Currently assumes only one command per input.
*/
public class CommandParser
{

    /* Common directions that should be prefixed with "go" */
    private static readonly string[] ALL_POSSIBLE_DIRECTIONS =
    {
        "north", "south", "east", "west",
        "northeast", "northwest", "southeast", "southwest",
        "n", "s", "e", "w", "ne", "nw", "se", "sw",
        "up", "down", "u", "d"
    };

    /// <summary>
    /// Parses the input text and returns the input action and the selected item if any.
    /// The parser "speaks for the player" and consults the controller (who speaks for
    /// the game) when it needs to resolve both actions and item references.
    /// </summary>
    /// <param name="inputText">The input text from the player.</param>
    /// <param name="controller">The game controller, used for world and action lookups as needed.</param>
    /// <returns>A result containing the action, item name, and any matching items.</returns>
    public ParseResult ParseUserInput(
        string inputText,
        GameController controller)
    {
        Debug.Log($"[Parser] Received input: {inputText}");

        // Enrich the input text with basic rules
        string playerCommand = EnrichInputText(inputText);

        // Separate the input text into words
        List<string> separatedInputWords = playerCommand.Trim().ToLower()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        Debug.Log($"[Parser] Separated input words: {string.Join(", ", separatedInputWords)}");

        // Get the input action (using the controller's actions manager).
        ActionsManager actionsManager = controller.actionsManager;
        (Action action, List<string> separatedInputWordsNoKeyword) = GetInputAction(
            actionsManager, separatedInputWords);

        // Get the item name and matching items (using the game controller for world lookups).
        string itemName = "";
        var itemsMatchingName = new List<BaseObject>();
        if (separatedInputWordsNoKeyword != null && separatedInputWordsNoKeyword.Count > 0)
        {
            (itemName, itemsMatchingName) = GetItem(
                separatedInputWordsNoKeyword,
                controller
            );
        }

        return new ParseResult
        {
            Action = action,
            ItemsMatchingName = itemsMatchingName,
            ItemName = itemName
        };
    }

    /* Enriches the input text with basic rules. */
    private static string EnrichInputText(string inputText)
    {
        // Strip trailing and leading spaces
        inputText = inputText.Trim();

        // If the input is a direction, add the 'go' keyword to the input
        if (ALL_POSSIBLE_DIRECTIONS.Contains(inputText.ToLower()))
        {
            inputText = $"go {inputText}";
        }

        return inputText;
    }

    /* Looks for a single item in the given list of words, and 
    returns the item name and the matching items. The parser "speaks
    for the player", but it consults the controller (who speaks for the game)
    to find which world objects match the given name. */
    private (string itemName, List<BaseObject> matchingItems) GetItem(
        List<string> separatedInputWordsNoKeyword,
        GameController controller)
    {
        for (int idx = separatedInputWordsNoKeyword.Count; idx > 0; idx--)
        {
            string itemName = string.Join(" ", separatedInputWordsNoKeyword.Take(idx));
            Debug.Log($"[Parser] Checking item name: {itemName}");

            // Ask the controller's objects manager which items match this name in the current world.
            List<BaseObject> matchingItems = controller.objectsManager.GetAllItemsMatchingName(itemName);

            if (matchingItems != null && matchingItems.Count > 0)
            {
                string itemNames = string.Join(", ", matchingItems.Select(item => item.name));
                Debug.Log($"[Parser] Found {matchingItems.Count} items matching name: {itemNames}");
                return (itemName, matchingItems);
            }
        }

        Debug.Log($"[Parser] No items found in <{string.Join(" ", separatedInputWordsNoKeyword)}>.");
        // Default item name is the joined input words
        return (string.Join(" ", separatedInputWordsNoKeyword), new List<BaseObject>());
    }

    /* Returns the action and the remaining words without the keyword.
    Assumes that the first entry in the input is the action keyword. */
    private (Action action, List<string> remainingWords) GetInputAction(
        ActionsManager actionsManager,
        List<string> separatedInputWords)
    {
        // Try 3 words, then 2, then 1
        for (int i = 3; i > 0; i--)
        {
            if (separatedInputWords.Count >= i)
            {
                string keyword = string.Join(" ", separatedInputWords.Take(i));
                Action action = actionsManager.GetAction(keyword);

                if (action != null)
                {
                    Debug.Log($"[Parser] Found action matching keyword <{keyword}>");
                    var remainingWords = separatedInputWords.Skip(i).ToList();
                    return (action, remainingWords);
                }
            }
        }

        // If we get here, no keyword was found
        Debug.Log($"[Parser] No keyword found in command: {string.Join(" ", separatedInputWords)}");
        return (null, separatedInputWords);
    }
}

/// <summary>
/// Result of parsing user input.
/// The parser "speaks for the player", so it produces a high-level intent:
/// which action keyword was used, what item name the player said, and
/// which world objects currently match that name (as seen via the controller).
/// </summary>
public class ParseResult
{
    public Action Action { get; set; }
    public List<BaseObject> ItemsMatchingName { get; set; } = new List<BaseObject>();
    public string ItemName { get; set; } = "";
}
