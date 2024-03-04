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
    [Export]
    public Vector3 focused_position;

    private float zoom_level = 1f;

    private NetworkEntity focused_entity;

    [Export]
    public Camera3D camera;

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(PeerID);
    }

    public void SetFocusedEntity(NetworkEntity ent)
    {
        focused_entity = ent;
        focused_map_id = focused_entity.map_id_string;
        focused_position = focused_entity.Position;
    }

    public void ClearFocusedEntity()
    {
        focused_entity = null;
    }

    public void Spawn()
    {
        // Time to forceset a position!
        string new_map;
        MapController.GridPos new_pos;
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
                int rand = (int)GD.Randi() % spawners.Count;
                new_map = spawners[rand].map_id_string;
                new_pos = spawners[rand].grid_pos;
                GD.Print("-map: " + new_map);
                GD.Print("-pos: " + new_pos);
                Rpc(nameof(UpdateClientFocusedPos),new_map,TOOLS.GridToPosWithOffset(new_pos));
                return;
            }
            else
            {
                GD.Print("-NO SPAWNERS!");
            }
        }
        // EMERGENCY FALLBACK TO 0,0,0 on first map loaded!
        GD.Print("Client FALLBACK RESPAWN: " + Name);
        new_map = MapController.FallbackMap();
        new_pos = new MapController.GridPos((float)0.5,(float)0.5,0);
        GD.Print("-map: " + new_map);
        GD.Print("-pos: " + new_pos);
        Rpc(nameof(UpdateClientFocusedPos),new_map,TOOLS.GridToPosWithOffset(new_pos));
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)] // Tell the client we want to forcibly move this
    public virtual void UpdateClientFocusedPos(string map_id, Vector3 new_pos)
    {
        GD.Print("Client " + Name + " got movement update: " + " to: " + map_id + " : " + new_pos);
        focused_map_id = map_id;
        focused_position = new_pos;
    }


    public void Tick()
    {
        if(focused_entity != null)
        {
            focused_map_id = focused_entity.map_id_string;
            focused_position = focused_entity.Position;
        }
    }

    public override void _Process(double delta)
    {
        if(!IsMultiplayerAuthority()) return;
        // Client only!
        camera.Current = true;
        camera.Position = focused_position + new Vector3(0f,zoom_level * MainController.max_zoom,0.3f);
        camera.LookAt(focused_position);
    }

    public void Kill()
    {
        clients.Remove(this);
    }
}
