using Godot;
using System;
using System.Collections.Generic;

[GlobalClass] 
public partial class NetworkClient : Node3D
{
	public static List<NetworkClient> clients = new List<NetworkClient>();

    [Export]
    public string id = "";

    public void Spawn(string new_id)
    {
        id = new_id;
        clients.Add(this);
    }

    public void Kill()
    {
        clients.Remove(this);
    }
}
