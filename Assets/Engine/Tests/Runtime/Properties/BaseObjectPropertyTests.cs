using NUnit.Framework;

public class BaseObjectPropertyTests
{
    [Test]
    public void Id_ReturnsTypeName()
    {
        var property = new TestProperty();
        Assert.AreEqual("TestProperty", property.Id);
    }

    [Test]
    public void Description_CanBeSetAndRetrieved()
    {
        var property = new TestProperty();
        property.Description = "Test description";
        Assert.AreEqual("Test description", property.Description);
    }

    // Helper class for testing
    private class TestProperty : BaseObjectProperty
    {
    }
}

