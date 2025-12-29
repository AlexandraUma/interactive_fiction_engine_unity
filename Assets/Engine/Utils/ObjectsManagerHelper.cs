using System.Collections.Generic;

/// <summary>
/// Utility helpers for working with collections of <see cref="BaseObject"/>s.
/// Encapsulates common logic for name/alias handling and indexing objects
/// into lookup dictionaries.
/// </summary>
public static class ObjectsManagerHelper
{
    /// <summary>
    /// Returns the primary name used for indexing an object: its mainName if set,
    /// otherwise the Unity asset name.
    /// </summary>
    public static string GetPrimaryName(BaseObject item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(item.mainName))
        {
            return item.mainName;
        }

        return item.name ?? string.Empty;
    }

    /// <summary>
    /// Returns all names by which an object can be referred to: its primary name plus aliases.
    /// </summary>
    public static List<string> GetAllNames(BaseObject item)
    {
        var names = new List<string>();

        string primary = GetPrimaryName(item);
        if (!string.IsNullOrWhiteSpace(primary))
        {
            names.Add(primary);
        }

        if (item != null && item.aliases != null)
        {
            foreach (string alias in item.aliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                {
                    names.Add(alias);
                }
            }
        }

        return names;
    }

    /// <summary>
    /// Helper: Add item to a given collection by name and aliases.
    /// </summary>
    public static void AddItemToCollection(
        BaseObject item,
        Dictionary<string, List<BaseObject>> collection)
    {
        if (item == null || collection == null)
        {
            return;
        }

        List<string> names = GetAllNames(item);
        foreach (string rawName in names)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                continue;
            }

            string key = rawName.ToLowerInvariant();
            if (!collection.TryGetValue(key, out List<BaseObject> itemsWithName))
            {
                itemsWithName = new List<BaseObject>();
                collection[key] = itemsWithName;
            }

            if (!itemsWithName.Contains(item))
            {
                itemsWithName.Add(item);
            }
        }
    }

    /// <summary>
    /// Helper: Remove an item cleanly from a collection.
    /// An item might have many aliases, so we remove it from all collections.
    /// </summary>
    public static void RemoveItemFromCollection(
        BaseObject item,
        Dictionary<string, List<BaseObject>> collection)
    {
        if (item == null || collection == null || collection.Count == 0)
        {
            return;
        }

        List<string> names = GetAllNames(item);
        foreach (string rawName in names)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                continue;
            }

            string key = rawName.ToLowerInvariant();
            if (!collection.TryGetValue(key, out List<BaseObject> itemsWithName))
            {
                continue;
            }

            itemsWithName.Remove(item);
            if (itemsWithName.Count == 0)
            {
                collection.Remove(key);
            }
        }
    }

    /// <summary>
    /// Helper: Add BaseObject, indexed by their names into a collection.
    /// A collection is a mapping of item names (including aliases) to the
    /// list of all objects with that name.
    /// </summary>
    public static void UnpackItemsMapIntoCollection(
        Dictionary<string, BaseObject> itemsMap,
        Dictionary<string, List<BaseObject>> collection)
    {
        if (itemsMap == null)
        {
            return;
        }

        foreach (BaseObject item in itemsMap.Values)
        {
            AddItemToCollection(item, collection);
        }
    }
}
