using Godot;
using System;
using System.Collections.Generic;

// Turfs are map tiles that other entities move on. Turfs have a list of entities they currently contain.
[GlobalClass] 
public partial class NetworkChunk : NetworkEntity
{
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
    
    [Export]
    public TurfMeshUpdater mesh_updater;

    // AUTO UPDATED, This might be better moved to an RPC?
    public override void _Process(double delta)
    {
        mesh_updater.MeshUpdated(this);
    }
}
