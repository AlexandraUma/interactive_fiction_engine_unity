using System.Collections.Generic;


/// <summary>
/// Handles the "attack" action (and its aliases) in the interactive fiction engine.
/// Mirrors the behaviour of the original Python Attack action.
/// </summary>
public class Attack : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "attack";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "hit", "strike", "bang", "rap", "knock", "knock on", "punch"
    };

    /// <summary>
    /// Attack can affect the world (and so must return an ActionStatus).
    /// </summary>
    public override bool CanAffectWorld => true;

    /// <summary>
    /// Attack can be performed with or without an item.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.OPTIONAL;

    /// <summary>
    /// Attack is valid for any (and no) item.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item) => true;

    /// <summary>
    /// Executes the attack action.
    /// 
    /// Possible outcomes:
    /// - No item to attack: default text response.
    /// - Item to attack: item's specific attack response or default text response.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // No item specified: generic response
        if (item == null)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: "Violence isn't the answer to this one."
            );
        }

        // If the item defines a restriction for "attack", honour it and short‑circuit.
        string restrictionMessage = ActionHelper.GetRestrictionMessage(item, Keyword);
        if (restrictionMessage != null)
        {
            return ActionHelper.LogActionAndReturnStatus(
                gameController: controller,
                message: restrictionMessage,
                status: ActionStatus.RESTRICTED
            );
        }

        return AttackItem(controller, item);
    }

    /// <summary>
    /// Logs the item's attack response or a default response.
    /// </summary>
    private ActionStatus AttackItem(GameController controller, BaseObject item)
    {
        string textResponse = ActionHelper.GetTextResponse(item, Keyword);
        textResponse ??= "Violence isn't the answer to this one.";

        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: textResponse
        );
    }
}