using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gender identity for a creature, used to derive pronouns.
/// Mirrors the Python PRONOUNS mapping (male/female/non-binary/inanimate).
/// </summary>
public enum CreatureGender
{
    Male,
    Female,
    NonBinary,
    Inanimate
}

/// <summary>
    /// A living actor (player or non-player) in the world.
    ///
    /// A creature is a <see cref="BaseObject"/> that typically:
    /// - Can be alive or dead (via <see cref="Aliveness"/>).
    /// - Can hold items (via <see cref="HoldsContents"/>).
    /// - Has gender-based pronouns for text output.
/// </summary>
[CreateAssetMenu(fileName = "New Creature", menuName = "IFEngine/Kinds/Creature")]
public class Creature : BaseObject
{
    [Header("Creature Identity")]
    [Tooltip("True if this creature represents the player character.")]
    public bool isPlayer = false;

    [Tooltip("The gender identity used to derive pronouns (he/she/they/it).")]
    public CreatureGender gender = CreatureGender.NonBinary;

    /// <summary>
    /// Subject pronoun based on <see cref="gender"/>, e.g. "he", "she", "they", "it".
    /// </summary>
    public string SubjectPronoun => gender switch
    {
        CreatureGender.Male => "he",
        CreatureGender.Female => "she",
        CreatureGender.Inanimate => "it",
        _ => "they",
    };

    /// <summary>
    /// Object pronoun based on <see cref="gender"/>, e.g. "him", "her", "them", "it".
    /// </summary>
    public string ObjectPronoun => gender switch
    {
        CreatureGender.Male => "him",
        CreatureGender.Female => "her",
        CreatureGender.Inanimate => "it",
        _ => "them",
    };

    /// <summary>
    /// Possessive determiner based on <see cref="gender"/>, e.g. "his", "her", "their", "its".
    /// </summary>
    public string PossessivePronoun => gender switch
    {
        CreatureGender.Male => "his",
        CreatureGender.Female => "her",
        CreatureGender.Inanimate => "its",
        _ => "their",
    };

    /// <summary>
    /// Reflexive pronoun based on <see cref="gender"/>, e.g. "himself", "herself", "themselves", "itself".
    /// </summary>
    public string ReflexivePronoun => gender switch
    {
        CreatureGender.Male => "himself",
        CreatureGender.Female => "herself",
        CreatureGender.Inanimate => "itself",
        _ => "themselves",
    };

    /// <summary>
    /// Convenience access to an <see cref="Aliveness"/> trait if present.
    /// Defaults to 100 health if no trait has been attached yet.
    /// </summary>
    public int Health
    {
        get
        {
            Aliveness aliveness = GetProperty<Aliveness>();
            return aliveness?.Health ?? 100;
        }
        set
        {
            Aliveness aliveness = GetProperty<Aliveness>();
            aliveness.Health = value; // NRE if misconfigured: no Aliveness attached
        }
    }

    /// <summary>
    /// True if this creature is currently alive.
    /// If no <see cref="Aliveness"/> trait is present, a default of Health &gt; 0 is assumed.
    /// </summary>
    public bool IsAlive
    {
        get
        {
            Aliveness aliveness = GetProperty<Aliveness>();
            return aliveness?.IsAlive ?? Health > 0;
        }
    }
    protected override void OnEnable()
    {
        // 1. Run the "Auto-Initialize" logic
        bool wasChanged = InitializeDefaults();

        // 2. IMPORTANT: Call the base lookup rebuild
        base.OnEnable();

        // 3. If we added something, save it to the asset file
        if (wasChanged) { MarkDirty(); }
    }

    private void Reset()
    {
        // Reset is called when the asset is first created via the menu
        InitializeDefaults();
        RebuildPropertiesLookups();
        MarkDirty();
    }

    private bool InitializeDefaults()
    {
        bool changed = false;

        // Ensure list is ready
        if (aliases == null) { aliases = new List<string>(); changed = true; }

        // Add default aliases if missing
        if (!aliases.Contains("creature")) { aliases.Add("creature"); changed = true; }
        if (!aliases.Contains("person")) { aliases.Add("person"); changed = true; }

        // Add default Traits only if they don't exist in the serialized list
        if (!HasPropertyInList<Aliveness>())
        {
            properties.Add(new Aliveness(100));
            changed = true;
        }

        if (!HasPropertyInList<HoldsContents>())
        {
            properties.Add(new HoldsContents());
            changed = true;
        }

        return changed;
    }

    // Helper to check the List before the Dictionary lookup is even built
    private bool HasPropertyInList<T>() where T : BaseObjectProperty
    {
        return properties.Exists(p => p is T);
    }

    private void MarkDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        // Only try to save to disk if this is an actual asset file
        if (UnityEditor.AssetDatabase.Contains(this))
        {
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }
#endif
}

    // /// <summary>
    // /// Called when the scriptable object is loaded or created.
    // /// </summary>
    // protected override void OnEnable()
    // {
    //     InitializeDefaults();
    // }

    // /// <summary>
    // /// Called when the "Reset" button is pressed in the Inspector.
    // /// </summary>
    // protected override void OnValidate()
    // {
    //     InitializeDefaults();
    // }

    // /// <summary>
    // /// Centralized logic to ensure properties exist without overwriting 
    // /// existing data during serialization.
    // /// </summary>
    // private void InitializeDefaults()
    // {
    //     // 1. Ensure lists are never null
    //     aliases ??= new List<string>();

    //     // 2. Add default aliases if missing
    //     if (!aliases.Contains("creature")) aliases.Add("creature");
    //     if (!aliases.Contains("person")) aliases.Add("person");

    //     // 3. Attach default traits only if they aren't already there.
    //     // This check is CRUCIAL to avoid overwriting saved data when 
    //     // the object is re-loaded via OnEnable.
    //     if (!HasProperty<Aliveness>())
    //     {
    //         AddProperty(new Aliveness(health: 100));
    //     }

    //     if (!HasProperty<HoldsContents>())
    //     {
    //         AddProperty(new HoldsContents());
    //     }
    // }
}

