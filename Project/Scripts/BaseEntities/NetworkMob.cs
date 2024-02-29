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
        template_data = data;
        //density = template_data.density;
        //opaque = template_data.opaque;
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
