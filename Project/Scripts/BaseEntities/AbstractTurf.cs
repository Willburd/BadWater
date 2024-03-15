using Godot;
using System;
using System.Collections.Generic;

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

    public new void Move(string new_mapID, MapController.GridPos new_pos, bool perform_turf_actions = true)
    {
        map_id_string = new_mapID;
        grid_pos = new_pos;
    }
    public new void Move(string new_mapID, Vector3 new_pos, bool perform_turf_actions = true)
    {
        Move(new_mapID, new MapController.GridPos(new_pos), perform_turf_actions);
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
        AudioController.PlayAt(step_sound, map_id_string ,grid_pos.WorldPos() + new Vector3(MapController.tile_size/2,0,MapController.tile_size/2), AudioController.screen_range, quiet ? -10 : 0);
    }
}
