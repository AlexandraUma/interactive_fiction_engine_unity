using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The Objects manager tracks all objects in the game.
/// Basically, if you're looking for a game object, ask it.
/// 
/// This class, like other managers receives validated objects from 
/// the GameController.
///
/// It keeps fast name-based lookup collections for:
/// - All objects visible in the current room (including exits and nested contents).
/// - All objects carried by the player.
/// </summary>
public class ObjectsManager
{
    private readonly Dictionary<string, Room> _allRoomsByName;
    private readonly Dictionary<string, BaseObject> _nonPlayerCharactersByName;

    // Name -> all objects with that name in the current room (including exits, containers, etc.).
    private readonly Dictionary<string, List<BaseObject>> _roomObjectsCollections;

    // Name -> all objects with that name carried by the player.
    private readonly Dictionary<string, List<BaseObject>> _playerCarriedItemsCollections;

    private readonly Creature _playerCharacter;
    private Room _currentRoom;

    /// <summary>
    /// The room the player is currently in, exposed as a <see cref="BaseObject"/>
    /// because most actions treat rooms just like any other world object.
    /// </summary>
    public BaseObject CurrentRoom => _currentRoom;

    /// <summary>
    /// The player character as a generic <see cref="BaseObject"/>.
    /// </summary>
    public BaseObject PlayerCharacter => _playerCharacter;

    /// <summary>
    /// Initialise rooms, items and characters, and their collections.
    /// 
    /// Note that all these objects are expected to have been validated
    /// by the <see cref="GameController"/> before construction.
    /// If required data is missing or of the wrong type, this constructor
    /// will throw so that configuration bugs are caught early in testing.
    /// </summary>
    public ObjectsManager(
        BaseObject startingRoom,
        List<BaseObject> allRooms,
        BaseObject playerCharacter,
        List<BaseObject> nonPlayerCharacters)
    {
        if (startingRoom == null)
        {
            throw new ArgumentNullException(nameof(startingRoom));
        }
        if (allRooms == null)
        {
            throw new ArgumentNullException(nameof(allRooms));
        }
        if (playerCharacter == null)
        {
            throw new ArgumentNullException(nameof(playerCharacter));
        }
        if (nonPlayerCharacters == null)
        {
            throw new ArgumentNullException(nameof(nonPlayerCharacters));
        }

        Room startingRoomTyped = startingRoom as Room
            ?? throw new ArgumentException("startingRoom must be a Room asset.", nameof(startingRoom));

        _playerCharacter = playerCharacter as Creature
            ?? throw new ArgumentException("playerCharacter must be a Creature asset.", nameof(playerCharacter));

        // Index all rooms by their primary name for quick lookups if needed.
        _allRoomsByName = allRooms
            .OfType<Room>()
            .ToDictionary(
                room => ObjectsManagerHelper.GetPrimaryName(room),
                room => room,
                StringComparer.OrdinalIgnoreCase);

        // Index non-player characters by their primary name.
        _nonPlayerCharactersByName = new Dictionary<string, BaseObject>(StringComparer.OrdinalIgnoreCase);
        foreach (BaseObject npc in nonPlayerCharacters)
        {
            if (npc == null)
            {
                // Invalid entry in the list; let this surface during content validation.
                continue;
            }

            string key = ObjectsManagerHelper.GetPrimaryName(npc);
            if (!_nonPlayerCharactersByName.ContainsKey(key))
            {
                _nonPlayerCharactersByName.Add(key, npc);
            }
        }

        _roomObjectsCollections = new Dictionary<string, List<BaseObject>>(StringComparer.OrdinalIgnoreCase);
        _playerCarriedItemsCollections = new Dictionary<string, List<BaseObject>>(StringComparer.OrdinalIgnoreCase);

        // Unpack the player's starting inventory into the carried-items collection.
        HoldsContents playerContents = _playerCharacter.GetProperty<HoldsContents>();
        if (playerContents != null)
        {
            ObjectsManagerHelper.UnpackItemsMapIntoCollection(playerContents.Contents, _playerCarriedItemsCollections);
        }

        // Finally, set the current room and unpack its contents.
        SetCurrentRoom(startingRoomTyped);
    }

    /// <summary>
    /// Clears the collections for the current room.
    /// </summary>
    public void ClearCollectionsForNewRoom()
    {
        _roomObjectsCollections.Clear();
    }

    /// <summary>
    /// Set the current room as the new room and unpacks its collections.
    /// The player is added to the room collection (but not its contents)
    /// because their presence is temporary and used for interpreting
    /// commands like "examine me".
    /// </summary>
    public void SetCurrentRoom(Room newRoom)
    {
        ClearCollectionsForNewRoom();  // wipe old exits/objects
        _currentRoom = newRoom;

        // Mirror Python's num_visits on the room.
        _currentRoom.numVisits += 1;

        // Rebuild exits + items.
        UnpackRoomItems();

        // Add the player to the room temporarily.
        ObjectsManagerHelper.AddItemToCollection(_playerCharacter, _roomObjectsCollections);
    }

    /// <summary>
    /// Unpack items in the current room: its contents, exits, and contents
    /// of any in-room containers that can hold contents (recursively).
    /// </summary>
    private void UnpackRoomItems()
    {
        if (_currentRoom == null)
        {
            return;
        }

        // 1. Items directly in the room (via HoldsContents).
        HoldsContents roomContents = _currentRoom.GetProperty<HoldsContents>();
        if (roomContents != null)
        {
            // Items that are directly in the room.
            ObjectsManagerHelper.UnpackItemsMapIntoCollection(roomContents.Contents, _roomObjectsCollections);

            // 2. Recursively unpack items inside containers that also have HoldsContents.
            foreach (BaseObject item in roomContents.Contents.Values)
            {
                UnpackContainerContentsRecursive(item);
            }
        }

        // 3. Exits leaving the room.
        if (_currentRoom.exits != null)
        {
            foreach (Exit exit in _currentRoom.exits)
            {
                if (exit != null)
                {
                    ObjectsManagerHelper.AddItemToCollection(exit, _roomObjectsCollections);
                    
                    // 4. Doors attached to exits (so players can reference them directly).
                    if (exit.door != null)
                    {
                        ObjectsManagerHelper.AddItemToCollection(exit.door, _roomObjectsCollections);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recursively unpacks the contents of a container and any nested containers.
    /// </summary>
    private void UnpackContainerContentsRecursive(BaseObject container)
    {
        if (container == null)
        {
            return;
        }

        HoldsContents containerContents = container.GetProperty<HoldsContents>();
        if (containerContents == null)
        {
            return;
        }

        // Unpack items in this container.
        ObjectsManagerHelper.UnpackItemsMapIntoCollection(containerContents.Contents, _roomObjectsCollections);

        // Recursively unpack nested containers.
        foreach (BaseObject item in containerContents.Contents.Values)
        {
            UnpackContainerContentsRecursive(item);
        }
    }

    /// <summary>
    /// Put the item into the current room's contents,
    /// and also in the room's carried items collection.
    ///
    /// This method can be called when dropping an item.
    /// </summary>
    public void AddItemToRoom(BaseObject item)
    {
        HoldsContents roomContents = _currentRoom.GetProperty<HoldsContents>();
        string itemName = ObjectsManagerHelper.GetPrimaryName(item);
        roomContents.Contents[itemName] = item;

        ObjectsManagerHelper.AddItemToCollection(item, _roomObjectsCollections);
    }

    /// <summary>
    /// Remove an item from the current room.
    /// </summary>
    public void RemoveItemFromRoom(BaseObject item)
    {
        ObjectsManagerHelper.RemoveItemFromCollection(item, _roomObjectsCollections);

        HoldsContents roomContents = _currentRoom.GetProperty<HoldsContents>();

        // Remove any entries in the contents map that reference this item.
        var keysToRemove = new List<string>();
        foreach (KeyValuePair<string, BaseObject> kvp in roomContents.Contents)
        {
            if (kvp.Value == item)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (string key in keysToRemove)
        {
            roomContents.Contents.Remove(key);
        }
    }

    /// <summary>
    /// Put the item into the player's inventory by placing it in the player's contents,
    /// and in the player's carried items collection.
    /// </summary>
    public void AddItemToPlayer(BaseObject item)
    {

        HoldsContents playerContents = _playerCharacter.GetProperty<HoldsContents>();
        string key = ObjectsManagerHelper.GetPrimaryName(item);
        playerContents.Contents[key] = item;

        ObjectsManagerHelper.AddItemToCollection(item, _playerCarriedItemsCollections);
    }

    /// <summary>
    /// Remove the item from the player's contents and collection.
    /// </summary>
    public void RemoveItemFromPlayer(BaseObject item)
    {

        ObjectsManagerHelper.RemoveItemFromCollection(item, _playerCarriedItemsCollections);

        HoldsContents playerContents = _playerCharacter.GetProperty<HoldsContents>();
        var keysToRemove = new List<string>();
        foreach (KeyValuePair<string, BaseObject> kvp in playerContents.Contents)
        {
            if (kvp.Value == item)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (string key in keysToRemove)
        {
            playerContents.Contents.Remove(key);
        }
    }

    /// <summary>
    /// Check if the item is in the player's inventory.
    /// </summary>
    public bool IsItemCarriedByPlayer(BaseObject item)
    {

        HoldsContents playerContents = _playerCharacter.GetProperty<HoldsContents>();
        bool inInventory = playerContents.Contents.Values.Contains(item);
        bool inCollections = false;

        foreach (List<BaseObject> items in _playerCarriedItemsCollections.Values)
        {
            if (items.Contains(item))
            {
                inCollections = true;
                break;
            }
        }

        return inInventory && inCollections;
    }

    /// <summary>
    /// Get all items the player is carrying.
    /// </summary>
    public List<BaseObject> GetItemsCarriedByPlayer()
    {
        HoldsContents playerContents = _playerCharacter.GetProperty<HoldsContents>();
        return new List<BaseObject>(playerContents.Contents.Values);
    }

    /// <summary>
    /// Get all items by name from the room or inventory. If multiple items have the same name,
    /// return a list of all matching items.
    /// </summary>
    public List<BaseObject> GetAllItemsMatchingName(string itemName)
    {
        string key = itemName.ToLowerInvariant();
        var results = new List<BaseObject>();

        if (_roomObjectsCollections.TryGetValue(key, out List<BaseObject> roomItems))
        {
            results.AddRange(roomItems);
        }

        if (_playerCarriedItemsCollections.TryGetValue(key, out List<BaseObject> carriedItems))
        {
            results.AddRange(carriedItems);
        }

        return results;
    }

}
