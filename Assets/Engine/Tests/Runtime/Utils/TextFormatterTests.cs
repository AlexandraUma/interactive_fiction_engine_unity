using NUnit.Framework;

public class TextFormatterTests
{
    [Test]
    public void FormatEvent_FormatsPrologueEvent()
    {
        var gameEvent = new IFEvent(EventType.PROLOGUE, "This is a prologue.");
        string formatted = TextFormatter.FormatEvent(gameEvent);
        Assert.IsTrue(formatted.Contains("This is a prologue."));
        Assert.IsTrue(formatted.Contains("<i>"));
    }

    [Test]
    public void FormatEvent_FormatsPlayerInputEvent()
    {
        var gameEvent = new IFEvent(EventType.PLAYER_INPUT, "take item");
        string formatted = TextFormatter.FormatEvent(gameEvent);
        Assert.IsTrue(formatted.Contains("take item"));
        Assert.IsTrue(formatted.Contains("<b>"));
        Assert.IsTrue(formatted.Contains(">"));
    }

    [Test]
    public void FormatEvent_FormatsRoomNameEvent()
    {
        var gameEvent = new IFEvent(EventType.ROOM_NAME, "Test Room");
        string formatted = TextFormatter.FormatEvent(gameEvent);
        Assert.IsTrue(formatted.Contains("Test Room"));
        Assert.IsTrue(formatted.Contains("<b>"));
    }

    [Test]
    public void FormatEvent_FormatsDefaultEvent()
    {
        var gameEvent = new IFEvent(EventType.WORLD_RESPONSE, "You take the item.");
        string formatted = TextFormatter.FormatEvent(gameEvent);
        Assert.IsTrue(formatted.Contains("You take the item."));
    }

    [Test]
    public void Bold_WrapsTextInBoldTags()
    {
        string result = TextFormatter.Bold("test");
        Assert.AreEqual("<b>test</b>", result);
    }

    [Test]
    public void Italic_WrapsTextInItalicTags()
    {
        string result = TextFormatter.Italic("test");
        Assert.AreEqual("<i>test</i>", result);
    }

    [Test]
    public void Color_WrapsTextInColorTags()
    {
        string result = TextFormatter.Color("test", "#FF0000");
        Assert.AreEqual("<color=#FF0000>test</color>", result);
    }

    [Test]
    public void Size_WrapsTextInSizeTags()
    {
        string result = TextFormatter.Size("test", 20);
        Assert.AreEqual("<size=20>test</size>", result);
    }

    [Test]
    public void Underline_WrapsTextInUnderlineTags()
    {
        string result = TextFormatter.Underline("test");
        Assert.AreEqual("<u>test</u>", result);
    }
}

