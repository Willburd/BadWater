using Godot;
using System;
using System.Collections.Generic;

// Effect entities are map flags for spawners, synced decals, or other turf effects that can be interacted with, but not picked up, they do not update unless interacted with.
[GlobalClass] 
public partial class NetworkEffect : NetworkEntity
{
    [Export]
    public MeshUpdater mesh_updater;

    protected override void Internal_MeshUpdate()
    {
        Godot.Collections.Dictionary entity_data = new Godot.Collections.Dictionary
        {
            { "model", abstract_owner.model },
            { "texture", abstract_owner.texture },
            { "anim_speed", abstract_owner.anim_speed }
        };
        // Update json on other end.
        Rpc(nameof(ClientMeshUpdate), Position, Json.Stringify(entity_data));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = debug_visual)]
    private void ClientMeshUpdate(Vector3 pos, string mesh_json)
    {
        Position = pos;
        mesh_updater.MeshUpdated(mesh_json);
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
