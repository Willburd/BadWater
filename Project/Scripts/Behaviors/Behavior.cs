using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;

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

            case "TURF_MINEABLE":
                new_behave = new BehaviorEvents.TurfMineable(); // Wall over top of a panel!
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
             * MOB BEHAVIORS (Living creatures and players)
             ****************************************************************/
            case "MOB_SIMPLE": // Life, death, status effects, breathing, eating, reagent processing.
                new_behave = new BehaviorEvents.SimpleMob();
            break;

            case "MOB_COMPLEX": // Has species, unique markings, Has organs and limbs, DNA, traits, mutations, on top of all the stuff MOB has.
                new_behave = new BehaviorEvents.SimpleMob();
            break;

            /*****************************************************************
             * UNIQUE MOB BEHAVIORS (Hyper specialized mobs that need entirely unique features)
             ****************************************************************/
            case "MOB_BORER":
                new_behave = new BehaviorEvents.SimpleMob();
            break;

            /*****************************************************************
             * Debugging purposes only.
             ****************************************************************/
            default:
            case "_BEHAVIOR_":
                new_behave = new Behavior();
            break;
        }
        new_behave?.MapLoadVars(data.GetTempData());
        return new_behave;
    }


    // Used to read from the json and get custom data that's packed into thesame place as the rest!
    public virtual void MapLoadVars(Godot.Collections.Dictionary data)
    {

    }

    // Called upon creation to set variables or state, usually detected by map information.
    public virtual void Init(AbstractEntity self, MainController.DataType entity_type)
    {
        //GD.Print("INIT " + self.display_name); // REPLACE ME!!!
    }
    
    // Same as above, but when we NEED everything else Init() before we can properly tell our state!
    public virtual void LateInit(AbstractEntity self, MainController.DataType entity_type)
    {
        //GD.Print("LATE INIT " + self.display_name); // REPLACE ME!!!
    }

    // Tick every game tick from the object it's inside of! Different for abstract and networked entities...
    public virtual void Tick(AbstractEntity self, MainController.DataType entity_type)
    {
        //GD.Print("TICK " + self.display_name); // REPLACE ME!!!
    }

    public virtual void HandleInput(AbstractEntity self, MainController.DataType entity_type, Godot.Collections.Dictionary input)
    {
        //GD.Print("INPUTS " + self.display_name); // REPLACE ME!!!
    }

    // Update graphical state of host entity/abstract! Different for abstract and networked entities... 
    public virtual void UpdateIcon(AbstractEntity self, MainController.DataType entity_type)
    {
        // ALWAYS CALL base.UpdateIcon() AT END;
        self.UpdateNetwork(true);
    }

    public virtual void Crossed(AbstractEntity self, MainController.DataType entity_type, AbstractEntity crosser)
    {
        //GD.Print("CROSSED " + self.display_name); // REPLACE ME!!!
    }
    public virtual void UnCrossed(AbstractEntity self, MainController.DataType entity_type, AbstractEntity crosser)
    {
        //GD.Print("UNCROSSED " + self.display_name); // REPLACE ME!!!
    }

    public virtual void Bump(AbstractEntity self, MainController.DataType entity_type, AbstractEntity hitby)
    {
        GD.Print("BUMPED " + self.display_name); // REPLACE ME!!!
    }


    // visibility state of entity, only matters if on a turf and not inside anything.
    public virtual bool IsNetworkVisible()
    {
        return true;
    }

    // Click interactions
    public void Click(AbstractEntity self, MainController.DataType entity_type, AbstractEntity target, Godot.Collections.Dictionary click_params)
    {
        GD.Print("CLICKED " + self.display_name); // REPLACE ME!!!
    }

    public void Drag(AbstractEntity self, MainController.DataType entity_type, AbstractEntity target,Godot.Collections.Dictionary click_params)
    {
        GD.Print("DRAGGED " + self.display_name + " TO " + target.display_name); // REPLACE ME!!!
    }
}
