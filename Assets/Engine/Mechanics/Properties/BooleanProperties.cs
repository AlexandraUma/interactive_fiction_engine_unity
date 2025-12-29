using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Any item with this property can be lit or unlit.
/// Mirrors the Python Lightable(BooleanProperty) behaviour.
/// </summary>
[Serializable]
public class Lightable : BooleanProperty
{
    /// <summary>
    /// True if the object is currently lit/illuminated.
    /// Convenience wrapper around the underlying <see cref="BooleanProperty.Value"/>.
    /// </summary>
    public bool IsLit
    {
        get => Value;
        set => Value = value;
    }

    public Lightable(bool isLit = true)
    {
        Description = "This object can be lit or extinguished.";
        Value = isLit;
    }
}

/// <summary>
/// Any item with this property can be locked and unlocked.
/// Mirrors the Python Lockable(BooleanProperty) behaviour.
/// </summary>
[Serializable]
public class Lockable : BooleanProperty
{
    /// <summary>
    /// True if the object is currently locked.
    /// Convenience wrapper around the underlying <see cref="BooleanProperty.Value"/>.
    /// </summary>
    public bool IsLocked
    {
        get => Value;
        set => Value = value;
    }

    public Lockable(bool isLocked = false)
    {
        Description = "This object can be locked or unlocked.";
        Value = isLocked;
    }
}

/// <summary>
/// Any item with this property can be opened and closed.
/// Mirrors the Python Openable(BooleanProperty) behaviour.
/// </summary>
[Serializable]
public class Openable : BooleanProperty
{
    /// <summary>
    /// True if the object is currently open.
    /// Convenience wrapper around the underlying <see cref="BooleanProperty.Value"/>.
    /// </summary>
    public bool IsOpen
    {
        get => Value;
        set => Value = value;
    }

    public Openable(bool isOpen = false)
    {
        Description = "This object can be opened or closed.";
        Value = isOpen;
    }
}

/// <summary>
/// Any item with this property cannot be moved from its current location.
/// Mirrors the Python FixedInPlace(BooleanProperty) behaviour.
/// </summary>
[Serializable]
public class FixedInPlace : BooleanProperty
{
    /// <summary>
    /// True if the object is fixed in place and cannot be moved.
    /// Convenience wrapper around the underlying <see cref="BooleanProperty.Value"/>.
    /// </summary>
    public bool IsFixedInPlace
    {
        get => Value;
        set => Value = value;
    }

    public FixedInPlace(bool isFixedInPlace = true)
    {
        Description = "This object is fixed in place and cannot be moved.";
        Value = isFixedInPlace;
    }
}


