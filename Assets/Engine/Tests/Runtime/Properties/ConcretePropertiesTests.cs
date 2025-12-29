using NUnit.Framework;
using System.Collections.Generic;

public class ConcretePropertiesTests
{
    [Test]
    public void FixedInPlace_IsBooleanProperty()
    {
        var property = new FixedInPlace();
        Assert.IsInstanceOf<BooleanProperty>(property);
        Assert.IsTrue(property.IsFixedInPlace);
    }

    [Test]
    public void FixedInPlace_CanBeSet()
    {
        var property = new FixedInPlace(isFixedInPlace: false);
        Assert.IsFalse(property.IsFixedInPlace);
        
        property.IsFixedInPlace = true;
        Assert.IsTrue(property.IsFixedInPlace);
    }

    [Test]
    public void Lockable_IsBooleanProperty()
    {
        var property = new Lockable();
        Assert.IsInstanceOf<BooleanProperty>(property);
        Assert.IsFalse(property.IsLocked);
    }

    [Test]
    public void Lockable_CanBeSet()
    {
        var property = new Lockable(isLocked: true);
        Assert.IsTrue(property.IsLocked);
        
        property.IsLocked = false;
        Assert.IsFalse(property.IsLocked);
    }

    [Test]
    public void Openable_IsBooleanProperty()
    {
        var property = new Openable();
        Assert.IsInstanceOf<BooleanProperty>(property);
        Assert.IsFalse(property.IsOpen);
    }

    [Test]
    public void Openable_CanBeSet()
    {
        var property = new Openable(isOpen: true);
        Assert.IsTrue(property.IsOpen);
        
        property.IsOpen = false;
        Assert.IsFalse(property.IsOpen);
    }

    [Test]
    public void Lightable_IsBooleanProperty()
    {
        var property = new Lightable();
        Assert.IsInstanceOf<BooleanProperty>(property);
        Assert.IsTrue(property.IsLit);
    }

    [Test]
    public void Lightable_CanBeSet()
    {
        var property = new Lightable(isLit: false);
        Assert.IsFalse(property.IsLit);
        
        property.IsLit = true;
        Assert.IsTrue(property.IsLit);
    }

    [Test]
    public void Writable_IsFunctionalProperty()
    {
        var property = new Writable();
        Assert.IsInstanceOf<FunctionalProperty>(property);
    }

    [Test]
    public void Writable_CanStoreWriting()
    {
        var property = new Writable("Hello, world!");
        Assert.AreEqual("Hello, world!", property.Writing);
        
        property.Writing = "New text";
        Assert.AreEqual("New text", property.Writing);
    }

    [Test]
    public void HoldsContents_IsFunctionalProperty()
    {
        var property = new HoldsContents();
        Assert.IsInstanceOf<FunctionalProperty>(property);
    }

    [Test]
    public void HoldsContents_CanStoreContents()
    {
        var item1 = TestObjectFactory.CreateBaseObject("Item1", "item one");
        var item2 = TestObjectFactory.CreateBaseObject("Item2", "item two");
        var contents = new Dictionary<string, BaseObject>
        {
            { "item one", item1 },
            { "item two", item2 }
        };
        
        var property = new HoldsContents(contents);
        Assert.AreEqual(2, property.Contents.Count);
        Assert.AreEqual(item1, property.Contents["item one"]);
        Assert.AreEqual(item2, property.Contents["item two"]);
    }

    [Test]
    public void Aliveness_IsFunctionalProperty()
    {
        var property = new Aliveness();
        Assert.IsInstanceOf<FunctionalProperty>(property);
    }

    [Test]
    public void Aliveness_CanStoreHealth()
    {
        var property = new Aliveness(health: 50);
        Assert.AreEqual(50, property.Health);
        
        property.Health = 75;
        Assert.AreEqual(75, property.Health);
    }

    [Test]
    public void Aliveness_IsAliveWhenHealthGreaterThanZero()
    {
        var property = new Aliveness(health: 100);
        Assert.IsTrue(property.IsAlive);
        
        property.Health = 0;
        Assert.IsFalse(property.IsAlive);
        
        property.Health = -10;
        Assert.IsFalse(property.IsAlive);
    }
}

