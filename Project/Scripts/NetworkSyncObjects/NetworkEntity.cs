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

    public void DeleteEntity()
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

    public void ClickPressed(Vector3 pos)
    {
        if(!TOOLS.PeerConnected(NetworkClient.peer_active_client)) return;
        Godot.Collections.Dictionary new_inputs = TOOLS.AssembleStandardClick(pos);
        Rpc(nameof(ClientUpdateClickedEntity),int.Parse(NetworkClient.peer_active_client.Name),Json.Stringify(new_inputs));
    }

    public void ClickReleased(Vector3 pos)
    {
        if(!TOOLS.PeerConnected(NetworkClient.peer_active_client)) return;
        Godot.Collections.Dictionary new_inputs = TOOLS.AssembleStandardClick(pos);
        Rpc(nameof(ClientUpdateReleasedEntity),int.Parse(NetworkClient.peer_active_client.Name),Json.Stringify(new_inputs));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    public void ClientUpdateClickedEntity(int clientID,string parameters_json)
    {
        if(!Multiplayer.IsServer()) return; // Server only
        foreach(NetworkClient client in MainController.ClientList)
        {
            if(client.Name == clientID.ToString())
            {
                client.ClickEntityStart(abstract_owner,parameters_json);
                break;
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    public void ClientUpdateReleasedEntity(int clientID,string parameters_json)
    {
        if(!Multiplayer.IsServer()) return; // Server only
        foreach(NetworkClient client in MainController.ClientList)
        {
            if(client.Name == clientID.ToString())
            {
                client.ClickEntityEnd(abstract_owner,parameters_json);
                break;
            }
        }
    }
}
