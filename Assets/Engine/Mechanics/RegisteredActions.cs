using System.Collections.Generic;
public static class RegisteredActions
{
    public static List<Action> Create()
    {
        return new List<Action>
           {
               new Attack(),
               new Close(),
               new Examine(),
               new Go(),
               new Listen(),
               new Lock(),
               new Look(),
               new Open(),
               new Smell(),
               new Take(),
               new Unlock(),
               new Inventory(),
           };
    }
}