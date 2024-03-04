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
    [Export]
    public EffectMeshUpdater mesh_updater;

    public override void MeshUpdate()
    {
        GD.Print("INTERNAL MESH UPDATE");
        Godot.Collections.Dictionary entity_data = new Godot.Collections.Dictionary
        {
            { "model", abstract_owner.model },
            { "texture", abstract_owner.texture },
            { "anim_speed", abstract_owner.anim_speed }
        };
        // Update json on other end.
        Rpc(nameof(ClientMeshUpdate),Json.Stringify(entity_data));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void ClientMeshUpdate(string mesh_json)
    {
        mesh_updater.MeshUpdated(mesh_json);
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
