using UnityEngine;

public abstract class ActionResponseLogic: ScriptableObject
{
    public abstract void Execute(GameController controller, BaseObject item);
}
