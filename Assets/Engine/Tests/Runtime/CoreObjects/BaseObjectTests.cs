using NUnit.Framework;
using UnityEngine;
using System;

public class BaseObjectTests
{
    private BaseObject testObject;

    [SetUp]
    public void SetUp()
    {
        testObject = TestObjectFactory.CreateBaseObject("TestObject", "Test Object");
    }

    [Test]
    public void AddProperty_AddsPropertyToObject()
    {
        var property = new FixedInPlace();
        testObject.AddProperty(property);

        Assert.IsTrue(testObject.HasProperty<FixedInPlace>());
        Assert.AreEqual(property, testObject.GetProperty<FixedInPlace>());
    }

    [Test]
    public void AddProperty_ThrowsWhenPropertyIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => testObject.AddProperty(null));
    }

    [Test]
    public void AddProperty_ThrowsWhenDuplicateId()
    {
        var property1 = new FixedInPlace();
        var property2 = new FixedInPlace();
        
        testObject.AddProperty(property1);
        Assert.Throws<ArgumentException>(() => testObject.AddProperty(property2));
    }

    [Test]
    public void RemoveProperty_RemovesPropertyById()
    {
        var property = new FixedInPlace();
        testObject.AddProperty(property);

        bool removed = testObject.RemoveProperty(property.Id);
        
        Assert.IsTrue(removed);
        Assert.IsFalse(testObject.HasProperty<FixedInPlace>());
    }

    [Test]
    public void RemoveProperty_ReturnsFalseForNonExistentId()
    {
        bool removed = testObject.RemoveProperty("NonExistentId");
        Assert.IsFalse(removed);
    }

    [Test]
    public void RemoveProperty_ReturnsFalseForNullOrEmptyId()
    {
        Assert.IsFalse(testObject.RemoveProperty(null));
        Assert.IsFalse(testObject.RemoveProperty(""));
        Assert.IsFalse(testObject.RemoveProperty("   "));
    }

    [Test]
    public void HasProperty_ReturnsTrueWhenPropertyExists()
    {
        testObject.AddProperty(new FixedInPlace());
        Assert.IsTrue(testObject.HasProperty<FixedInPlace>());
    }

    [Test]
    public void HasProperty_ReturnsFalseWhenPropertyDoesNotExist()
    {
        Assert.IsFalse(testObject.HasProperty<FixedInPlace>());
    }

    [Test]
    public void HasProperty_ReturnsFalseForNullOrEmptyId()
    {
        Assert.IsFalse(testObject.HasProperty(null));
        Assert.IsFalse(testObject.HasProperty(""));
        Assert.IsFalse(testObject.HasProperty("   "));
    }

    [Test]
    public void GetProperty_ReturnsPropertyById()
    {
        var property = new FixedInPlace();
        testObject.AddProperty(property);

        var retrieved = testObject.GetProperty(property.Id);
        Assert.AreEqual(property, retrieved);
    }

    [Test]
    public void GetProperty_ReturnsNullForNonExistentId()
    {
        var retrieved = testObject.GetProperty("NonExistentId");
        Assert.IsNull(retrieved);
    }

    [Test]
    public void GetProperty_ReturnsNullForNullOrEmptyId()
    {
        Assert.IsNull(testObject.GetProperty(null));
        Assert.IsNull(testObject.GetProperty(""));
        Assert.IsNull(testObject.GetProperty("   "));
    }

    [Test]
    public void GetProperty_Generic_ReturnsPropertyByType()
    {
        var property = new FixedInPlace();
        testObject.AddProperty(property);

        var retrieved = testObject.GetProperty<FixedInPlace>();
        Assert.AreEqual(property, retrieved);
    }

    [Test]
    public void GetProperty_Generic_ReturnsNullWhenPropertyDoesNotExist()
    {
        var retrieved = testObject.GetProperty<FixedInPlace>();
        Assert.IsNull(retrieved);
    }

    [Test]
    public void RebuildPropertiesLookups_RebuildsLookupDictionary()
    {
        var property1 = new FixedInPlace();
        var property2 = new Lockable();
        
        testObject.AddProperty(property1);
        testObject.AddProperty(property2);

        // Manually clear the lookup (simulating deserialization)
        testObject.RebuildPropertiesLookups();

        Assert.IsTrue(testObject.HasProperty<FixedInPlace>());
        Assert.IsTrue(testObject.HasProperty<Lockable>());
        Assert.AreEqual(property1, testObject.GetProperty<FixedInPlace>());
        Assert.AreEqual(property2, testObject.GetProperty<Lockable>());
    }

    [Test]
    public void RebuildPropertiesLookups_HandlesNullProperties()
    {
        // Add a null property to the list (simulating corrupted data)
        testObject.properties.Add(null);
        
        // Should not throw
        Assert.DoesNotThrow(() => testObject.RebuildPropertiesLookups());
    }

    [Test]
    public void MultipleProperties_CanCoexist()
    {
        testObject.AddProperty(new FixedInPlace());
        testObject.AddProperty(new Lockable());
        testObject.AddProperty(new Openable());

        Assert.IsTrue(testObject.HasProperty<FixedInPlace>());
        Assert.IsTrue(testObject.HasProperty<Lockable>());
        Assert.IsTrue(testObject.HasProperty<Openable>());
    }
}

