using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

[GlobalClass]
public partial class NetworkClient : Node3D
{
    public static NetworkClient peer_active_client;
    public int PeerID              // Used by main controller to know that all controllers are ready for first game tick
    {
        get 
        { 
            int x;
            if(Int32.TryParse(Name, out x)) return x;
            return 1;
        }
    }
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(PeerID);
    }


    /*****************************************************************
     * Login and credential requests
     ****************************************************************/
    public bool has_logged_in = false;
    public string login_name = null;
    public string login_hash = null;

    public void RequestCredentials()
    {
        GD.Print("Request credentials");
        login_name = "";
        login_hash = "";
        Rpc(nameof(RespondCredentials),int.Parse(Name));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)] // Tell the client we want to forcibly move this
    public void RespondCredentials(int peerid)
    {
        if(!IsMultiplayerAuthority()) return; // Only on a client, and...
        if(Name != peerid.ToString()) return; // Only the client we're asking!
        // DUMP TEMPORARY STUFF
        // TODO - Actual login =================================================================================================================================
        string assign_name = BootController.controller.account_entry.Text;
        string pass_hash = BootController.controller.accpass_entry.Text;
        peer_active_client = this; // Set the client reference for clicks!
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


    /*****************************************************************
     * Currently focused entity of the client... It will follow this around.
     ****************************************************************/
    private AbstractEntity focused_entity;
    public Vector3 focused_position;
    public string focused_map_id;
    private Vector3 sync_position;
    private string sync_map_id;
    private Godot.Collections.Dictionary visual_state = new Godot.Collections.Dictionary();         // The visual effects from status conditions

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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferChannel = (int)MainController.RPCTransferChannels.Movement)] // Tell the client we want to forcibly move this
    public virtual void UpdateClientFocusedPos(string map_id, Vector3 new_pos)
    {
        focused_map_id = map_id;
        focused_position = new_pos;
        sync_map_id = focused_map_id;
        sync_position = focused_position;
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    private void SetClientControl(string control_data)
    {
        client_input_data = (Godot.Collections.Dictionary)Json.ParseString(control_data);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    private void UpdateClientMobVisuals(string control_data)
    {
        visual_state = (Godot.Collections.Dictionary)Json.ParseString(control_data);
        bool blind = TOOLS.ApplyExistingTag(visual_state,"blind",false);
        float white_fade = TOOLS.ApplyExistingTag(visual_state,"white_fade",0);
        float black_fade = TOOLS.ApplyExistingTag(visual_state,"black_fade",0);
    }


    /*****************************************************************
     * Client processing and input handling
     ****************************************************************/
    private Godot.Collections.Dictionary client_input_data = new Godot.Collections.Dictionary();    // current inputs from client
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
        // Only update focused entity if we have actual inputs to give
        if(client_input_data.Keys.Count > 0 && focused_entity != null) 
        {
            DAT.Dir old_dir = focused_entity.direction;
            focused_entity.ControlUpdate(client_input_data);
            focused_entity.UpdateNetworkDirection(old_dir);
        }
        client_input_data = new Godot.Collections.Dictionary();
    }
    public override void _Process(double delta)
    {
        if(!TOOLS.PeerConnected(this)) return;
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
            // perform camera transform 
            Vector2 tf_vec = new Vector2(Input.GetAxis("game_left","game_right"),Input.GetAxis("game_up","game_down"));
            tf_vec = tf_vec.Rotated(CamRotationVector2().Angle() - 1.5708f); // -90 degrees, but in radians
            tf_vec.Normalized();
            // assign actual values
            new_inputs["x"] = tf_vec.X;
            new_inputs["y"] = tf_vec.Y;
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
        // Unzoom and zoom
        if(Input.IsActionJustPressed("game_zoom"))
        {
            zoom_level -= 0.05f;
            if(zoom_level < 0) zoom_level = 0f;
        }
        else if(Input.IsActionJustPressed("game_unzoom"))
        {
            zoom_level += 0.05f;
            if(zoom_level > 1) zoom_level = 1f;
        }
        // camera stepped movements
        float cam_steps = (Mathf.Pi / 4);
        if(Input.IsActionJustPressed("game_camstepright"))
        {
            view_rotation = Mathf.Round(view_rotation / cam_steps) * cam_steps;
            view_rotation += cam_steps;
        }
        if(Input.IsActionJustPressed("game_camstepleft"))
        {
            view_rotation = Mathf.Round(view_rotation / cam_steps) * cam_steps;
            view_rotation -= cam_steps;
        }
    }


    /*****************************************************************
     * Client click handling
     ****************************************************************/
    private AbstractEntity current_click_held_entity;
    private Vector3 current_click_start_pos;

    public override void _UnhandledInput(InputEvent @event)
    {
        if(!IsMultiplayerAuthority()) return;
        // Client only input handling
        if(@event is InputEventMouseMotion mouse_move)
        {
            if(mouse_move.Velocity.Length() > 0)
            {
                if(Input.IsActionPressed("game_camcontrol"))
                {
                    float velocity = mouse_move.Velocity.X / 15000f;
                    view_rotation -= velocity;
                    view_rotation %= Mathf.Pi * 2;
                }
            }
        }
        if(@event is InputEventMouseButton mouse_button)
        {
            // Only handle LR presses
            if(mouse_button.ButtonIndex != MouseButton.Left && mouse_button.ButtonIndex != MouseButton.Right) return;

            // Raycasting time! We want to handle a raycast all on our own, because godot has some brain damage in its input system, and this is too hard for it.
            Camera3D current_camera = GetViewport().GetCamera3D();
            Vector3 from = current_camera.ProjectRayOrigin(mouse_button.Position);
            float dist = 100f;
            Vector3 raynormal = current_camera.ProjectRayNormal(mouse_button.Position);
            Vector3 to = from + (raynormal * dist);
            // Scan all colliding bodies on a raycast!
            SortedList<float,StaticBody3D> dist_list = new SortedList<float, StaticBody3D>();
            Godot.Collections.Array<Rid> already_processed = new Godot.Collections.Array<Rid>();
            Godot.Collections.Dictionary raycast_hit = GetWorld3D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(from, to));
            while(raycast_hit.Count > 0)
            {
                if(raycast_hit["collider"].AsGodotObject() is StaticBody3D static_body)
                {
                    dist_list.Add(TOOLS.VecDist(from,raycast_hit["position"].AsVector3()), static_body);
                }
                already_processed.Add(raycast_hit["rid"].AsRid()); // Prevent endless loops
                raycast_hit = GetWorld3D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters3D.Create( from, to, exclude:already_processed));
            }
            // Now that they're sorted by distance, attempt from nearest to furthest to interact!
            foreach(var dat in dist_list)
            {
                if(mouse_button.ButtonIndex == MouseButton.Left)
                {
                    if(dat.Value.GetParent().GetParent() is MeshUpdater mesh_handler)
                    {
                        if(mesh_handler.ClickInput(current_camera, @event,from + (raynormal * dat.Key),dat.Value)) break;
                    }
                }
                // Make rightclicking always get the TURF's content list!
                if(dat.Value.GetParent() is TurfClickHandler turf_handler)
                {
                    if(turf_handler.ClickInput(current_camera, @event,from + (raynormal * dat.Key),dat.Value)) break;
                }
            }
        }
    }

    // Clientside catch for turfplane click!
    public void ClientTurfClick(string parameters_json)
    {
        if(TOOLS.PeerConnected(this)) Rpc(nameof(ClickTurf), parameters_json);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    private void ClickTurf(string parameters_json)
    {
        if(!Multiplayer.IsServer()) return; // Server only
        Godot.Collections.Dictionary client_click_data = TOOLS.ParseJson(parameters_json);
        if(client_click_data.Keys.Count == 0) return;

        if(client_click_data["button"].AsInt32() == (int)MouseButton.Left)
        {
            if(!client_click_data["state"].AsBool()) // Only handle turfclick on release of the button to CONFIRM it...
            {
                AbstractTurf turf = MapController.GetTurfAtPosition(focused_map_id,new MapController.GridPos((float)client_click_data["x"].AsDouble(),(float)client_click_data["z"].AsDouble(),(float)client_click_data["y"].AsDouble()),true);
                if(current_click_held_entity != null)
                {
                    current_click_held_entity.Dragged( focused_entity, turf, client_click_data);
                    current_click_held_entity = null;
                    current_click_start_pos = Vector3.Zero;
                }
                else if(turf != null && focused_entity != null)
                {
                    DAT.Dir old_dir = focused_entity.direction;
                    focused_entity?.Clicked( focused_entity?.ActiveHand, turf,client_click_data);
                    focused_entity.UpdateNetworkDirection(old_dir);
                }
            }
        }

        if(client_click_data["button"].AsInt32() == (int)MouseButton.Right)
        {
            if(client_click_data["state"].AsBool()) // Creates a menu, so do this instantly!
            {
                AbstractTurf turf = MapController.GetTurfAtPosition(focused_map_id,new MapController.GridPos((float)client_click_data["x"].AsDouble(),(float)client_click_data["z"].AsDouble(),(float)client_click_data["y"].AsDouble()),true);
                // Create a list of entities on the tile that we can click, including the turf!
                if(turf == null) return;
                foreach(AbstractEntity ent in turf.Contents)
                {
                    GD.Print(ent.display_name);
                }
                GD.Print(turf.display_name);
            }
        }
    }
    public void ClickEntityStart(AbstractEntity ent,string parameters_json)
    {
        Godot.Collections.Dictionary client_click_data = TOOLS.ParseJson(parameters_json);
        current_click_held_entity = ent;
        current_click_start_pos = new Vector3((float)client_click_data["x"].AsDouble(),(float)client_click_data["y"].AsDouble(),(float)client_click_data["z"].AsDouble());
    }
    public void ClickEntityEnd(AbstractEntity ent,string parameters_json)
    {
        Godot.Collections.Dictionary client_click_data = TOOLS.ParseJson(parameters_json);
        Vector3 release_pos = new Vector3((float)client_click_data["x"].AsDouble(),(float)client_click_data["y"].AsDouble(),(float)client_click_data["z"].AsDouble());
        if(current_click_held_entity != null && ent != null) // Catching entity drags!
        {
            if(TOOLS.VecDist(release_pos, current_click_start_pos) > 0.1f || current_click_held_entity != ent)
            {
                // Dragged onto another entity!
                current_click_held_entity.Dragged(focused_entity,ent,TOOLS.ParseJson(parameters_json));
            }
            else
            {
                // Click on same entity!
                if(focused_entity != null)
                {
                    DAT.Dir old_dir = focused_entity.direction;
                    focused_entity.Clicked(focused_entity?.ActiveHand,ent,TOOLS.ParseJson(parameters_json));
                    focused_entity.UpdateNetworkDirection(old_dir);
                }
            }
            // Cleanup
            current_click_start_pos = Vector3.Zero;
            current_click_held_entity = null;
        }
    }


    /*****************************************************************
     * Client camera handling
     ****************************************************************/
    [Export]
    public Camera3D camera;
    private float zoom_level = 1f;
    private float view_rotation = 0f;
    private AudioListener3D listener;
    public Vector3 CamRotationVector3()
    {
        Vector2 vec = CamRotationVector2();
        return new Vector3(vec.X,0,vec.Y).Normalized();
    }
    public Vector2 CamRotationVector2()
    {
        return new Vector2(Mathf.Sin(view_rotation),Mathf.Cos(view_rotation)).Normalized();
    }
    public override void _PhysicsProcess(double delta)
    {
        if(!IsMultiplayerAuthority()) return;
        // Client only camera update
        if(camera.Current == false)
        {
            camera.Current = true;
            listener = new AudioListener3D();
            AddChild(listener);
        }
        float lerp_speed = Mathf.Lerp(2f,40f, Mathf.Max(0 , Mathf.InverseLerp(-1,22,TOOLS.VecDist(camera.Position,focused_position) )));
        camera.Position = camera.Position.MoveToward(focused_position + CamRotationVector3() + new Vector3(0f,Mathf.Lerp(MainController.min_zoom,MainController.max_zoom,zoom_level),0), (float)delta * lerp_speed);
        camera.LookAt(focused_position + new Vector3(0,0.1f,0));
        listener.Position = focused_position + Vector3.Up;
    }


    /*****************************************************************
     * Client sound transmission
     ****************************************************************/
    public void PlaySoundAt(string path, Vector3 pos, float range, float volume_mod)
    {
        if(!has_logged_in) return;
        if(TOOLS.PeerConnected(this)) Rpc(nameof(ClientPlayAudio),path, pos, range, volume_mod);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = (int)MainController.RPCTransferChannels.ClientData)]
    public void ClientPlayAudio(string path, Vector3 pos, float range, float volume_mod)
    {
        SoundPlayer newsound = GD.Load<PackedScene>("res://Prefabs/SoundPlayer.tscn").Instantiate() as SoundPlayer;
        if(range <= -1)
        {
            newsound.player_global = new AudioStreamPlayer();
            newsound.player_global.VolumeDb = volume_mod;
            newsound.AddChild(newsound.player_global);
        }
        else
        {
            newsound.player_pos = new AudioStreamPlayer3D();
            newsound.player_pos.UnitSize = range * 0.60f; // Gestimate for byond like sound range
            newsound.player_pos.VolumeDb = volume_mod;
            newsound.AddChild(newsound.player_pos);
        }
        newsound.path = path;
        newsound.Position = pos;
        GetTree().Root.AddChild(newsound);
    }


    /*****************************************************************
     * Client disconnetion
     ****************************************************************/
    public void DisconnectClient()
    {
        // Server handling client DC
        AccountController.ClientLeave(this);
        focused_entity?.ClearClientOwner();
        MainController.controller.Multiplayer.MultiplayerPeer.DisconnectPeer(int.Parse(Name)); // Calls Kill() remotely
    }
}
