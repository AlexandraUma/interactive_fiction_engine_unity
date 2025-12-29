using System;
using System.Collections.Generic;

public partial class BaseObject
{
    // Runtime lookups for fast per-action queries. These are rebuilt from the
    // serialized lists (textResponses, actionResponses, actionOverrides) and
    // are never serialized themselves.
    [NonSerialized]
    private readonly Dictionary<string, TextResponse> _textResponsesByKeyword =
        new Dictionary<string, TextResponse>(StringComparer.OrdinalIgnoreCase);

    [NonSerialized]
    private readonly Dictionary<string, ActionResponseEntry> _actionResponsesByKeyword =
        new Dictionary<string, ActionResponseEntry>(StringComparer.OrdinalIgnoreCase);

    [NonSerialized]
    private readonly Dictionary<string, ActionOverrideEntry> _actionOverridesByKeyword =
        new Dictionary<string, ActionOverrideEntry>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Rebuilds the per-action lookup dictionaries from the serialized lists
    /// on this object. Called during OnEnable/OnValidate so that configuration
    /// errors surface early in testing.
    /// </summary>
    partial void RebuildActionLookups()
    {
        _textResponsesByKeyword.Clear();
        _actionResponsesByKeyword.Clear();
        _actionOverridesByKeyword.Clear();

        // Text responses
        if (textResponses != null)
        {
            foreach (TextResponse tr in textResponses)
            {
                if (tr == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(tr.actionKeyword))
                {
                    throw new ArgumentException(
                        $"BaseObject '{name}' has a TextResponse with an empty actionKeyword.");
                }

                if (_textResponsesByKeyword.ContainsKey(tr.actionKeyword))
                {
                    throw new ArgumentException(
                        $"BaseObject '{name}' defines multiple TextResponses for action '{tr.actionKeyword}'.");
                }

                _textResponsesByKeyword[tr.actionKeyword] = tr;
            }
        }

        // Action responses
        if (actionResponses != null)
        {
            foreach (ActionResponseEntry entry in actionResponses)
            {
                if (entry == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.actionKeyword))
                {
                    throw new ArgumentException(
                        $"BaseObject '{name}' has an ActionResponseEntry with an empty actionKeyword.");
                }

                if (_actionResponsesByKeyword.ContainsKey(entry.actionKeyword))
                {
                    throw new ArgumentException(
                        $"BaseObject '{name}' defines multiple ActionResponseEntries for action '{entry.actionKeyword}'.");
                }

                _actionResponsesByKeyword[entry.actionKeyword] = entry;
            }
        }

        // Action overrides
        if (actionOverrides != null)
        {
            foreach (ActionOverrideEntry entry in actionOverrides)
            {
                if (entry == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.actionKeyword))
                {
                    throw new ArgumentException(
                        $"BaseObject '{name}' has an ActionOverrideEntry with an empty actionKeyword.");
                }

                if (_actionOverridesByKeyword.ContainsKey(entry.actionKeyword))
                {
                    throw new ArgumentException(
                        $"BaseObject '{name}' defines multiple ActionOverrideEntries for action '{entry.actionKeyword}'.");
                }

                _actionOverridesByKeyword[entry.actionKeyword] = entry;
            }
        }
    }

    /// <summary>
    /// Public method to rebuild action lookups. Useful for testing.
    /// </summary>
    public void RebuildActionLookupsPublic()
    {
        RebuildActionLookups();
    }

    /// <summary>
    /// Returns the text response associated with a given action keyword,
    /// or null if none is defined for this object.
    /// </summary>
    public string GetTextResponseFor(string actionKeyword)
    {
        if (string.IsNullOrWhiteSpace(actionKeyword))
        {
            throw new ArgumentException("Action keyword must be non-empty.", nameof(actionKeyword));
        }

        if (_textResponsesByKeyword.TryGetValue(actionKeyword, out TextResponse response) &&
            !string.IsNullOrWhiteSpace(response?.response))
        {
            return response.response;
        }

        return null;
    }

    /// <summary>
    /// Returns the overriding Action for a given keyword, or null if none
    /// is defined for this object.
    /// </summary>
    public Action GetOverrideFor(string actionKeyword)
    {
        if (string.IsNullOrWhiteSpace(actionKeyword))
        {
            throw new ArgumentException("Action keyword must be non-empty.", nameof(actionKeyword));
        }

        if (_actionOverridesByKeyword.TryGetValue(actionKeyword, out ActionOverrideEntry entry))
        {
            return entry?.anActionThatOverridesTheDefaultAction;
        }

        return null;
    }

    /// <summary>
    /// Returns the list of action response logics for a given keyword,
    /// or null if none are defined.
    /// </summary>
    public List<ActionResponseLogic> GetActionResponsesFor(string actionKeyword)
    {
        if (string.IsNullOrWhiteSpace(actionKeyword))
        {
            throw new ArgumentException("Action keyword must be non-empty.", nameof(actionKeyword));
        }

        if (_actionResponsesByKeyword.TryGetValue(actionKeyword, out ActionResponseEntry entry))
        {
            return entry?.responses;
        }

        return null;
    }
}
