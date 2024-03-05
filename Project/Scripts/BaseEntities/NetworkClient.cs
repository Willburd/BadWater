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


    private Godot.Collections.Dictionary client_input_data = new Godot.Collections.Dictionary();
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
        focused_position = focused_entity.grid_pos.WorldPos();
        Rpc(nameof(UpdateClientFocusedPos),focused_map_id,focused_position);
    }

    public void ClearFocusedEntity()
    {
        focused_entity = null;
    }

    public void Spawn()
    {
        // Prep
        clients.Add(this);
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
                SpawnHostEntity(spawners[rand].map_id_string,spawners[rand].grid_pos);
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
        if(focused_entity == null) focused_entity = AbstractEffect.CreateEntity(new_map,"BASE:THINGY",MainController.DataType.Item);
        focused_entity.Move(new_map,new_pos,false);
        // Inform client of movment from server
        Rpc(nameof(UpdateClientFocusedPos),new_map,TOOLS.GridToPosWithOffset(new_pos));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)] // Tell the client we want to forcibly move this
    public virtual void UpdateClientFocusedPos(string map_id, Vector3 new_pos)
    {
        focused_map_id = map_id;
        focused_position = new_pos;
        sync_map_id = focused_map_id;
        sync_position = focused_position;
    }

    public void Tick()
    {
        UpdateClientControl();
        if(focused_entity != null)
        {
            focused_map_id = focused_entity.map_id_string;
            focused_position = focused_entity.grid_pos.WorldPos();
            if(sync_map_id != focused_map_id || sync_position != focused_position)
            {
                Rpc(nameof(UpdateClientFocusedPos),focused_map_id,focused_position);
            }
        }
    }

    private void UpdateClientControl()
    {
        focused_entity?.ControlUpdate(client_input_data);
        client_input_data = new Godot.Collections.Dictionary();
    }

    public override void _Process(double delta)
    {
        if(!IsMultiplayerAuthority()) return;
        // Client only!
        camera.Current = true;
        camera.Position = focused_position + new Vector3(0f,zoom_level * MainController.max_zoom,0.3f);
        camera.LookAt(focused_position);
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void SetClientControl(string control_data)
    {
        client_input_data = (Godot.Collections.Dictionary)Json.ParseString(control_data);
    }

    public void Kill()
    {
        clients.Remove(this);
    }
}
