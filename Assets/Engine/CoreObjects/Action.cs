using System.Collections.Generic;
using UnityEngine;

// Defines the scope of items that an action can be applied to.
public enum ItemApplicabilityLevel
{
    // The action does not require any item
    NA = 0,

    // The action can be performed with or without an item
    OPTIONAL = 1,

    // The action requires a specific item
    REQUIRED = 2
}

// Defines the status of an action.
public enum ActionStatus
{
    // The user's command failed according to the rules of the game world. 
    // E.g. going through a locked door without the right key. Or unlocking an orange.
    FAILED = 0,

    // The user's command is redundant. E.g. trying to open an already open door
    INEFFECTIVE = 1,

    // The user's command was valid and not redundant. E.g., opening a door
    SUCCESSFUL = 2,

    // The user's command is conceptually valid but explicitly disallowed
    // by design-time restrictions on the world or objects.
    RESTRICTED = 3
}

// Abstract base class for all actions in the interactive fiction engine.
public abstract class Action: ScriptableObject
{
    // The verb that triggers the Action.
    public abstract string Keyword { get; }

    // Other names for the Action.
    public abstract List<string> Aliases { get; }

    /* Return true if the action changes the state of the world, and false otherwise.
    
    This is used to compile the game. Any actions that affect the
    world must return ActionStatus, any that do not must return null.
    */
    public abstract bool CanAffectWorld { get; }

    // Returns one applicability level.
    public abstract ItemApplicabilityLevel ItemApplicabilityLevel { get; }

    /* Returns true if the action can be applied to the given item, false otherwise.
    
    This function is called by actions with an item_applicability_level of REQUIRED, 
    and thus must be implemented by those actions. Other actions must return true for all items.
    */
    public abstract bool CanApplyToItem(BaseObject item);

    /* This function implements the action. Returns ActionStatus if the 
    action affects the world, otherwise null.
    */
    public abstract ActionStatus Execute(GameController controller, BaseObject item);
}