using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;

public partial class Behavior
{
    // Returns subtypes of behavior object
    public static Behavior CreateBehavior(string behavior_ID)
    {
        switch(behavior_ID)
        {
            case "_BEHAVIOR_": // Debugging purposes only.
                return new Behavior();
        }
        return null; // NO BEHAVIOR
    }



    // Called upon creation to set variables or state, usually detected by map information.
    public virtual void Init(AbstractEntity owner, MainController.DataType entity_type)
    {
        Generic_Init(entity_type);
    }
    public virtual void Init(NetworkEntity owner, MainController.DataType entity_type)          
    {
        Generic_Init(entity_type);
    }
    protected virtual void Generic_Init(MainController.DataType entity_type) // Use this if you don't need any unique type information from the owner to perform your actions!
    {
        GD.Print("INIT"); // REPLACE ME!!!
    }


    
    // Same as above, but when we NEED everything else Init() before we can properly tell our state!
    public virtual void LateInit(AbstractEntity owner, MainController.DataType entity_type)
    {
        Generic_LateInit(entity_type);
    }
    public virtual void LateInit(NetworkEntity owner, MainController.DataType entity_type)      
    {
        Generic_LateInit(entity_type);
    }
    protected virtual void Generic_LateInit(MainController.DataType entity_type) // Use this if you don't need any unique type information from the owner to perform your actions!
    {
        GD.Print("LATE INIT"); // REPLACE ME!!!
    }



    // Tick every game tick from the object it's inside of! Different for abstract and networked entities...
    public virtual void Tick(AbstractEntity owner, MainController.DataType entity_type)
    {
        Generic_Tick(entity_type);
    }
    public virtual void Tick(NetworkEntity owner, MainController.DataType entity_type)
    {
        Generic_Tick(entity_type);
    }
    protected virtual void Generic_Tick(MainController.DataType entity_type) // Use this if you don't need any unique type information from the owner to perform your actions!
    {
        GD.Print("TICK"); // REPLACE ME!!!
    }



    // Update graphical state of host entity/abstract! Different for abstract and networked entities...
    public virtual void UpdateIcon(AbstractEntity owner, MainController.DataType entity_type)
    {
        Generic_UpdateIcon(entity_type);
    }
    public virtual void UpdateIcon(NetworkEntity owner, MainController.DataType entity_type)
    {
        Generic_UpdateIcon(entity_type);
    }
    protected virtual void Generic_UpdateIcon(MainController.DataType entity_type) // Use this if you don't need any unique type information from the owner to perform your actions!
    {
        GD.Print("UPDATEICON"); // REPLACE ME!!!
    }
}
