using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Door object that represents a door in the game.
///
/// In this engine, all doors are lockable, openable, and fixed in place.
/// </summary>
[CreateAssetMenu(fileName = "New Door", menuName = "IFEngine/Kinds/Door")]
public class Door : BaseObject
{
    /// <summary>
    /// Convenience wrapper around the <see cref="Openable"/> trait.
    /// </summary>
    public bool IsOpen
    {
        get
        {
            Openable openable = GetProperty<Openable>();
            return openable?.IsOpen ?? false;
        }
        set
        {
            Openable openable = GetProperty<Openable>();
            openable.IsOpen = value; // NRE if misconfigured: no Openable attached
        }
    }

    /// <summary>
    /// Convenience wrapper around the <see cref="Lockable"/> trait.
    /// </summary>
    public bool IsLocked
    {
        get
        {
            Lockable lockable = GetProperty<Lockable>();
            return lockable?.IsLocked ?? false;
        }
        set
        {
            Lockable lockable = GetProperty<Lockable>();
            lockable.IsLocked = value;
        }
    }

    /// <summary>
    /// Called when the asset is first created or reset in the editor.
    /// Sets sensible defaults to mirror the Python constructor:
    /// - aliases always include "door"
    /// - FixedInPlace true by default
    /// - Openable and Lockable present, closed and unlocked by default
    /// </summary>
    private void Reset()
    {
        // Ensure helpful default aliases.
        if (aliases == null)
        {
            aliases = new List<string>();
        }

        if (!aliases.Contains("door"))
        {
            aliases.Add("door");
        }

        // Attach default door traits if they are not present yet.
        if (!HasProperty<FixedInPlace>())
        {
            AddProperty(new FixedInPlace(isFixedInPlace: true));
        }

        if (!HasProperty<Openable>())
        {
            AddProperty(new Openable(isOpen: false));
        }

        if (!HasProperty<Lockable>())
        {
            AddProperty(new Lockable(isLocked: false));
        }
    }
}
