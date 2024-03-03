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

    public override void TemplateRead(PackData data)
    {
        // nothing here, nope!
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
    
    [Export]
    public TurfMeshUpdater mesh_updater;

    // AUTO UPDATED, This might be better moved to an RPC?
    public new void Tick()
    {
        // Replace behavioral calls from parent, this does it's own stuff!
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
}
