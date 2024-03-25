using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

public partial class AbstractTurf : AbstractEntity
{
    public static AbstractTurf CreateTurf(PackData data)
    {
        AbstractTurf new_turf = null;
        switch(data.behaviorID)
        {
            /*****************************************************************
             * TURF BEHAVIORS (turf that behaves in certain ways)
             ****************************************************************/
            case "TURF_RAW":
                new_turf = new Behaviors_BASE.AbstractBasicTurf(0); // Bottommost turf build level! Dirt/Sand/Rock.
            break;

            case "TURF_PANEL":
                new_turf = new Behaviors_BASE.AbstractBasicTurf(1); // Second level of construction, PANEL
            break;
            
            case "TURF_FLOOR":
                new_turf = new Behaviors_BASE.AbstractBasicTurf(2); // Flooring over top of a panel!
            break;
            
            case "TURF_WALL":
                new_turf = new Behaviors_BASE.AbstractBasicTurf(4); // Wall over top of a panel!
            break;

            case "TURF_MINEABLE":
                new_turf = new Behaviors_BASE.AbstractMineableTurf(); // Wall over top of a panel!
            break;

            /*****************************************************************
             * Debugging purposes only.
             ****************************************************************/
            default:
            case "_BEHAVIOR_":
                new_turf = new AbstractTurf();
            break;
        }
        return new_turf;
    }

    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        TurfData temp = data as TurfData;
        model = temp.model;
        texture = temp.texture;
        anim_speed = temp.anim_speed;
        density = temp.density;
        opaque = temp.opaque;
        step_sound = temp.step_sound;
    }
    // End of template data


    AtmoController.AtmoCell air_mix = null;
    private AbstractArea area = null;

    public new void Move(MapController.GridPos new_pos, bool perform_turf_actions = true)
    {
        grid_pos = new_pos;
    }
    public new void Move(AbstractEntity new_container, bool perform_turf_actions = true)
    {
        GD.PrintErr("Attempted to move turf into AbstractEntity...");
    }   

    public virtual void RandomTick()                // Some turfs respond to random updates, every area will perform a number of them based on the area's size!
    {

    }

    public void AtmosphericsCheck()
    {
        // check all four directions for any atmo diffs, if any flag for update - TODO =================================================================================================================================
    }

    public AbstractArea Area
    {
        get {return area;}
        set {area = value;} // SET USING Area.AddTurf() DO NOT SET DIRECTLY
    }

    public void PlayStepSound(bool quiet)
    {
        AudioController.PlayAt(step_sound, GridPos.GetCentered(), AudioController.screen_range, quiet ? -10 : 0);
    }

    // Override this for turf interactions. Construction, deconstruction, maybe even rotation! Who knows what magic sins are ahead in our codebase.
    public virtual bool InteractTurf(AbstractEntity used_item, AbstractEntity user)
    {
        return false;
    }
    public virtual bool AttackTurf(AbstractEntity used_item, AbstractEntity user)
    {
        // Hits a mob on the turf. Walls should override this to attack the wall!
        if(used_item == null) return false; // Ignore unarmed strikes over turfs.

        List<AbstractMob> viable_targets = new List<AbstractMob>();
        bool success = false; // Hitting something makes this true. If its still false, the miss sound is played.

        foreach(AbstractEntity ent in Contents)
        {
            if(ent is AbstractMob && ent != user) viable_targets.Add(ent as AbstractMob);
        }

        if(viable_targets.Count <= 0) // No valid targets on this tile.
        {
            //if(W.can_cleave) success = W.cleave(user, src) // TODO Cleaving weapons =================================================================================================================
        }
        else
        {
            AbstractMob victim = TOOLS.Pick(viable_targets);
            success = used_item._Interact(victim, user, 0, new Godot.Collections.Dictionary());
        }

        if(user is AbstractMob user_mob)
        {
            user_mob.SetClickCooldown(user_mob.GetAttackCooldown(used_item));
            user_mob?.LoadedNetworkEntity?.AnimationRequest(NetwornAnimations.Animation.ID.Attack, MapController.GetMapDirection(user,this) );
        }
        if(!success) // Nothing got hit.
        {
            ChatController.VisibleMessage(user,user?.display_name.The(true) + " swipes the " + used_item?.display_name.The() + " over the " + this.display_name.The() + ".", ChatController.VisibleMessageFormatting.Warning);
            AudioController.PlayAt("BASE/Attacks/Punch/Miss", grid_pos, AudioController.screen_range, 0);
        }
        return success;
    }
}
