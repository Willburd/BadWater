using Behaviors_BASE;
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

    public static NetworkEntity CreateEntity(AbstractEntity abs, MainController.DataType type, string map_id)
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
        if(type != MainController.DataType.Chunk)
        {
            newEnt.clickable = abs.display_name.Length > 0; // If no name, no click
        }
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
    [Export]
    public bool clickable = false;

    // delete needs to be handled remotely
    public void DeleteEntity()
    {
        abstract_owner = null;
        mesh_updater?.QueueFree();
        QueueFree();
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }


    /*****************************************************************
     * Drawn mesh handler
     ****************************************************************/
    [Export]
    public MeshUpdater mesh_updater;
    public virtual void MeshUpdate()
    {
        // Normally just directly updated it, but some objects are multimeshes...
        Internal_MeshUpdate();
    }
    protected virtual void Internal_MeshUpdate()
    {
        // Override for mesh behaviors...
        Godot.Collections.Dictionary entity_data = new Godot.Collections.Dictionary
        {
            { "model", abstract_owner.model },
            { "texture", abstract_owner.texture },
            { "anim_speed", abstract_owner.anim_speed }
        };
        if(abstract_owner is AbstractSimpleMob abs_mob)
        {
            entity_data["state"] = abstract_owner.icon_state == "" ? abs_mob.Stat.ToString() : abstract_owner.icon_state;
        }
        else
        {
            entity_data["state"] = abstract_owner.icon_state == "" ? "Idle" : abstract_owner.icon_state;
        }
        // Update json on other end.
        Rpc(nameof(ClientMeshUpdate), Json.Stringify(entity_data));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = debug_visual, TransferChannel = (int)MainController.RPCTransferChannels.VisualUpdate)]
    protected virtual void ClientMeshUpdate( string mesh_json)
    {
        Godot.Collections.Dictionary data = TOOLS.ParseJson(mesh_json);
        // Get new model
        mesh_updater?.Free();
        mesh_updater = MeshUpdater.GetModelScene(data);
        mesh_updater.Visible = false;
        AddChild(mesh_updater);
        // Init model textures
        if(mesh_updater == null) 
        {
            GD.Print("No model for " + data["model"]);
            return;
        }
        mesh_updater.TextureUpdated(mesh_json);
        mesh_updater.Visible = true;
    }


    /*****************************************************************
     * Movement and interpolation handling
     ****************************************************************/
    struct MoveStep
    {
        public MoveStep(Vector3 getpos, ulong getstep)
        {
            pos = getpos;
            step = getstep;
        }
        public Vector3 pos;
        public ulong step;
    }
    public void SetUpdatedPosition() // Force an update based on the current serverside position it's already at... So basically what's done on client join.
    {
        Rpc(nameof(ClientUpdatePosition),Position,true);
    }
    public void SetUpdatedPosition(Vector3 pos, bool forced)
    {
        Position = pos;
        Rpc(nameof(ClientUpdatePosition),Position,forced);
    }
    private List<MoveStep> movement_steps = new List<MoveStep>();

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    public void ClientUpdatePosition(Vector3 new_pos, bool force)
    {
        if(movement_steps.Count == 0 || force)
        {
            // teleport if first movement queue
            movement_steps.Clear();
            movement_steps.Add(new MoveStep(new_pos,Time.GetTicksUsec()));
            movement_steps.Add(new MoveStep(new_pos,Time.GetTicksUsec()));
            Position = movement_steps[0].pos + animation_offset;
        }
        movement_steps.Add(new MoveStep(new_pos,Time.GetTicksUsec()));
    }
    public override void _PhysicsProcess(double delta)
    {
        ulong render_time = Time.GetTicksUsec();
        if(movement_steps.Count > 1)
        {
            if(IsAnimationPlaying)
            {
                // Process returns true if finished
                if(animation_loaded.Process(delta)) ResetAnimationVars();
                mesh_updater.SetAnimationVars(animation_alpha);
            }
            while(movement_steps.Count > 2 && render_time > movement_steps[1].step) movement_steps.RemoveAt(0);
            if( movement_steps[0].pos == movement_steps[1].pos)
            {
                Position = movement_steps[0].pos + animation_offset; // Already there!
            }
            else
            {
                // Interpolate!
                float interpo = (render_time - movement_steps[0].step) / (movement_steps[1].step - movement_steps[0].step);
                Position = movement_steps[0].pos.Lerp(movement_steps[1].pos,Mathf.Min(interpo,1f)) + animation_offset;
            }
        }
        
    }


    /*****************************************************************
     * Animation handling
     ****************************************************************/
    private NetwornAnimations.Animation animation_loaded;
    public bool IsAnimationPlaying
    {
        get {return animation_loaded != null;}
    }
    public void AnimationRequest(NetwornAnimations.Animation.ID anim, Vector3 dir_vec, int length = 0)
    {
        abstract_owner.SetAnimationLock( NetwornAnimations.Animation.LookupAnimationLock(anim), length > 0 ? length : NetwornAnimations.Animation.LookupAnimationLength(anim));
        Rpc(nameof(AnimationHandle),(int)anim,dir_vec);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    public void AnimationHandle(int anim_in, Vector3 dir_vec)
    {
        if(Multiplayer.IsServer()) return; // Client only
        if(IsAnimationPlaying) animation_loaded = null; // Force cleanout, and apply new animation, for stuff like pounces that windup and let rip from their current offset!
        animation_loaded = NetwornAnimations.Animation.PlayAnimation(this,(NetwornAnimations.Animation.ID)anim_in,animation_offset,animation_alpha,dir_vec);
    }

    // animator!
    float animation_alpha = 1f;
    Vector3 animation_offset = Vector3.Zero;
    private void ResetAnimationVars()
    {
        animation_offset = Vector3.Zero;
        animation_alpha = 1f;
        animation_loaded = null;
    }
    public void SetAnimationVars(Vector3 offset, float alpha)
    {
        if(animation_loaded == null) return;
        animation_offset = offset;
        animation_alpha = alpha;
    }

    /*****************************************************************
     * Click handling
     ****************************************************************/
    public void ClickPressed(Vector3 pos, MouseButton button)
    {
        if(!TOOLS.PeerConnected(NetworkClient.peer_active_client)) return;
        if(!clickable) return;
        Godot.Collections.Dictionary new_inputs = TOOLS.AssembleStandardClick(pos);
        new_inputs["button"] = (int)button;
        new_inputs["state"] = true;
        Rpc(nameof(ClientUpdateClickedEntity),int.Parse(NetworkClient.peer_active_client.Name),Json.Stringify(new_inputs));
    }

    public void ClickReleased(Vector3 pos, MouseButton button)
    {
        if(!TOOLS.PeerConnected(NetworkClient.peer_active_client)) return;
        if(!clickable) return;
        Godot.Collections.Dictionary new_inputs = TOOLS.AssembleStandardClick(pos);
        new_inputs["button"] = (int)button;
        new_inputs["state"] = false;
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
