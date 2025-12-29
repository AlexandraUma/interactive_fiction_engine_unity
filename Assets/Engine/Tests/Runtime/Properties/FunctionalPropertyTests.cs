using NUnit.Framework;

public class FunctionalPropertyTests
{
    [Test]
    public void InheritsFromBaseObjectProperty()
    {
        var property = new TestFunctionalProperty();
        Assert.IsInstanceOf<BaseObjectProperty>(property);
        Assert.IsNotNull(property.Id);
        // Description can be null initially (it's optional), but should be accessible
        string description = property.Description; // Should not throw
        // Can be null or empty - that's valid
        property.Description = "Test description";
        Assert.AreEqual("Test description", property.Description);
    }

    // Helper class for testing
    private class TestFunctionalProperty : FunctionalProperty
    {
    }
}

