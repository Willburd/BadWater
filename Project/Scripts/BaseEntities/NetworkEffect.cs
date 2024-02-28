using Godot;
using System;
using System.Collections.Generic;

// Effect entities are map flags for spawners, synced decals, or other turf effects that can be interacted with, but not picked up, they do not update unless interacted with.
[GlobalClass] 
public partial class NetworkEffect : NetworkEntity
{
    // Beginning of template data
    public override void TemplateClone(PackData data)
    {
        template_data = data;
        EffectData temp = template_data as EffectData;
        spawner_id = temp.spawner_id;
        cleanable = temp.cleanable;
    } 
    [Export]
    public string spawner_id = "";     // If used as a mob or item spawner by the map, gets loaded into a list of spawners as well as the list of effects!
    [Export]
    public bool cleanable = false;
    // End of template data

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
