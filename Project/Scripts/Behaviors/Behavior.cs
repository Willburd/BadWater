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
    public virtual void Init(AbstractEntity self)
    {
        //GD.Print("INIT " + self.display_name); // REPLACE ME!!!
    }
    
    // Same as above, but when we NEED everything else Init() before we can properly tell our state!
    public virtual void LateInit(AbstractEntity self)
    {
        //GD.Print("LATE INIT " + self.display_name); // REPLACE ME!!!
    }

    // Tick every game tick from the object it's inside of! Different for abstract and networked entities...
    public virtual void Tick(AbstractEntity self)
    {
        //GD.Print("TICK " + self.display_name); // REPLACE ME!!!
    }

    public virtual void HandleInput(AbstractEntity self, Godot.Collections.Dictionary input)
    {
        //GD.Print("INPUTS " + self.display_name); // REPLACE ME!!!
    }

    // Update graphical state of host entity/abstract! Different for abstract and networked entities... 
    public virtual void UpdateIcon(AbstractEntity self)
    {
        // ALWAYS CALL base.UpdateIcon() AT END;
        self.UpdateNetwork(true);
    }

    public virtual void Crossed(AbstractEntity self, AbstractEntity crosser)
    {
        //GD.Print("CROSSED " + self.display_name); // REPLACE ME!!!
    }
    public virtual void UnCrossed(AbstractEntity self, AbstractEntity crosser)
    {
        //GD.Print("UNCROSSED " + self.display_name); // REPLACE ME!!!
    }

    public virtual void Bump(AbstractEntity self, AbstractEntity hitby)
    {
        GD.Print("BUMPED " + self.display_name); // REPLACE ME!!!
    }


    // visibility state of entity, only matters if on a turf and not inside anything.
    public virtual bool IsNetworkVisible()
    {
        return true;
    }

    public virtual bool IsIntangible(AbstractEntity self)
    {
        return self.intangible;
    }

    // Click interactions
    public virtual void Clicked(AbstractEntity self, AbstractEntity use_item, AbstractEntity target, Godot.Collections.Dictionary click_params)
    {
        GD.Print(self?.display_name + " CLICKED " + target?.display_name + " USING " + use_item?.display_name); // REPLACE ME!!!
    }

    public virtual void Dragged(AbstractEntity self, AbstractEntity user, AbstractEntity target,Godot.Collections.Dictionary click_params)
    {
        GD.Print(user?.display_name + " DRAGGED " + self?.display_name + " TO " + target?.display_name); // REPLACE ME!!!
    }

    public virtual void AttackSelf(AbstractEntity self, AbstractEntity user)
    {
        GD.Print(user?.display_name + " USED " + self?.display_name + " ON ITSELF"); // REPLACE ME!!!
    }

    //do stuff before attackby!
    public virtual bool PreAttack(AbstractEntity self, AbstractEntity user, AbstractEntity target, Godot.Collections.Dictionary click_parameters) 
    {
        return false; //return TRUE to avoid calling attackby after this proc does stuff
    }
    
    public bool Attack(AbstractEntity self, AbstractEntity user, AbstractEntity target, float attack_modifier, Godot.Collections.Dictionary click_parameters)
    {
        bool success = PreAttack( self, user, target,click_parameters);
        if(success)	return true; // We're returning the value of pre_attack, important if it has a special return.
        return target.AttackedBy( user, attack_modifier, click_parameters);
    }


    public bool AttackedBy(AbstractEntity self, AbstractEntity use_item, AbstractEntity user, float attack_modifier, Godot.Collections.Dictionary click_parameters)
    {
        if(user is not AbstractMob) return false;
        /*if(can_operate(src, user) && I.do_surgery(src,user,user.zone_sel.selecting))
            return TRUE*/
        return use_item.WeaponAttack( self, user, self, /*user.zone_sel.selecting*/ 0, attack_modifier);
    }   

    public bool WeaponAttack(AbstractEntity self, AbstractEntity user, AbstractEntity target, int target_zone, float attack_modifier)
    {
        /*
        if(!force || (flags & NOBLUDGEON)) return false;
        if(M == user && user.a_intent != I_HURT) return false;

        /////////////////////////
        user.lastattacked = M
        M.lastattacker = user

        if(!no_attack_log) add_attack_logs(user,M,"attacked with [name] (INTENT: [uppertext(user.a_intent)]) (DAMTYE: [uppertext(damtype)])")
        /////////////////////////

        user.SetClickCooldown(user.GetAttackSpeed(src))
        user.do_attack_animation(M)

        var/hit_zone = M.resolve_item_attack(src, user, target_zone)
        if(hit_zone)
        {
            apply_hit_effect(M, user, hit_zone, attack_modifier);
        }
        */
        return true;
    }

    public virtual bool UnarmedAttack(AbstractEntity self, AbstractEntity target, bool proximity)
    {
        return false;
    }
    public virtual void AfterAttack(AbstractEntity self, AbstractEntity user, AbstractEntity target, bool proximity, Godot.Collections.Dictionary click_parameters)
    {

    }
}
