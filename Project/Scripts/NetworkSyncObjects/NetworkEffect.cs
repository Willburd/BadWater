using Godot;
using System;
using System.Collections.Generic;

// Effect entities are map flags for spawners, synced decals, or other turf effects that can be interacted with, but not picked up, they do not update unless interacted with.
[GlobalClass] 
public partial class NetworkEffect : NetworkEntity
{
    [Export]
    public string synced_text = ""; // Set from server in AbstractEntity.UpdateCustomNetworkData, synced to client

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
