using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

// Turfs are map tiles that other entities move on. Turfs have a list of entities they currently contain.
[GlobalClass] 
public partial class NetworkChunk : NetworkEntity
{
    public int timer = 0;
    public bool do_not_unload = false;

    [Export]
    public bool has_mesh_update = true;
    [Export]
    public TurfMeshUpdater mesh_updater;

    public void Tick()
    {
        timer += 1;
    }

    public override void _Process(double delta)
    {
        if(has_mesh_update) mesh_updater.MeshUpdated(this);
        has_mesh_update = false;
    }

    public bool Unload()
    {
        // handles unique situations where a chunk shouldn't unload just yet...
        if(!do_not_unload)
        {
            Kill();
            return true;
        }
        return false;
    }
    
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
