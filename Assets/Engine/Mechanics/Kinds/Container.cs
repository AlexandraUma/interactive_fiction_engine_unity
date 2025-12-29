using UnityEngine;

/// <summary>
/// A general-purpose in-world object that can hold other objects.
/// <see cref="HoldsContents"/> property to track its contents.
/// </summary>
[CreateAssetMenu(fileName = "New Container", menuName = "IFEngine/Kinds/Container")]
public class Container : BaseObject
{
    /// <summary>
    /// Called when the asset is first created or reset in the editor.
    /// </summary>
    private void Reset()
    {
        if (!aliases.Contains("container"))
        {
            aliases.Add("container");
        }

        // Attach a HoldsContents trait if not already present, so this object
        // can actually contain other objects.
        if (!HasProperty<HoldsContents>())
        {
            AddProperty(new HoldsContents());
        }
    }
}
