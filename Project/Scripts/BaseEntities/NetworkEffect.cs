using Godot;
using System;
using System.Collections.Generic;

// Effect entities are map flags for spawners, synced decals, or other turf effects that can be interacted with, but not picked up, they do not update unless interacted with.
[GlobalClass] 
public partial class NetworkEffect : NetworkEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        PackRef = new PackRef(data);
        EffectData temp = AssetLoader.GetPackFromModID(PackRef) as EffectData;
        SetTag(temp.tag);
        model = temp.model;
        texture = temp.texture;
        is_spawner = temp.is_spawner;
        cleanable = temp.cleanable;
        SetBehavior(Behavior.CreateBehavior(temp));
    } 
    [Export]
    public bool is_spawner = false; // Uses tag as ID
    [Export]
    public bool cleanable = false;
    // End of template data

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
