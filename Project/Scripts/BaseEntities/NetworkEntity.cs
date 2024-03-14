using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

[GlobalClass] 
public partial class NetworkEntity : Node3D
{
    protected AbstractEntity abstract_owner;
    public const bool debug_visual = false; // if server gets visual updates

    public static NetworkEntity CreateEntity(AbstractEntity abs, string map_id, MainController.DataType type)
    {
        NetworkEntity newEnt = null;
        switch(type)
        {
            case MainController.DataType.Chunk:
                newEnt = GD.Load<PackedScene>("res://Prefabs/NetworkChunk.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Effect:
                newEnt = GD.Load<PackedScene>("res://Prefabs/NetworkEffect.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Item:
                newEnt = GD.Load<PackedScene>("res://Prefabs/NetworkItem.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Structure:
                newEnt = GD.Load<PackedScene>("res://Prefabs/NetworkStructure.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Machine:
                newEnt = GD.Load<PackedScene>("res://Prefabs/NetworkMachine.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Mob:
                newEnt = GD.Load<PackedScene>("res://Prefabs/NetworkMob.tscn").Instantiate() as NetworkEntity;
                break;
        }
        // NetworkEntity init
        newEnt.abstract_owner = abs;
        newEnt.map_id_string = map_id;
        // Add to active network entities list
        MainController.controller.entity_container.AddChild(newEnt,true);
        return newEnt;
    }

    /*****************************************************************
     * Network entities just follow their abstract owner around...
     ****************************************************************/
    [Export]
    public string map_id_string;
    [Export]
    public Vector3 velocity = Vector3.Zero;
    [Export]
    public DAT.Dir direction = DAT.Dir.South;

    public void Kill()
    {
        QueueFree();
    }
    
    [Export]
    public MeshUpdater mesh_updater;

    public virtual void MeshUpdate()
    {
        // Normally just directly updated it, but some objects are multimeshes...
        Internal_MeshUpdate();
    }
    protected virtual void Internal_MeshUpdate()
    {
        // Override for mesh behaviors... Then call a unique RPC to that network entity type.
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }

    public void ClickPressed(Vector3 position,StaticBody3D collider)
    {
        Vector2 texspace = ColliderUVSpace(position,collider);
        if(CheckTexturePressed(mesh_updater,texspace.X,texspace.Y)) 
        {
            NetworkClient.peer_active_client.drag_start_location = position;
            NetworkClient.peer_active_client.holding_entity = this;
        }
    }

    public void ClickReleased(Vector3 position,StaticBody3D collider)
    {
        Godot.Collections.Dictionary new_inputs = new Godot.Collections.Dictionary();
        new_inputs["mod_control"]   = Input.IsActionPressed("mod_control");
        new_inputs["mod_alt"]       = Input.IsActionPressed("mod_alt");
        new_inputs["mod_shift"]     = Input.IsActionPressed("mod_shift");
        if(TOOLS.PeerConnected(NetworkClient.peer_active_client) && NetworkClient.peer_active_client.holding_entity != null) 
        {
            Vector2 texspace = ColliderUVSpace(position,collider);
            if(TOOLS.VecDist(NetworkClient.peer_active_client.drag_start_location,position) < 0.9f)
            {
                Rpc(nameof(PressFromClient), int.Parse(NetworkClient.peer_active_client.Name), new_inputs);
            }
            else
            {
                NetworkClient.peer_active_client.holding_entity.Rpc(nameof(DragFromClient), int.Parse(NetworkClient.peer_active_client.Name), this, new_inputs);
            }
        }
        NetworkClient.peer_active_client.holding_entity = null;
        NetworkClient.peer_active_client.drag_start_location = Vector3.Zero;
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    public void PressFromClient(int clientid, Godot.Collections.Dictionary parameters_json)
    {
        if(abstract_owner == null) return; // How?
        if(!Multiplayer.IsServer()) return; // SERVER ONLY
        NetworkClient client = null;
        foreach(Node node in MainController.ClientContainer.GetChildren())
        {
            if(node is NetworkClient cli)
            {
                if(cli.Name == clientid.ToString()) client = cli;
            }
        }
        abstract_owner.Click(((NetworkClient)client).GetFocusedEntity(),parameters_json);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    public void DragFromClient(int clientid, NetworkEntity dragTarget, Godot.Collections.Dictionary parameters_json)
    {
        if(abstract_owner == null) return; // How?
        if(dragTarget == null || dragTarget.abstract_owner == null) // Nope!
        if(!Multiplayer.IsServer()) return; // SERVER ONLY
        NetworkClient client = (NetworkClient)MainController.ClientContainer.FindChild(clientid.ToString());
        abstract_owner.Drag(client.GetFocusedEntity(),dragTarget.abstract_owner,parameters_json);
    }

    public static Vector2 ColliderUVSpace(Vector3 position,StaticBody3D collider)
    {
        // Godot, please explain to me why you don't have the ability to get texcoord from a click off a mesh...
        // Massively crippling for anyone doing pixel detection or mesh painting..
        position = collider.ToLocal(position);
        position *= collider.Quaternion.Inverse();
        float meshu = Mathf.InverseLerp(-1f,1f,position.X);
        float meshv = Mathf.InverseLerp(-1f,1f,position.Z);
        return new Vector2(meshu,meshv);
    }
    
    public static bool CheckTexturePressed(MeshUpdater mesh_updater, float meshu, float meshv)
    {
        // Check texture information
        AssetLoader.LoadedTexture tex_data = mesh_updater.CachedTextureData;
        float ux = tex_data.u + (tex_data.width * meshu);
        float vy = tex_data.v + (tex_data.height * meshv);
        Color col = AssetLoader.texture_pages[tex_data.tex_page].GetPixel(Mathf.FloorToInt(ux),Mathf.FloorToInt(vy));
        return col.A > 0.01f;
    }
}
