using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;

public partial class Behavior
{
    // Returns subtypes of behavior object
    public static Behavior CreateBehavior(PackData data)
    {
        Behavior new_behave = null;
        switch(data.behaviorID)
        {
            case "TURF_RAW":
                new_behave = new TurfBasic(0); // Bottommost turf build level! Dirt/Sand/Rock.
            break;

            case "TURF_PANEL":
                new_behave = new TurfBasic(1); // Second level of construction, PANEL
            break;
            
            case "TURF_FLOOR":
                new_behave = new TurfBasic(2); // Flooring over top of a panel!
            break;
            
            case "TURF_WALL":
                new_behave = new TurfBasic(4); // Wall over top of a panel!
            break;



            case "_BEHAVIOR_": // Debugging purposes only.
                new_behave = new Behavior();
            break;
        }
        new_behave?.MapLoadVars(data.GetTempData());
        return null; // NO BEHAVIOR
    }


    // Used to read from the json and get custom data that's packed into thesame place as the rest!
    public virtual void MapLoadVars(Godot.Collections.Dictionary data)
    {

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
