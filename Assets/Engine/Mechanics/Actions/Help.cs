using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles the "help" action in the interactive fiction engine.
///
/// Displays basic hints and a list of available commands to help the player
/// understand how to interact with the game.
/// </summary>
[UnityEngine.CreateAssetMenu(fileName = "Help", menuName = "IFEngine/Actions/CoreActions/Help")]
public class Help : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "help";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "?",
        "hint",
        "hints",
        "commands"
    };

    /// <summary>
    /// Help only displays information; it does not change the world.
    /// </summary>
    public override bool CanAffectWorld => false;

    /// <summary>
    /// Help does not require an item. Any item provided is ignored.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.NA;

    /// <summary>
    /// Help can be invoked regardless of item context.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item) => true;

    /// <summary>
    /// Display help information to the player.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        string message = BuildHelpMessage(controller);

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: message
        );
    }

    /// <summary>
    /// Build the help message with hints and available commands.
    /// </summary>
    private static string BuildHelpMessage(GameController controller)
    {
        string help = "<b>How to Play</b>\n";
        help += "Type commands to interact with the world. For example:\n";
        help += "  • <i>look</i> — describe your surroundings\n";
        help += "  • <i>examine [thing]</i> — look closely at something\n";
        help += "  • <i>take [thing]</i> — pick something up\n";
        help += "  • <i>go [direction]</i> — move to another location\n";
        help += "  • <i>inventory</i> — see what you're carrying\n";
        help += "\n";

        // List all available actions
        help += "<b>Available Commands</b>\n";
        
        List<Action> actions = controller.actionsManager.GetAllActions();
        if (actions != null && actions.Count > 0)
        {
            // Sort actions alphabetically by keyword
            var sortedActions = actions.OrderBy(a => a.Keyword).ToList();
            
            foreach (Action action in sortedActions)
            {
                string aliases = action.Aliases != null && action.Aliases.Count > 0
                    ? $" (also: {string.Join(", ", action.Aliases)})"
                    : "";
                help += $"  • <i>{action.Keyword}</i>{aliases}\n";
            }
        }

        help += "\n";
        help += "<b>Tips</b>\n";
        help += "  • Try different verbs if something doesn't work.\n";
        help += "  • Examine things for clues.\n";
        help += "  • Type <i>quit</i> to exit the game.";

        return help;
    }
}

