using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All game spaces are rooms. A room is a <see cref="BaseObject"/> that
/// is conceptually fixed in place, can hold contents, and can be lit or unlit.
/// </summary>
[CreateAssetMenu(fileName = "New Room", menuName = "IFEngine/Kinds/Room")]
public class Room : BaseObject
{
    // Runtime-only: resets when exiting Play Mode. Tracks how many times the room has been visited.
    [System.NonSerialized]
    public int numVisits = 0;

    [Header("Exits")]
    [Tooltip("All exits that leave this room.")]
    public List<Exit> exits = new();

    /// <summary>
    /// Convenience access to a <see cref="Lightable"/> trait if present.
    /// Rooms are considered lit by default.
    /// </summary>
    public bool IsLit
    {
        get
        {
            Lightable lightable = GetProperty<Lightable>();
            return lightable?.IsLit ?? true;
        }
        set
        {
            Lightable lightable = GetProperty<Lightable>();
            lightable.IsLit = value;
        }
    }

    /// <summary>
    /// Called when the asset is first created or reset in the editor.
    /// </summary>
    private void Reset()
    {
        // Ensure helpful default aliases.
        aliases ??= new List<string>();

        if (!aliases.Contains("room"))
        {
            aliases.Add("room");
        }

        if (!aliases.Contains("location"))
        {
            aliases.Add("location");
        }

        // Default appearance if none has been authored.
        if (string.IsNullOrWhiteSpace(initialAppearance) && !string.IsNullOrWhiteSpace(mainName))
        {
            initialAppearance = $"You are in the {mainName}.";
        }

        // Attach default room traits if they are not present yet.
        if (!HasProperty<FixedInPlace>())
        {
            AddProperty(new FixedInPlace(isFixedInPlace: true));
        }

        if (!HasProperty<HoldsContents>())
        {
            AddProperty(new HoldsContents());
        }

        if (!HasProperty<Lightable>())
        {
            AddProperty(new Lightable(isLit: true)); // rooms are lit by default
        }
    }
}