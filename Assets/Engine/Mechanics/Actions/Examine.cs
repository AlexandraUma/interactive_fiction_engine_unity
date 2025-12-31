using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the "examine" action in the interactive fiction engine.
///
/// - With an item: logs the item's examine text response or a default description.
/// - Without an item: redirects to the "look" action.
/// </summary>
[CreateAssetMenu(fileName = "Examine", menuName = "IFEngine/Actions/CoreActions/Examine")]
public class Examine : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "examine";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "x",
        "look at",
        "inspect"
    };

    /// <summary>
    /// Examine does not change world state; it only describes.
    /// </summary>
    public override bool CanAffectWorld => false;

    /// <summary>
    /// Examine can be performed with or without an item. If no item is supplied,
    /// we redirect to the "look" action.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.OPTIONAL;

    /// <summary>
    /// Examine is valid for any (and no) item.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item) => true;

    /// <summary>
    /// Executes the examine action.
    ///
    /// - No item: redirect to "look".
    /// - Item provided: show the item's examine text or a default message.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // If no item is given, redirect to the 'look' action.
        if (item == null)
        {
            Action lookAction = controller.actionsManager.GetAction("look")
                ?? throw new InvalidOperationException("No 'look' action registered in this game.");

            return lookAction.Execute(controller, null);
        }

        // If the item defines a restriction for "examine", honour it and short‑circuit.
        string restrictionMessage = ActionHelper.GetRestrictionMessage(item, Keyword);
        if (restrictionMessage != null)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: restrictionMessage,
                status: ActionStatus.RESTRICTED
            );
        }

        // Otherwise, examine the item.
        return ExamineItem(controller, item);
    }

    /// <summary>
    /// Logs the item's examine response, description, or a default text.
    /// On first examination (before any world-affecting interaction), shows initialAppearance if set.
    /// </summary>
    private ActionStatus ExamineItem(GameController controller, BaseObject item)
    {
        string textResponse = ActionHelper.GetTextResponse(item, Keyword);

        if (textResponse == null)
        {
            // Show initialAppearance only if the item hasn't been interacted with yet
            if (!item.hasBeenInteractedWith && !string.IsNullOrWhiteSpace(item.initialAppearance))
            {
                textResponse = item.initialAppearance;
            }
            else
            {
                textResponse = $"You don't see anything special about the {item.mainName}.";
            }
        }

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse
        );
    }
}


