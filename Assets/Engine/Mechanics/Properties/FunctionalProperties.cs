using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// An item inheriting this property can have text written on it.
/// Mirrors the Python Writable(FunctionalProperty) behaviour.
/// </summary>
[Serializable]
public class Writable : FunctionalProperty
{
    /// <summary>
    /// The current text written on this object.
    /// </summary>
    public string Writing { get; set; }

    public Writable(string writing = "")
    {
        Description = "Text can be written on this object.";
        Writing = writing ?? string.Empty;
    }
}

/// <summary>
/// An item inheriting this property can have other items inside or on it.
/// Mirrors the Python HoldsContents(FunctionalProperty) behaviour.
/// </summary>
[Serializable]
public class HoldsContents : FunctionalProperty, ISerializationCallbackReceiver
{
    /// <summary>
    /// Authorable list of starting contents for this object.
    /// Designers edit this list in the Inspector.
    /// </summary>
    [SerializeField]
    [Tooltip("Starting contents for this object. At runtime, a name-based map is built from this list.")]
    private List<BaseObject> initialContents = new();

    /// <summary>
    /// Runtime map from a primary name to the contained object.
    /// This is built from <see cref="initialContents"/> after deserialization.
    /// </summary>
    [NonSerialized]
    private Dictionary<string, BaseObject> _contents = new();

    /// <summary>
    /// The contents held by this object, keyed by an identifier (e.g. object id or name).
    /// </summary>
    public Dictionary<string, BaseObject> Contents => _contents;

    public HoldsContents(Dictionary<string, BaseObject> contents = null)
    {
        Description = "This object can contain other objects.";

        if (contents != null)
        {
            _contents = contents;
            initialContents = new List<BaseObject>(contents.Values);
        }
        else
        {
            _contents = new Dictionary<string, BaseObject>();
            initialContents = new List<BaseObject>();
        }
    }

    /// <summary>
    /// Unity calls this before serializing the parent object.
    /// We don't need to push runtime changes back into the asset here,
    /// so this is intentionally left empty.
    /// </summary>
    public void OnBeforeSerialize()
    {
        // Intentionally no-op: the Inspector edits initialContents directly.
    }

    /// <summary>
    /// After Unity deserializes the asset, rebuild the runtime map
    /// from the inspector-authored list.
    /// </summary>
    public void OnAfterDeserialize()
    {
        _contents = new Dictionary<string, BaseObject>();

        if (initialContents == null)
        {
            return;
        }

        foreach (BaseObject item in initialContents)
        {
            if (item == null)
            {
                continue;
            }

            string key = ObjectsManagerHelper.GetPrimaryName(item);
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (!_contents.ContainsKey(key))
            {
                _contents.Add(key, item);
            }
        }
    }
}

/// <summary>
/// An item inheriting this property can have health points and an "alive" state.
/// Mirrors the Python Aliveness(FunctionalProperty) behaviour (without the bug in is_alive).
/// </summary>
[System.Serializable]
public class Aliveness : FunctionalProperty
{
    [SerializeField] // This makes it show up in the Inspector
    private int _health = 100;

    public int Health
    {
        get => _health;
        set => _health = value;
    }

    public bool IsAlive => _health > 0;

    public Aliveness(int health = 100)
    {
        Description = "This object has health and can be alive if health is greater than 0 or dead otherwise.";
        _health = health;
    }
}
