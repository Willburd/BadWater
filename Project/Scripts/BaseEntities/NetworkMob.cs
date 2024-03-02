using Godot;
using System;
using System.Collections.Generic;

// Mob entities are living creatures, often with AI that interact with the map and other mobs. Even players are mobs.
[GlobalClass] 
public partial class NetworkMob : NetworkEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        PackRef = new PackRef(data);
        //MobData temp = AssetLoader.GetPackFromModID(PackRef) as MobData;
        //SetTag(temp.tag);
        //model = temp.model;
        //texture = temp.texture;
        //density = template_data.density;
        //opaque = template_data.opaque;
        //SetBehavior(Behavior.CreateBehavior(temp.behaviorID));
    }
    [Export]
    public bool density;                // blocks movement
    [Export]
    public bool opaque;               // blocks vision
    // End of template data
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }

    int health = 100;
    int hunger = 0;
}
