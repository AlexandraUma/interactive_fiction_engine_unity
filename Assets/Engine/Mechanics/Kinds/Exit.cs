using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An exit is a connection between locations.
///
/// It is a <see cref="BaseObject"/> that is fixed in place and conceptually
/// represents moving from one <see cref="Room"/> to another, optionally via a door.
/// </summary>
[CreateAssetMenu(fileName = "New Exit", menuName = "IFEngine/Kinds/Exit")]
public class Exit : BaseObject
{
    [Header("Exit Target")]
    [Tooltip("The room this exit leads to.")]
    public Room destinationRoom;

    [Tooltip("Optional door object associated with this exit.")]
    public BaseObject door;

    /// <summary>
    /// Called when the asset is first created or reset in the editor.
    /// Mirrors the Python Exit defaults where possible.
    /// </summary>
    private void Reset()
    {
        // Ensure helpful default aliases.
        aliases ??= new List<string>();

        if (!aliases.Contains("exit"))
        {
            aliases.Add("exit");
        }

        // Attach default exit traits if they are not present yet.
        if (!HasProperty<FixedInPlace>())
        {
            AddProperty(new FixedInPlace(isFixedInPlace: true));
        }
    }
}


