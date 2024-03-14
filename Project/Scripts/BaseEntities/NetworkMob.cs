using Godot;
using System;
using System.Collections.Generic;

// Mob entities are living creatures, often with AI that interact with the map and other mobs. Even players are mobs.
[GlobalClass] 
public partial class NetworkMob : NetworkEntity
{
    protected override void Internal_MeshUpdate()
    {
        Godot.Collections.Dictionary entity_data = new Godot.Collections.Dictionary
        {
            { "model", abstract_owner.model },
            { "texture", abstract_owner.texture },
            { "anim_speed", abstract_owner.anim_speed },
            { "state", abstract_owner.icon_state }
        };
        // Update json on other end.
        Rpc(nameof(ClientMeshUpdate), Position, Json.Stringify(entity_data));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = debug_visual, TransferChannel = (int)MainController.RPCTransferChannels.VisualUpdate)]
    private void ClientMeshUpdate(Vector3 pos, string mesh_json)
    {
        Godot.Collections.Dictionary turf_data = TOOLS.ParseJson(mesh_json);
        // Get new model
        if(mesh_updater != null) mesh_updater.QueueFree();
        mesh_updater = MeshUpdater.GetModelScene(turf_data);
        mesh_updater.Visible = false;
        AddChild(mesh_updater);
        // Init model textures
        if(mesh_updater == null)
        {
            GD.Print("No updater for " + turf_data["model"]);
            return;
        }
        mesh_updater.TextureUpdated(mesh_json);
        mesh_updater.Visible = true;
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
