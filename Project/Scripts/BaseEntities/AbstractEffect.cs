using Godot;
using System;
using System.Collections.Generic;

// Effect entities are map flags for spawners, synced decals, or other turf effects that can be interacted with, but not picked up, they do not update unless interacted with.
public class AbstractEffect : AbstractEntity
{
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
