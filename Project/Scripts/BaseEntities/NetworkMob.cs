using Godot;
using System;
using System.Collections.Generic;

// Mob entities are living creatures, often with AI that interact with the map and other mobs. Even players are mobs.
[GlobalClass] 
public partial class NetworkMob : NetworkEntity
{
    // Beginning of template data

    // End of template data

    public void Sync(AbstractItem abs)
    {
        // sync data
        base.Sync(abs);
    }
    
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }

    int health = 100;
    int hunger = 0;
}
