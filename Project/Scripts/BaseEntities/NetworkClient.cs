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
        clients.Add(this);
        camera.Current = false;
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
        if(IsNodeReady() && !IsMultiplayerAuthority()) return;
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
