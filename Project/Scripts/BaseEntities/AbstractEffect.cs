using Godot;
using System;
using System.Collections.Generic;

// Effect entities are map flags for spawners, synced decals, or other turf effects that can be interacted with, but not picked up, they do not update unless interacted with.
public class AbstractEffect : AbstractEntity
{
    public static AbstractEffect CreateEffect(PackData data, string data_string = "")
    {
        AbstractEffect new_effect = null;
        switch(data.behaviorID)
        {
            /*****************************************************************
             * EFFECT BEHAVIORS (Stuff like reagent smears being stepped through)
             ****************************************************************/
            case "EFFECT_MESS": // makes nearby turfs dirty when crossed
                new_effect = new Behaviors_BASE.AbstractOnStep(); // Performs behaviors when crossed
            break;
            case "EFFECT_MESS_STEPS": // leaves a trail of steps after you walk in it
                new_effect = new Behaviors_BASE.AbstractOnStep(); // Performs behaviors when crossed
            break;

            /*****************************************************************
             * MAP EVENT BEHAVIORS (stuff like onstep teleports)
             ****************************************************************/
            case "EVENT_ONSTEP":
                new_effect = new Behaviors_BASE.AbstractOnStep(); // Performs behaviors when crossed
            break;

            /*****************************************************************
             * Special effects
             ****************************************************************/
            case "POINT_AT":
                new_effect = new Behaviors_BASE.PointAt(); // Hovers above a point and vanishes
            break;

            case "RUNE_TEXT":
                new_effect = new Behaviors_BASE.RuneText(data_string); // Hovers above a point showing text, and then vanishes
            break;

            /*****************************************************************
             * Debugging purposes only.
             ****************************************************************/
            default:
            case "_BEHAVIOR_":
                new_effect = new AbstractEffect();
            break;
        }
        return new_effect;
    }

    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        EffectData temp = data as EffectData;
        is_spawner = temp.is_spawner;
        cleanable = temp.cleanable;
    } 
    
    public bool is_spawner = false; // Uses tag as ID
    public bool cleanable = false;
    // End of template data
}
