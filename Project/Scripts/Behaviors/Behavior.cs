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
            /*****************************************************************
             * TURF BEHAVIORS (turf that behaves in certain ways)
             ****************************************************************/
            case "TURF_RAW":
                new_behave = new BehaviorEvents.TurfBasic(0); // Bottommost turf build level! Dirt/Sand/Rock.
            break;

            case "TURF_PANEL":
                new_behave = new BehaviorEvents.TurfBasic(1); // Second level of construction, PANEL
            break;
            
            case "TURF_FLOOR":
                new_behave = new BehaviorEvents.TurfBasic(2); // Flooring over top of a panel!
            break;
            
            case "TURF_WALL":
                new_behave = new BehaviorEvents.TurfBasic(4); // Wall over top of a panel!
            break;

            /*****************************************************************
             * EFFECT BEHAVIORS (Stuff like reagent smears being stepped through)
             ****************************************************************/
            case "EFFECT_MESS": // makes nearby turfs dirty when crossed
                //new_behave = new BehaviorEvents.OnStepped(); // Performs behaviors when crossed
            break;
            case "EFFECT_MESS_STEPS": // leaves a trail of steps after you walk in it
                //new_behave = new BehaviorEvents.OnStepped(); // Performs behaviors when crossed
            break;

            /*****************************************************************
             * MAP EVENT BEHAVIORS (stuff like onstep teleports)
             ****************************************************************/
            case "EVENT_ONSTEP":
                new_behave = new BehaviorEvents.OnStepped(); // Performs behaviors when crossed
            break;

            /*****************************************************************
             * Debugging purposes only.
             ****************************************************************/
            case "_BEHAVIOR_":
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



    // This is a bit of a mess, but it needs to handle all abstract and network entity types being crossed with both abstract and network entities
    public virtual void Crossed(AbstractEntity owner, MainController.DataType entity_type, AbstractEntity crosser)
    {
        Abstract_Crossed(entity_type,crosser);
    }
    public virtual void Crossed(AbstractEntity owner, MainController.DataType entity_type, NetworkEntity crosser)
    {
        Entity_Crossed(entity_type,crosser);
    }
    public virtual void Crossed(NetworkEntity owner, MainController.DataType entity_type, AbstractEntity crosser)
    {
        Abstract_Crossed(entity_type,crosser);
    }
    public virtual void Crossed(NetworkEntity owner, MainController.DataType entity_type, NetworkEntity crosser)
    {
        Entity_Crossed(entity_type,crosser);
    }
    protected virtual void Abstract_Crossed(MainController.DataType entity_type, AbstractEntity crosser) // Use this if you don't need any unique type information from the owner to perform your actions!
    {
        Generic_Crossed(entity_type);
    }
    protected virtual void Entity_Crossed(MainController.DataType entity_type, NetworkEntity crosser) // Use this if you don't need any unique type information from the owner to perform your actions!
    {
        Generic_Crossed(entity_type);
    }
    protected virtual void Generic_Crossed(MainController.DataType entity_type) // Use this if the object doesn't need any information from the owner, OR the crosser!
    {
        GD.Print("CROSSED"); // REPLACE ME!!!
    }



    // This is a bit of a mess, but it needs to handle all abstract and network entity types being crossed with both abstract and network entities
    public virtual void UnCrossed(AbstractEntity owner, MainController.DataType entity_type, AbstractEntity crosser)
    {
        Abstract_UnCrossed(entity_type,crosser);
    }
    public virtual void UnCrossed(AbstractEntity owner, MainController.DataType entity_type, NetworkEntity crosser)
    {
        Entity_UnCrossed(entity_type,crosser);
    }
    public virtual void UnCrossed(NetworkEntity owner, MainController.DataType entity_type, AbstractEntity crosser)
    {
        Abstract_UnCrossed(entity_type,crosser);
    }
    public virtual void UnCrossed(NetworkEntity owner, MainController.DataType entity_type, NetworkEntity crosser)
    {
        Entity_UnCrossed(entity_type,crosser);
    }
    protected virtual void Abstract_UnCrossed(MainController.DataType entity_type, AbstractEntity crosser) // Use this if you don't need any unique type information from the owner to perform your actions!
    {
        Generic_UnCrossed(entity_type);
    }
    protected virtual void Entity_UnCrossed(MainController.DataType entity_type, NetworkEntity crosser) // Use this if you don't need any unique type information from the owner to perform your actions!
    {
        Generic_UnCrossed(entity_type);
    }
    protected virtual void Generic_UnCrossed(MainController.DataType entity_type) // Use this if the object doesn't need any information from the owner, OR the crosser!
    {
        GD.Print("UNCROSSED"); // REPLACE ME!!!
    }



    // visibility state of entity, only matters if on a turf and not inside anything.
    public virtual bool IsNetworkVisible()
    {
        return true;
    }
}
