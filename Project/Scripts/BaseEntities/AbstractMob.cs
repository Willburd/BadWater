using Godot;
using System;
using System.Collections.Generic;

// Mob entities are objects on a map perform regular life updates, have special inventory slots to wear things, and recieve inputs from clients that they decide how to interpret.
public partial class AbstractMob : AbstractEntity
{ // Returns subtypes of behavior object
    public static AbstractMob CreateMob(PackData data, string data_string = "")
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
                new_mob = new Behaviors_BASE.AbstractComplexMob();
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
        // MobData temp = data as MobData;
    }

    /*****************************************************************
     * Click handling
     ****************************************************************/
    protected virtual void Examinate(AbstractEntity target)
    {
        if(target is null) return; //Could be gone by the time they finally pick something

        direction = TOOLS.RotateTowardEntity(this,target);

        string results = target.Examine(this);
        if(results == null || results.Length <= 0) results = "You were unable to examine that. Tell a developer!";

        ChatController.InspectMessage( this, results);
    }

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
