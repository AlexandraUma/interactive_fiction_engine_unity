using System.Collections.Generic;

/// <summary>
/// Handles the "inventory" action in the interactive fiction engine.
///
/// Mirrors the behaviour of the original Python Inventory action:
/// - Ignores any provided item.
/// - Lists all items currently carried by the player, or reports that the
///   player is not carrying anything.
/// </summary>
public class Inventory : Action
{
    /// <summary>
    /// The verb that triggers this action.
    /// </summary>
    public override string Keyword => "inventory";

    /// <summary>
    /// Other names that should resolve to the same action.
    /// </summary>
    public override List<string> Aliases { get; } = new()
    {
        "i",
        "inv"
    };

    /// <summary>
    /// Inventory only reports state; it does not change the world.
    /// </summary>
    public override bool CanAffectWorld => false;

    /// <summary>
    /// Inventory does not conceptually require an item.
    /// Any item provided is ignored.
    /// </summary>
    public override ItemApplicabilityLevel ItemApplicabilityLevel =>
        ItemApplicabilityLevel.NA;

    /// <summary>
    /// Inventory can conceptually be applied regardless of item; the item is ignored.
    /// </summary>
    public override bool CanApplyToItem(BaseObject item) => true;

    /// <summary>
    /// Handle the player's request to view their inventory.
    /// </summary>
    public override ActionStatus Execute(GameController controller, BaseObject item)
    {
        // Get the items in the player's inventory (ignoring the provided item, if any).
        List<BaseObject> itemsInInventory = controller.objectsManager.GetItemsCarriedByPlayer();

        string message;
        if (itemsInInventory == null || itemsInInventory.Count == 0)
        {
            message = "You are not carrying anything.";
        }
        else
        {
            // Compute the inventory string
            message = ComputeInventoryString(itemsInInventory);
        }

        // Log the inventory and return a successful status.
        return ActionHelper.LogActionAndReturnStatus(
            gameController: controller,
            message: message
        );
    }

    /// <summary>
    /// Compute the string representation of the player's inventory.
    /// </summary>
    private static string ComputeInventoryString(List<BaseObject> itemsInInventory)
    {
        string inventoryString = "You are carrying:\n";

        foreach (BaseObject item in itemsInInventory)
        {
            if (item == null)
            {
                continue;
            }

            // Prefer the object's mainName, falling back to the Unity asset name.
            string itemName = !string.IsNullOrEmpty(item.mainName)
                ? item.mainName
                : item.name;

            inventoryString += $"\t - {itemName}\n";
        }

        return inventoryString;
    }
}