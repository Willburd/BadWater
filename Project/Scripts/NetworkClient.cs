using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

[GlobalClass] 
public partial class NetworkClient : Node
{
	public static List<NetworkClient> clients = new List<NetworkClient>();

    [Export]
    public string id = "";
    [Export]
    public string focused_map_id;
    [Export]
    public Vector3 focused_position;

    private NetworkEntity focused_entity;

    [Export]
    public Camera3D camera;


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
        camera.Current = IsMultiplayerAuthority();
        camera.Position = focused_position;
    }

    public void Spawn(string new_id)
    {
        id = new_id;
        clients.Add(this);
        camera.Current = false;
    }

    public void Kill()
    {
        clients.Remove(this);
    }
}
