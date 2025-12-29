using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// To store the core/registered actions for the game.
/// </summary>
[CreateAssetMenu(fileName = "New ActionSet", menuName = "IFEngine/Actions/ActionSet")]
public class ActionSet: ScriptableObject
{
    public List<Action> actions = new();
}