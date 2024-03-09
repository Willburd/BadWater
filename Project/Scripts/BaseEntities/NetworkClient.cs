using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

[GlobalClass]
public partial class NetworkClient : Node
{
    
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
        GD.Print("Client " + Name + " focused entity updated to " + ent);
        focused_entity = ent;
        focused_map_id = focused_entity.map_id_string;
        focused_position = focused_entity.GridPos.WorldPos();
        AccountController.UpdateAccount(this,ent);
        if(TOOLS.PeerConnected(this)) Rpc(nameof(UpdateClientFocusedPos),focused_map_id,focused_position);
    }
    public AbstractEntity GetFocusedEntity()
    {
        return focused_entity;
    }

    public void ClearFocusedEntity()
    {
        GD.Print(Name + " Client focused entity cleared");
        focused_entity = null;
        AccountController.UpdateAccount(this,null);
    }


    public bool has_logged_in = false;
    public string login_name = null;
    public string login_hash = null;

    public void RequestCredentials()
    {
        GD.Print("Request credentials");
        login_name = "";
        login_hash = "";
        Rpc(nameof(RespondCredentials));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)] // Tell the client we want to forcibly move this
    public void RespondCredentials()
    {
        if(!IsMultiplayerAuthority()) return;
        // DUMP TEMPORARY STUFF
        // TODO - Actual login
        string assign_name = ((TextEdit)GetTree().Root.GetChild(0).GetChild<CanvasLayer>(2).GetChild(6)).Text;
        string pass_hash = ((TextEdit)GetTree().Root.GetChild(0).GetChild<CanvasLayer>(2).GetChild(7)).Text;
        Rpc(nameof(AcknowledgeCredentials), Name, assign_name, pass_hash);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)] // Tell the client we want to forcibly move this
    public void AcknowledgeCredentials(string name_id, string assign_name, string pass_hash)
    {
        if(!Multiplayer.IsServer()) return; // Server only
        if(Name == name_id && !has_logged_in)
        {
            GD.Print("Received credentials " + assign_name);
            login_name = assign_name;
            login_hash = pass_hash;
            Init();
        }
    }

    public void Init()
    {
        GD.Print("Client init " + Name);
        // Check if valid, we can't login if this account is already online!
        if(!AccountController.CanJoin( login_name, login_hash)) 
        {
            // Can we join the game?
            DisconnectClient();
            return;
        }
        // Prep
        if(!AccountController.JoinGame(this, login_name, login_hash))
        {
            // Failed to join the game still..
            DisconnectClient();
            return;
        }
        // Assign tracking mob from account
        GD.Print("Successful account login " + login_name);
        has_logged_in = true;
        AbstractEntity foc = AccountController.GetClientEntity(this);
        if(foc == null) 
        {
            // TODO - properly handle first spawn!
            GD.Print("-No entity stored.");
            Spawn();
        }
        else
        {
            // logging back in from DC
            GD.Print("-Syncing to entity: " + foc);
            foc.SetClientOwner(this);
            SetFocusedEntity(foc);
        }
        // Client joins chunk controller
        ChunkController.NewClient(this);
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
                int rand = TOOLS.RandI(spawners.Count);
                SpawnHostEntity(spawners[rand].map_id_string,spawners[rand].GridPos);
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
    }

    private void SpawnHostEntity(string new_map, MapController.GridPos new_pos)
    {
        // SPAWN HOST OBJECT
        if(focused_entity == null) 
        {
            AbstractEntity new_ent = AbstractEffect.CreateEntity(new_map,"BASE:TEST",MainController.DataType.Mob);
            new_ent.SetClientOwner(this);
            SetFocusedEntity(new_ent);
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
        if(!has_logged_in) 
        {
            // Attempt login!
            if(login_name == null) // set in RequestCreds to "" then gets data from actual client!
            {
                // Request client info
                RequestCredentials(); 
            }
            return;
        }

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
        new_inputs["mod_control"]   = Input.IsActionPressed("mod_control");
        new_inputs["mod_alt"]       = Input.IsActionPressed("mod_alt");
        new_inputs["mod_shift"]     = Input.IsActionPressed("mod_shift");
        // Hotkeys
        new_inputs["walk"]     = new_inputs["mod_shift"];
        new_inputs["swap"]     = Input.IsActionPressed("game_swap");
        new_inputs["resist"]   = Input.IsActionPressed("game_resist");
        new_inputs["rest"]     = Input.IsActionPressed("game_rest");
        new_inputs["throw"]    = Input.IsActionPressed("game_throw");
        new_inputs["equip"]    = Input.IsActionPressed("game_equip");
        new_inputs["drop"]     = Input.IsActionPressed("game_drop");
        new_inputs["useheld"]      = Input.IsActionPressed("game_useheld");
        // Shifting input only triggers on taps, otherwise normal inputs
        new_inputs["x"] = 0;
        new_inputs["y"] = 0;
        if(!new_inputs["mod_control"].AsBool() || Input.IsActionJustPressed("game_left") || Input.IsActionJustPressed("game_right") || Input.IsActionJustPressed("game_up") || Input.IsActionJustPressed("game_down") )
        {
            new_inputs["x"] = Input.GetAxis("game_left","game_right");
            new_inputs["y"] = Input.GetAxis("game_up","game_down");
        }
        // Limit to only sending if we have useful input
        if(new_inputs["x"].AsDouble() != 0 
        || new_inputs["y"].AsDouble() != 0 
        || new_inputs["swap"].AsBool()
        || new_inputs["resist"].AsBool()
        || new_inputs["rest"].AsBool()
        || new_inputs["throw"].AsBool()
        || new_inputs["equip"].AsBool()
        || new_inputs["drop"].AsBool()
        || new_inputs["useheld"].AsBool())
        {
            Rpc(nameof(SetClientControl), Json.Stringify(new_inputs));
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!IsMultiplayerAuthority()) return;
        // Client only camera update
        camera.Current = true;
        float lerp_speed = Mathf.Lerp(2f,40f, Mathf.Max(0 , Mathf.InverseLerp(-1,22,TOOLS.VecDist(camera.Position,focused_position) )));
        camera.Position = camera.Position.MoveToward(focused_position + new Vector3(0f,zoom_level * MainController.max_zoom,0.3f), (float)delta * lerp_speed);
        camera.LookAt(new Vector3(camera.Position.X,focused_position.Y,camera.Position.Z-(float)0.1));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    private void SetClientControl(string control_data)
    {
        client_input_data = (Godot.Collections.Dictionary)Json.ParseString(control_data);
    }


    public void DisconnectClient()
    {
        // Server handling client DC
        AccountController.ClientLeave(this);
        focused_entity?.ClearClientOwner();
        MainController.controller.Multiplayer.MultiplayerPeer.DisconnectPeer(int.Parse(Name)); // Calls Kill() remotely
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    private void UpdateClientMobVisuals(string control_data)
    {
        visual_state = (Godot.Collections.Dictionary)Json.ParseString(control_data);
        bool blind = TOOLS.ApplyExistingTag(visual_state,"blind",false);
        float white_fade = TOOLS.ApplyExistingTag(visual_state,"white_fade",0);
        float black_fade = TOOLS.ApplyExistingTag(visual_state,"black_fade",0);
    }




    public void PlaySoundAt(string path, Vector3 pos, float range)
    {
        if(!has_logged_in) return;
        if(TOOLS.PeerConnected(this)) Rpc(nameof(ClientPlayAudio),path, pos, range);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    public void ClientPlayAudio(string path, Vector3 pos, float range)
    {
        SoundPlayer newsound = GD.Load<PackedScene>("res://Scenes/SoundPlayer.tscn").Instantiate() as SoundPlayer;
        if(range <= -1)
        {
            newsound.player_global = new AudioStreamPlayer();
            newsound.AddChild(newsound.player_global);
        }
        else
        {
            newsound.player_pos = new AudioStreamPlayer3D();
            newsound.AddChild(newsound.player_pos);
            newsound.player_pos.UnitSize = range * 0.60f; // Gestimate for byond like sound range
        }
        newsound.path = path;
        newsound.Position = pos;
        GetTree().Root.AddChild(newsound);
    }
}
