using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scenery is a fixed-in-place object in a location.
/// Scenery items are meant to be descriptive background; they typically
/// don't have complex action responses.
/// </summary>
[CreateAssetMenu(fileName = "New Scenery", menuName = "IFEngine/Kinds/Scenery")]
public class Scenery : BaseObject
{
    /// <summary>
    /// Called when the asset is first created or reset in the editor.
    /// Mirrors the Python Scenery(BaseObject, FixedInPlace) behaviour.
    /// </summary>
    private void Reset()
    {
        // Ensure helpful default aliases.
        aliases ??= new List<string>();

        if (!aliases.Contains("scenery"))
        {
            aliases.Add("scenery");
        }

        // Scenery is fixed in place by default.
        if (!HasProperty<FixedInPlace>())
        {
            AddProperty(new FixedInPlace(isFixedInPlace: true));
        }
    }
}
