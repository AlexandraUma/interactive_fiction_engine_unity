using NUnit.Framework;
using System.Collections.Generic;
using System;

public class ObjectsManagerHelperTests
{
    private BaseObject testItem;

    [SetUp]
    public void SetUp()
    {
        testItem = TestObjectFactory.CreateBaseObject("TestItem", "test item", new List<string> { "item", "thing" });
    }

    [Test]
    public void GetPrimaryName_ReturnsMainName()
    {
        string name = ObjectsManagerHelper.GetPrimaryName(testItem);
        Assert.AreEqual("test item", name);
    }

    [Test]
    public void GetPrimaryName_ReturnsAssetNameWhenMainNameIsEmpty()
    {
        var item = TestObjectFactory.CreateBaseObject("AssetName", "");
        string name = ObjectsManagerHelper.GetPrimaryName(item);
        Assert.AreEqual("AssetName", name);
    }

    [Test]
    public void GetPrimaryName_ReturnsEmptyStringForNullItem()
    {
        string name = ObjectsManagerHelper.GetPrimaryName(null);
        Assert.AreEqual(string.Empty, name);
    }

    [Test]
    public void GetAllNames_ReturnsPrimaryNameAndAliases()
    {
        var names = ObjectsManagerHelper.GetAllNames(testItem);
        Assert.Contains("test item", names);
        Assert.Contains("item", names);
        Assert.Contains("thing", names);
    }

    [Test]
    public void GetAllNames_HandlesNullAliases()
    {
        var item = TestObjectFactory.CreateBaseObject("Item", "item", null);
        var names = ObjectsManagerHelper.GetAllNames(item);
        Assert.Contains("item", names);
    }

    [Test]
    public void GetAllNames_FiltersEmptyAliases()
    {
        var item = TestObjectFactory.CreateBaseObject("Item", "item", new List<string> { "", "  ", "valid" });
        var names = ObjectsManagerHelper.GetAllNames(item);
        Assert.Contains("item", names);
        Assert.Contains("valid", names);
        Assert.IsFalse(names.Contains(""));
        Assert.IsFalse(names.Contains("  "));
    }

    [Test]
    public void AddItemToCollection_AddsItemByPrimaryName()
    {
        var collection = new Dictionary<string, List<BaseObject>>();
        ObjectsManagerHelper.AddItemToCollection(testItem, collection);

        Assert.IsTrue(collection.ContainsKey("test item"));
        Assert.Contains(testItem, collection["test item"]);
    }

    [Test]
    public void AddItemToCollection_AddsItemByAliases()
    {
        var collection = new Dictionary<string, List<BaseObject>>();
        ObjectsManagerHelper.AddItemToCollection(testItem, collection);

        Assert.IsTrue(collection.ContainsKey("item"));
        Assert.IsTrue(collection.ContainsKey("thing"));
        Assert.Contains(testItem, collection["item"]);
        Assert.Contains(testItem, collection["thing"]);
    }

    [Test]
    public void AddItemToCollection_IsCaseInsensitive()
    {
        // Use case-insensitive dictionary like ObjectsManager does
        var collection = new Dictionary<string, List<BaseObject>>(StringComparer.OrdinalIgnoreCase);
        ObjectsManagerHelper.AddItemToCollection(testItem, collection);

        // Helper stores keys in lowercase, but dictionary is case-insensitive
        Assert.IsTrue(collection.ContainsKey("TEST ITEM"));
        Assert.Contains(testItem, collection["TEST ITEM"]);
    }

    [Test]
    public void AddItemToCollection_HandlesNullItem()
    {
        var collection = new Dictionary<string, List<BaseObject>>();
        Assert.DoesNotThrow(() => ObjectsManagerHelper.AddItemToCollection(null, collection));
    }

    [Test]
    public void AddItemToCollection_HandlesNullCollection()
    {
        Assert.DoesNotThrow(() => ObjectsManagerHelper.AddItemToCollection(testItem, null));
    }

    [Test]
    public void AddItemToCollection_DoesNotAddDuplicate()
    {
        var collection = new Dictionary<string, List<BaseObject>>();
        ObjectsManagerHelper.AddItemToCollection(testItem, collection);
        ObjectsManagerHelper.AddItemToCollection(testItem, collection);

        Assert.AreEqual(1, collection["test item"].Count);
    }

    [Test]
    public void RemoveItemFromCollection_RemovesItem()
    {
        var collection = new Dictionary<string, List<BaseObject>>();
        ObjectsManagerHelper.AddItemToCollection(testItem, collection);
        ObjectsManagerHelper.RemoveItemFromCollection(testItem, collection);

        Assert.IsFalse(collection.ContainsKey("test item"));
    }

    [Test]
    public void RemoveItemFromCollection_RemovesFromAllAliases()
    {
        var collection = new Dictionary<string, List<BaseObject>>();
        ObjectsManagerHelper.AddItemToCollection(testItem, collection);
        ObjectsManagerHelper.RemoveItemFromCollection(testItem, collection);

        Assert.IsFalse(collection.ContainsKey("item"));
        Assert.IsFalse(collection.ContainsKey("thing"));
    }

    [Test]
    public void RemoveItemFromCollection_RemovesEmptyLists()
    {
        var collection = new Dictionary<string, List<BaseObject>>();
        ObjectsManagerHelper.AddItemToCollection(testItem, collection);
        ObjectsManagerHelper.RemoveItemFromCollection(testItem, collection);

        Assert.AreEqual(0, collection.Count);
    }

    [Test]
    public void RemoveItemFromCollection_HandlesNullItem()
    {
        var collection = new Dictionary<string, List<BaseObject>>();
        Assert.DoesNotThrow(() => ObjectsManagerHelper.RemoveItemFromCollection(null, collection));
    }

    [Test]
    public void RemoveItemFromCollection_HandlesNullCollection()
    {
        Assert.DoesNotThrow(() => ObjectsManagerHelper.RemoveItemFromCollection(testItem, null));
    }

    [Test]
    public void UnpackItemsMapIntoCollection_AddsAllItems()
    {
        var item1 = TestObjectFactory.CreateBaseObject("Item1", "item one");
        var item2 = TestObjectFactory.CreateBaseObject("Item2", "item two");
        var itemsMap = new Dictionary<string, BaseObject>
        {
            { "item one", item1 },
            { "item two", item2 }
        };
        var collection = new Dictionary<string, List<BaseObject>>();

        ObjectsManagerHelper.UnpackItemsMapIntoCollection(itemsMap, collection);

        Assert.Contains(item1, collection["item one"]);
        Assert.Contains(item2, collection["item two"]);
    }

    [Test]
    public void UnpackItemsMapIntoCollection_HandlesNullMap()
    {
        var collection = new Dictionary<string, List<BaseObject>>();
        Assert.DoesNotThrow(() => ObjectsManagerHelper.UnpackItemsMapIntoCollection(null, collection));
    }
}

