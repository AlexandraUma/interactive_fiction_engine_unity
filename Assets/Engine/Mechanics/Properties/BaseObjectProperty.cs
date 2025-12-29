using System;
using UnityEngine;

/// <summary>
/// Base type for all reusable traits that can be attached to a <see cref="BaseObject"/>.
/// 
/// - BaseObjectProperty
/// - BooleanProperty
/// - FunctionalProperty
/// 
/// Concrete game-specific properties (e.g. FixedInPlace, Lockable, Writable)
/// should derive from one of these types.
/// </summary>
[Serializable]
public abstract class BaseObjectProperty
{
    [SerializeField]
    [HideInInspector]
    // [TextArea]
    [Tooltip("Optional human-readable description of what this property represents.")]
    protected string description;

    /// <summary>
    /// Unique identifier for this property.
    /// 
    /// Always defaults to the concrete type name and cannot be changed in the Inspector.
    /// This ensures consistent string-based and type-based lookup patterns.
    /// </summary>
    public string Id => GetType().Name;

    /// <summary>
    /// Optional human-readable description of the property.
    /// </summary>
    public string Description
    {
        get => description;
        set => description = value;
    }
}

/// <summary>
/// Represents a property with a boolean (true/false) value.
/// 
/// Examples:
/// - FixedInPlace: whether an object can be moved at all.
/// - Lockable: whether an object is currently locked.
/// </summary>
[Serializable]
public abstract class BooleanProperty : BaseObjectProperty
{
    [SerializeField]
    private bool value;

    /// <summary>
    /// The current boolean value of this property.
    /// </summary>
    public bool Value
    {
        get => value;
        set => this.value = value;
    }

    public void Enable() => value = true;

    public void Disable() => value = false;

    public void Toggle() => value = !value;
}

/// <summary>
/// Represents a property with richer internal state or behaviour than a simple boolean.
/// 
/// Subclasses are expected to define their own fields, for example:
/// - A Writable property with a string 'writing' field.
/// - An Aliveness property with health points and an 'isAlive' flag.
/// </summary>
[Serializable]
public abstract class FunctionalProperty : BaseObjectProperty
{
    // Intentionally empty: concrete functional properties define their own data/behaviour.
}


