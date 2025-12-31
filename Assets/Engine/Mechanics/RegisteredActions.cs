using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RegisteredActions
{
    public static List<Action> Create()
    {
        // Automatically loads all Action assets from Resources/Actions/
        return Resources.LoadAll<Action>("CoreActions").ToList();
    }
}