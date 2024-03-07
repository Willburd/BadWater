using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

[GlobalClass]
public partial class NetworkClient : Node
{
    public static List<NetworkClient> clients = new List<NetworkClient>();
    
    public int PeerID              // Used by main controller to know that all controllers are ready for first game tick
    {
        get 
        { 
            int x;
            if(Int32.TryParse(Name, out x)) return x;
            return 1;
        }
    }

    [Export]
    public string focused_map_id;
    private string sync_map_id;
    [Export]
    public Vector3 focused_position;
    private Vector3 sync_position;
    private float zoom_level = 1f;


    private Godot.Collections.Dictionary client_input_data = new Godot.Collections.Dictionary();    // current inputs from client
    private Godot.Collections.Dictionary visual_state = new Godot.Collections.Dictionary();         // The visual effects from status conditions
    private AbstractEntity focused_entity;

    [Export]
    public Camera3D camera;

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(PeerID);
    }

    public void SetFocusedEntity(AbstractEntity ent)
    {
        focused_entity = ent;
        focused_map_id = focused_entity.map_id_string;
        focused_position = focused_entity.GridPos.WorldPos();
        if(TOOLS.PeerConnected(this)) Rpc(nameof(UpdateClientFocusedPos),focused_map_id,focused_position);
        AccountController.UpdateAccount(this);
    }
    public AbstractEntity GetFocusedEntity()
    {
        return focused_entity;
    }

    public void ClearFocusedEntity()
    {
        focused_entity = null;
        AccountController.UpdateAccount(this);
    }

    public void Init(string assign_name, string pass_hash)
    {
        // Check if valid, we can't login if this account is already online!
        clients.Add(this);
        if(!AccountController.CanJoin(assign_name,pass_hash)) 
        {
            // Can we join the game?
            GD.Print(Name + " Could not join as " + assign_name + " already active client");
            Kill();
            return;
        }
        // Prep
        if(!AccountController.JoinGame(this, assign_name, pass_hash))
        {
            // Failed to join the game still..
            GD.Print(Name + " Could not join as " + assign_name + " failed to join");
            Kill();
            return;
        }
        // Add to active network clients list
        MainController.controller.client_container.AddChild(this,true);
    }

    public void Spawn()
    {
        camera.Current = false;
        // Check for a spawner!
        if(MapController.spawners.ContainsKey("PLAYER"))
        {
            // TEMP, pick random instead?
            List<AbstractEffect> spawners = MapController.spawners["PLAYER"];
            if(spawners.Count > 0)
            {
                GD.Print("Client RESPAWN: " + Name);
                int rand = Mathf.Abs((int)GD.Randi() % spawners.Count);
                SpawnHostEntity(spawners[rand].map_id_string,spawners[rand].GridPos);
                ChunkController.NewClient(this);
                return;
            }
            else
            {
                GD.Print("-NO SPAWNERS!");
            }
        }
        // EMERGENCY FALLBACK TO 0,0,0 on first map loaded!
        GD.Print("Client FALLBACK RESPAWN: " + Name);
        SpawnHostEntity(MapController.FallbackMap(),new MapController.GridPos((float)0.5,(float)0.5,0));
        ChunkController.NewClient(this);
    }

    private void SpawnHostEntity(string new_map, MapController.GridPos new_pos)
    {
        // SPAWN HOST OBJECT
        if(focused_entity == null) 
        {
            AbstractEntity new_ent = AbstractEffect.CreateEntity(new_map,"BASE:TEST",MainController.DataType.Mob);
            new_ent.SetClientOwner(this);
            new_ent.Move(new_map,new_pos,false);
        }
        // Inform client of movment from server
        if(TOOLS.PeerConnected(this)) Rpc(nameof(UpdateClientFocusedPos),new_map,TOOLS.GridToPosWithOffset(new_pos));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferChannel = (int)MainController.RPCTransferChannels.Movement)] // Tell the client we want to forcibly move this
    public virtual void UpdateClientFocusedPos(string map_id, Vector3 new_pos)
    {
        focused_map_id = map_id;
        focused_position = new_pos;
        sync_map_id = focused_map_id;
        sync_position = focused_position;
    }

    public void Tick()
    {
        if(TOOLS.PeerConnecting(this)) return;

        // Process client inputs
        UpdateClientControl();
        Godot.Collections.Dictionary new_visual_state = new Godot.Collections.Dictionary();
        if(focused_entity != null)
        {
            // handle camera movement
            focused_map_id = focused_entity.map_id_string;
            focused_position = focused_entity.GridPos.WorldPos();
            if(sync_map_id != focused_map_id || sync_position != focused_position)
            {
                if(TOOLS.PeerConnected(this)) Rpc(nameof(UpdateClientFocusedPos),focused_map_id,focused_position);
            }
        }

        // One of the few things we actually update rather regularly... Visual hud update stuff
        if(TOOLS.PeerConnected(this)) Rpc(nameof(UpdateClientMobVisuals), Json.Stringify(new_visual_state));
    }

    private void UpdateClientControl()
    {
        focused_entity?.ControlUpdate(client_input_data);
        client_input_data = new Godot.Collections.Dictionary();
    }

    public override void _Process(double delta)
    {
        if(!IsMultiplayerAuthority()) return;
        // Get client inputs!
        Godot.Collections.Dictionary new_inputs = new Godot.Collections.Dictionary();
        new_inputs["x"] = Input.GetAxis("ui_left","ui_right") / 10;
        new_inputs["y"] = Input.GetAxis("ui_up","ui_down") / 10;
        // Limit to only sending if we have useful input
        if(new_inputs["x"].AsDouble() != 0 || new_inputs["y"].AsDouble() != 0)
        {
            Rpc(nameof(SetClientControl), Json.Stringify(new_inputs));
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!IsMultiplayerAuthority()) return;
        // Client only camera update
        camera.Current = true;
        camera.Position = camera.Position.MoveToward(focused_position + new Vector3(0f,zoom_level * MainController.max_zoom,0.3f), (float)delta * 10f);
        camera.LookAt(new Vector3(camera.Position.X,focused_position.Y,camera.Position.Z-(float)0.1));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    private void SetClientControl(string control_data)
    {
        client_input_data = (Godot.Collections.Dictionary)Json.ParseString(control_data);
    }

    public void Kill()
    {
        if(!IsMultiplayerAuthority())
        {
            // Server handling client DC
            AccountController.ClientLeave(this);
        }
        clients.Remove(this);
        QueueFree();
    }

    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    private void UpdateClientMobVisuals(string control_data)
    {
        visual_state = (Godot.Collections.Dictionary)Json.ParseString(control_data);
        bool blind = TOOLS.ApplyExistingTag(visual_state,"blind",false);
        float white_fade = TOOLS.ApplyExistingTag(visual_state,"white_fade",0);
        float black_fade = TOOLS.ApplyExistingTag(visual_state,"black_fade",0);
    }
}
