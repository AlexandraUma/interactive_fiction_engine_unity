using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class BaseObjectActionLookupsTests
{
    private BaseObject testObject;

    [SetUp]
    public void SetUp()
    {
        testObject = TestObjectFactory.CreateBaseObject("TestObject", "Test Object");
    }

    [Test]
    public void GetOverrideFor_ReturnsOverrideAction()
    {
        var overrideAction = new Take();
        var entry = new ActionOverrideEntry
        {
            actionKeyword = "take",
            anActionThatOverridesTheDefaultAction = overrideAction
        };
        testObject.actionOverrides = new List<ActionOverrideEntry> { entry };
        testObject.RebuildActionLookupsPublic();

        var result = testObject.GetOverrideFor("take");
        Assert.AreEqual(overrideAction, result);
    }

    [Test]
    public void GetOverrideFor_ReturnsNullWhenNoOverride()
    {
        testObject.RebuildActionLookupsPublic();
        var result = testObject.GetOverrideFor("take");
        Assert.IsNull(result);
    }

    [Test]
    public void GetOverrideFor_ThrowsForNullOrEmptyKeyword()
    {
        testObject.RebuildActionLookupsPublic();
        Assert.Throws<ArgumentException>(() => testObject.GetOverrideFor(null));
        Assert.Throws<ArgumentException>(() => testObject.GetOverrideFor(""));
        Assert.Throws<ArgumentException>(() => testObject.GetOverrideFor("   "));
    }

    [Test]
    public void GetOverrideFor_IsCaseInsensitive()
    {
        var overrideAction = new Take();
        var entry = new ActionOverrideEntry
        {
            actionKeyword = "take",
            anActionThatOverridesTheDefaultAction = overrideAction
        };
        testObject.actionOverrides = new List<ActionOverrideEntry> { entry };
        testObject.RebuildActionLookupsPublic();

        var result = testObject.GetOverrideFor("TAKE");
        Assert.AreEqual(overrideAction, result);
    }

    [Test]
    public void GetActionResponsesFor_ReturnsResponseList()
    {
        // Note: ActionResponseLogic is abstract, so we test with null for now
        // In real usage, concrete implementations would be used
        var entry = new ActionResponseEntry
        {
            actionKeyword = "take",
            responses = new List<ActionResponseLogic> { null }
        };
        testObject.actionResponses = new List<ActionResponseEntry> { entry };
        testObject.RebuildActionLookupsPublic();

        var result = testObject.GetActionResponsesFor("take");
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
    }

    [Test]
    public void GetActionResponsesFor_ReturnsNullWhenNoResponses()
    {
        testObject.RebuildActionLookupsPublic();
        var result = testObject.GetActionResponsesFor("take");
        Assert.IsNull(result);
    }

    [Test]
    public void GetActionResponsesFor_ThrowsForNullOrEmptyKeyword()
    {
        testObject.RebuildActionLookupsPublic();
        Assert.Throws<ArgumentException>(() => testObject.GetActionResponsesFor(null));
        Assert.Throws<ArgumentException>(() => testObject.GetActionResponsesFor(""));
        Assert.Throws<ArgumentException>(() => testObject.GetActionResponsesFor("   "));
    }

    [Test]
    public void GetTextResponseFor_ReturnsTextResponse()
    {
        var textResponse = new TextResponse
        {
            actionKeyword = "take",
            response = "You grab it."
        };
        testObject.textResponses = new List<TextResponse> { textResponse };
        testObject.RebuildActionLookupsPublic();

        var result = testObject.GetTextResponseFor("take");
        Assert.AreEqual("You grab it.", result);
    }

    [Test]
    public void GetTextResponseFor_ReturnsNullWhenNoResponse()
    {
        testObject.RebuildActionLookupsPublic();
        var result = testObject.GetTextResponseFor("take");
        Assert.IsNull(result);
    }

    [Test]
    public void GetTextResponseFor_ThrowsForNullOrEmptyKeyword()
    {
        testObject.RebuildActionLookupsPublic();
        Assert.Throws<ArgumentException>(() => testObject.GetTextResponseFor(null));
        Assert.Throws<ArgumentException>(() => testObject.GetTextResponseFor(""));
        Assert.Throws<ArgumentException>(() => testObject.GetTextResponseFor("   "));
    }

    [Test]
    public void RebuildActionLookups_ThrowsOnDuplicateTextResponses()
    {
        var response1 = new TextResponse { actionKeyword = "take", response = "Response 1" };
        var response2 = new TextResponse { actionKeyword = "take", response = "Response 2" };
        testObject.textResponses = new List<TextResponse> { response1, response2 };

        Assert.Throws<ArgumentException>(() => testObject.RebuildActionLookupsPublic());
    }

    [Test]
    public void RebuildActionLookups_ThrowsOnEmptyActionKeyword()
    {
        var response = new TextResponse { actionKeyword = "", response = "Response" };
        testObject.textResponses = new List<TextResponse> { response };

        Assert.Throws<ArgumentException>(() => testObject.RebuildActionLookupsPublic());
    }

    [Test]
    public void RebuildActionLookups_HandlesNullEntries()
    {
        testObject.textResponses = new List<TextResponse> { null };
        testObject.actionResponses = new List<ActionResponseEntry> { null };
        testObject.actionOverrides = new List<ActionOverrideEntry> { null };

        Assert.DoesNotThrow(() => testObject.RebuildActionLookupsPublic());
    }
}

