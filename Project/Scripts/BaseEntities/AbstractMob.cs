using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

// Mob entities are objects on a map perform regular life updates, have special inventory slots to wear things, and recieve inputs from clients that they decide how to interpret.
public partial class AbstractMob : AbstractEntity
{ // Returns subtypes of behavior object
    public static AbstractMob CreateMob(PackData data)
    {
        AbstractMob new_mob = null;
        switch(data.behaviorID)
        {
            /*****************************************************************
             * MOB BEHAVIORS (Living creatures and players)
             ****************************************************************/
            case "MOB_SIMPLE": // Life, death, status effects, breathing, eating, reagent processing.
                new_mob = new Behaviors_BASE.AbstractSimpleMob();
            break;

            case "MOB_COMPLEX": // Has species, unique markings, Has organs and limbs, DNA, traits, mutations, on top of all the stuff MOB has.
                new_mob = new Behaviors_BASE.AbstractSimpleMob();
            break;

            /*****************************************************************
             * UNIQUE MOB BEHAVIORS (Hyper specialized mobs that need entirely unique features)
             ****************************************************************/
            case "MOB_BORER":
                new_mob = new Behaviors_BASE.AbstractSimpleMob();
            break;

            /*****************************************************************
             * Debugging purposes only.
             ****************************************************************/
            default:
            case "_BEHAVIOR_":
                new_mob = new AbstractMob();
            break;
        }
        return new_mob;
    }


    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        MobData temp = data as MobData;
        max_health      = temp.max_health;
        health          = temp.max_health;
        walk_speed      = temp.walk_speed;
        run_speed       = temp.run_speed;
        // flags
        flags.HASHANDS      = temp.has_hands;
        flags.EXTRAHANDS    = temp.extra_hands;
        flags.COMPLEXTOOLS  = temp.complex_tools;
        // Inventory flags
        flags.WEARHAT       = temp.wears_hats;
        flags.WEARMASK      = temp.wears_mask;
        flags.WEAREYES      = temp.wears_eyes;
        flags.WEARUNIFORM   = temp.wears_uniform;
        flags.WEARSUIT      = temp.wears_suit;
        flags.WEARSHOES     = temp.wears_shoe;
        flags.WEAREARS      = temp.wears_ears;
        flags.WEARGLOVES    = temp.wears_glove;
        flags.WEARBELT      = temp.wears_belt;
    }
    // Gameplay interaction
    protected DAT.LifeState stat = DAT.LifeState.Alive;
    public DAT.LifeState Stat
    {
        get {return stat;}
    }
    public float max_health = 0;   // Size of item in world and bags
    public float health = 0;    // Size of item in world and bags
    public float walk_speed = (float)0.25;
    public float run_speed = 1;
    public Flags flags;
    public struct Flags
    {
        public Flags() {}
        // flags
        public bool HASHANDS = false;      // Can pick up objects
        public bool EXTRAHANDS = false;    // Has extra hand slots(basically just pockets without needing uniform)
        public bool COMPLEXTOOLS = false;  // If mob can use complex tools 
        // Inventory slot flags
        public bool WEARHAT = false;
        public bool WEARMASK = false;
        public bool WEAREYES = false;
        public bool WEARUNIFORM = false; // Controls more than uniform slot, allows pockets, id, back, etc
        public bool WEARSUIT = false;
        public bool WEARSHOES = false;
        public bool WEAREARS = false;
        public bool WEARGLOVES = false;
        public bool WEARBELT = false;
    }
    // End of template data

    // state data
    public AbstractEntity lastattacked = null;
    public AbstractEntity lastattacker = null;
    // end state data


    /*****************************************************************
     * Click cooldown
     ****************************************************************/
    protected int click_cooldown = 0;  // Time when mob cooldown has finished
    public void SetClickCooldown(int delay)
    {
        click_cooldown = Math.Max(MainController.WorldTicks + delay,click_cooldown);
    }
    protected bool CheckClickCooldown()
    {
        return click_cooldown > MainController.WorldTicks;
    }
    public int GetAttackCooldown(AbstractEntity item_used)
    {
        if(item_used == null) return DAT.DEFAULT_ATTACK_COOLDOWN;
        return DAT.DEFAULT_ATTACK_COOLDOWN;
    }
}
