using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A supporter is a fixed-in-place surface in a location that can hold other
/// objects on it (e.g. tables, pedestals, altars).
/// Mirrors the Python Supporter(BaseObject, FixedInPlace, HoldsContents) behaviour.
/// </summary>
[CreateAssetMenu(fileName = "New Supporter", menuName = "IFEngine/Kinds/Supporter")]
public class Supporter : BaseObject
{
    /// <summary>
    /// Called when the asset is first created or reset in the editor.
    /// Sets up sensible defaults (aliases and traits).
    /// </summary>
    private void Reset()
    {
        // Ensure helpful default aliases.
        aliases ??= new List<string>();

        if (!aliases.Contains("supporter"))
        {
            aliases.Add("supporter");
        }

        // Supporters are fixed in place and can hold contents by default.
        if (!HasProperty<FixedInPlace>())
        {
            AddProperty(new FixedInPlace(isFixedInPlace: true));
        }

        if (!HasProperty<HoldsContents>())
        {
            AddProperty(new HoldsContents());
        }
    }
}