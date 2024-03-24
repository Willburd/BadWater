using Godot;
using System;

[GlobalClass]
public partial class BootController : Node
{
    public static BootController controller;
    public BootController()
    {
        controller = this;
    }


    [Export]
    public Node entity_container;
    [Export]
    public Node client_container;

    [Export]
    public PackedScene client_prefab;
    [Export]
    public MultiplayerSpawner client_spawner;
    [Export]
    public MultiplayerSpawner entity_spawner;
    [Export]
    public MultiplayerSpawner chunk_spawner;

    int max_players = 0;	//Set from the ClientSpawner's data
    int max_entities = 0;  //Set from the EntitySpawners's data
    int max_chunks = 0;    //Set from the EntitySpawners's data
    AssetLoader asset_library;
    ConfigData config; 

    public override void _Ready()
    {
        // UI controls
        WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.JoinMenu);
        // Load config
        config = new ConfigData();
        config.Load("res://Config/Setup.json");
        //Load asset library
        asset_library = new AssetLoader();
        asset_library.Load();
        //Spawn server from launch options
        var arguments = new Godot.Collections.Dictionary();
        foreach (var argument in OS.GetCmdlineArgs())
        {
            if (argument.Find("=") > -1)
            {
                string[] keyValue = argument.Split("=");
                arguments[keyValue[0].Replace("--", "")] = keyValue[1];
            }
            else
            {
                arguments[argument.Replace("--", "")] = "";
            }
        }
        if(arguments.ContainsKey("s") || arguments.ContainsKey("e") || arguments.ContainsKey("headless"))
        {
            StartNetwork(true,arguments.ContainsKey("e"));
            WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.ServerConfig);
        }
    }

    public void StartNetwork(bool server, bool edit_mode)
    {
        GD.Print("Start networking");
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        if(server)
        {
            //Link signals
            Multiplayer.PeerConnected += _PeerJoin;
            Multiplayer.PeerDisconnected += _PeerLeave;
            //Set limits
            max_players = (int)client_spawner.SpawnLimit;
            max_entities = (int)entity_spawner.SpawnLimit;
            max_chunks = (int)chunk_spawner.SpawnLimit;
            //Create godot network server
            Error status = peer.CreateServer(config.port,max_players);
            if (status != Error.Ok)
            {
                GD.PrintErr("Server could not be created:");
                GD.PrintErr("Port: " + config.port);
                WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.JoinMenu);
                return;
            }
            peer.Host.Compress(ENetConnection.CompressionMode.Fastlz);
            // Set Spawn limits
            client_spawner.SpawnLimit = (uint)config.max_clients;
            entity_spawner.SpawnLimit = (uint)config.max_entities;
            chunk_spawner.SpawnLimit = (uint)config.max_chunks;
            //Start controller
            MainController server_scene = (MainController)GD.Load<PackedScene>("res://Prefabs/Server.tscn").Instantiate();
            server_scene.client_container = client_container;
            server_scene.entity_container = entity_container;
            server_scene.config = config;
            AddChild(server_scene);
            server_scene.Init(edit_mode ? MainController.ServerConfig.Editor : MainController.ServerConfig.Standard);
        }
        else
        {
            //Create godot client connection to server
            Error status = peer.CreateClient(WindowManager.controller.join_window.ip_entry.Text,int.Parse(WindowManager.controller.join_window.port_entry.Text));
            if (status != Error.Ok)
            {
                GD.PrintErr("Creating client FAILED.");
                WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.JoinMenu);
                return;
            }
            peer.Host.Compress(ENetConnection.CompressionMode.Fastlz);
        }
        Multiplayer.MultiplayerPeer = peer;
    }
        
    public void _PeerJoin(long id)
    {
        GD.Print("Peer join: " + id);
        NetworkClient c = (NetworkClient)client_prefab.Instantiate();
        c.Name = id.ToString();
        client_container.AddChild(c,true);
    }
        
    public void _PeerLeave(long id)
    {
        GD.Print("Peer Leave: " + id);
        NetworkClient c = (NetworkClient)client_container.GetNode(id.ToString());
        if(c != null && c.Multiplayer.MultiplayerPeer != null && IsMultiplayerAuthority()) c.DisconnectClient();
        //Removal of the client is done in the NetworkClient it self!
        if(IsMultiplayerAuthority()) return; //Only run on DC client
        //Reset the client...
        while(entity_container.GetChildCount() > 0) entity_container.GetChild(0).QueueFree();
        while(client_container.GetChildCount() > 0)client_container.GetChild(0).QueueFree();
        GD.Print("LEFT GAME");
        WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.JoinMenu);
    }
}
