using System;
using System.Collections.Generic;
using System.Linq;

public partial class BaseObject
{
    // Defined in BaseObject.ActionLookups.cs; used here so that both the
    // dynamic properties lookup and the action-related lookups are rebuilt
    // together during initialisation/validation.
    partial void RebuildActionLookups();
    /// <summary>
    /// Attach or replace a property on this object.
    /// </summary>
    public void AddProperty(BaseObjectProperty property)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        EnsurePropertyLookup();

        properties ??= new List<BaseObjectProperty>();

        if (_propertiesById.ContainsKey(property.Id))
        {
            throw new ArgumentException(
                $"BaseObject '{name}' already has a property with Id '{property.Id}'. " +
                "Properties should only be attached once during authoring.",
                nameof(property));
        }

        properties.Add(property);
        _propertiesById[property.Id] = property;

#if UNITY_EDITOR
        // This tells Unity: "The data in this ScriptableObject has changed, 
        // please save it to the disk and refresh the Inspector UI."
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /// <summary>
    /// Remove a property by its identifier.
    /// </summary>
    public bool RemoveProperty(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        EnsurePropertyLookup();

        bool removedFromDict = _propertiesById.Remove(id);

        properties?.RemoveAll(p => p != null && p.Id == id);

        return removedFromDict;
    }

    /// <summary>
    /// Returns true if this object has a property with the given identifier.
    /// </summary>
    public bool HasProperty(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        EnsurePropertyLookup();
        return _propertiesById.ContainsKey(id);
    }

    /// <summary>
    /// Returns true if this object has a property of the given type.
    /// </summary>
    public bool HasProperty<TProperty>() where TProperty : BaseObjectProperty
    {
        EnsurePropertyLookup();
        return _propertiesById.Values.OfType<TProperty>().Any();
    }

    /// <summary>
    /// Gets a property by its identifier, or null if it does not exist.
    /// </summary>
    public BaseObjectProperty GetProperty(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        EnsurePropertyLookup();

        _propertiesById.TryGetValue(id, out BaseObjectProperty prop);
        return prop;
    }

    /// <summary>
    /// Gets the first property of the given type, or null if none is present.
    /// </summary>
    public TProperty GetProperty<TProperty>() where TProperty : BaseObjectProperty
    {
        EnsurePropertyLookup();
        return _propertiesById.Values.OfType<TProperty>().FirstOrDefault();
    }

    private void EnsurePropertyLookup()
    {
        if (_propertiesById.Count == 0)
        {
            RebuildPropertiesLookups();
        }
    }

    public void RebuildPropertiesLookups()
    {
        // Rebuild properties lookup
        properties ??= new List<BaseObjectProperty>();

        _propertiesById.Clear();

        foreach (BaseObjectProperty prop in properties)
        {
            if (prop == null)
            {
                continue;
            }

            _propertiesById[prop.Id] = prop;
        }
    }
}
