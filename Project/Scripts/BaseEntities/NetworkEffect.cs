using Godot;
using System;
using System.Collections.Generic;

// Effect entities are map flags for spawners, synced decals, or other turf effects that can be interacted with, but not picked up, they do not update unless interacted with.
[GlobalClass] 
public partial class NetworkEffect : NetworkEntity
{
    // Beginning of template data
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
