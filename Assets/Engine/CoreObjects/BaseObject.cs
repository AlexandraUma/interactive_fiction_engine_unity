using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TextResponse
{
    public string actionKeyword;
    public string response;
}

[System.Serializable]
public class ActionResponseEntry
{
    public string actionKeyword;
    public List<ActionResponseLogic> responses;
}

[System.Serializable]
public class ActionOverrideEntry
{
    public string actionKeyword;
    public Action anActionThatOverridesTheDefaultAction;
}

[System.Serializable]
public class ActionRestriction
{
    public string actionKeyword;

    [TextArea(3, 10)]
    public string message;
}


[CreateAssetMenu(fileName = "New BaseObject", menuName = "IFEngine/CoreObjects/BaseObject")]
public partial class BaseObject : ScriptableObject
{
    public const string SILENCE = "silence";
    public const string NOTHING = "nothing";

    /***************** Basic Information *****************/
    [Header("Base Object Properties")]
    /* The unique moniker for the object. */
    public string mainName;

    /* Aliases are used to identify the object in the game.*/
    public List<string> aliases = new();

    [TextArea(3, 10)]
    /* The appearance of the objective before it's interacted with. */
    public string initialAppearance;

    /***************** Text Responses *****************/
    [Header("Text Responses")]
    /* Text responses are basically a narration of the corresponding action.*/
    public List<TextResponse> textResponses = new();

    /***************** Action Restrictions *****************/
    [Header("Action Restrictions")]
    /* Optional per-action restriction messages that prevent the action. */
    public List<ActionRestriction> actionRestrictions = new();

    /***************** Action Responses *****************/
    [Header("Action Responses")]
    /* Action responses are after-effects of the corresponding action.*/
    public List<ActionResponseEntry> actionResponses = new();

    /***************** Action Overrides *****************/
    [Header("Action Overrides")]
    /* Action overrides are used to override the default action for the object.*/
    public List<ActionOverrideEntry> actionOverrides = new();

    /***************** Sensory Information *****************/
    [Header("Sensory Information")]
    /* Sensory information is used to describe the object's sensory properties.*/
    public bool isVisible = true;
    public string scent = NOTHING;
    public string taste = NOTHING;
    public string sound = SILENCE;

    /***************** Dynamic Properties *****************/
    [Header("Dynamic Properties")]
    /* 
     * A collection of reusable traits attached to this object.
     * 
     * These mirror the Python BaseObjectProperty hierarchy. They are authored
     * in the inspector (for designers) but also manipulated at runtime by code.
     */
    [SerializeReference]
    public List<BaseObjectProperty> properties = new();

    // Runtime lookup by id for fast queries. Rebuilt from the serialized list.
    [System.NonSerialized]
    private readonly Dictionary<string, BaseObjectProperty> _propertiesById = new();

    protected virtual void OnEnable()
    {
        RebuildPropertiesLookups();
        RebuildActionLookups();
    }

    protected virtual void OnValidate()
    {
        RebuildPropertiesLookups();
        RebuildActionLookups();
    }
}