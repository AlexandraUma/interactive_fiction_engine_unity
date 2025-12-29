using NUnit.Framework;

public class BooleanPropertyTests
{
    [Test]
    public void Value_CanBeSetAndRetrieved()
    {
        var property = new TestBooleanProperty();
        property.Value = true;
        Assert.IsTrue(property.Value);
        
        property.Value = false;
        Assert.IsFalse(property.Value);
    }

    [Test]
    public void Enable_SetsValueToTrue()
    {
        var property = new TestBooleanProperty { Value = false };
        property.Enable();
        Assert.IsTrue(property.Value);
    }

    [Test]
    public void Disable_SetsValueToFalse()
    {
        var property = new TestBooleanProperty { Value = true };
        property.Disable();
        Assert.IsFalse(property.Value);
    }

    [Test]
    public void Toggle_FlipsValue()
    {
        var property = new TestBooleanProperty { Value = false };
        property.Toggle();
        Assert.IsTrue(property.Value);
        
        property.Toggle();
        Assert.IsFalse(property.Value);
    }

    [Test]
    public void InheritsFromBaseObjectProperty()
    {
        var property = new TestBooleanProperty();
        Assert.IsInstanceOf<BaseObjectProperty>(property);
        Assert.IsNotNull(property.Id);
        // Description can be null initially (it's optional), but should be accessible
        string description = property.Description; // Should not throw
        // Can be null or empty - that's valid
        property.Description = "Test description";
        Assert.AreEqual("Test description", property.Description);
    }

    // Helper class for testing
    private class TestBooleanProperty : BooleanProperty
    {
    }
}

