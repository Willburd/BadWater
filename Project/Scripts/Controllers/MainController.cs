using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

[GlobalClass] 
public partial class MainController : Node
{    
	public static MainController controller;
	public MainController()
    {
        controller = this;
    }


    public enum DataType
    {
        Map,
        Area,
		Turf,
        Chunk,
        Effect,
        Item,
        Structure,
        Machine,
        Mob,
		Reagent,
		Gasmix
    }

	public enum RPCTransferChannels
    {
        Main,
		Movement,
		VisualUpdate,
		MachineUpdate,
		ClientData,
		Chat,
		Sound
    }

	[Export]
	public ConfigData config;
    public TickRecord logged_times = new TickRecord();
	public TickRecord tick_gap_times = new TickRecord();

	private static List<DeligateController> subcontrollers = new List<DeligateController>();
	public static int GetSubControllerCount()
	{
		return subcontrollers.Count;
	}
	public static DeligateController GetSubControllerAtIndex(int i) // Debugging only please.
	{
		return subcontrollers[i];
	}

	public enum ServerConfig
	{
		Standard = 0,
		Editor = 1
	}

	[Export]
	public static ServerConfig server_state = ServerConfig.Standard;

	public const int min_zoom = 3;
	public const int max_zoom = 9;
	public const int tick_rate = 25; // Match ss13
	
	private static ulong next_tick_time;
	private static bool setup_phase = true;
	private static int ticks = 0;

	[Export]
	public Node entity_container;
	[Export]
	public Node client_container;

	public static List<NetworkClient> ClientList
	{
		get 
		{
			List<NetworkClient> ret = new List<NetworkClient>();
			for(int i = 0; i < ClientContainer.GetChildCount(); i++) 
			{
				ret.Add(ClientContainer.GetChild(i) as NetworkClient);
			}
			return ret;
		}
	} 
	public static Node ClientContainer
	{
		get{return MainController.controller.client_container;}
	} 

	public void Init(ServerConfig state)
	{
		// self singleton for all the others.
		GD.Print("Starting Server in state: " + state);
		server_state = state;

		// Create subcontrollers!
		subcontrollers.Add(new AccountController());	// Player logins and rejoining
		subcontrollers.Add(new MapController());		// Loaded map processing, and random updates for turfs
		subcontrollers.Add(new ChemController());		// Chemical reactions processing
		subcontrollers.Add(new AtmoController());		// Atmospherics system
		subcontrollers.Add(new MobController());		// Mobs living and dead and their life ticks
		subcontrollers.Add(new MachineController());	// Machine currently constructed and processing
		subcontrollers.Add(new ChunkController());		// Loaded chunks, and which clients get those chunks
		if(server_state == ServerConfig.Editor)
		{
			subcontrollers.Add(new EditorController());		// Editor storage for what objects are placable/searching it, processing placement of new entities/turfs
		}
		else
		{
			subcontrollers.Add(new EventController());		// Event choreographer for station SPICE
		}
	}

	public override void _Process(double delta)
	{	
		// setup runs as fast as possible
		if(setup_phase)
		{
			SetupTick();
			return;
		}
		
		// server ticker
		ulong current_tick = Time.GetTicksMsec();
		ulong started_tick = current_tick;
		while(current_tick >= next_tick_time)
		{	
			ServerTick();
			next_tick_time = current_tick + (ulong)(1000f / tick_rate);
			// debug logging
			tick_gap_times.Append(next_tick_time - started_tick);
			current_tick = Time.GetTicksMsec();
		}
	}

	private void SetupTick()
	{
		// threaded init of controllers
		// if a controller requires another one to be init before itself, it needs to handle that by checking the subcontroller's singleton
		// EX: if the atmosphere controller requires the map controller to be finished before it can setup!
		// wait for all controllers...
		bool all_ready = true;
		for(int i = 0; i < subcontrollers.Count; i++) 
		{
			DeligateController con = subcontrollers[i];
			if(!con.IsStarted && con.CanInit())
			{
				con.Started();
				con.Init();
			}
			if(con.IsStarted && !con.IsDoneInit)
			{
				con.SetupTick();
			}
			if(!con.IsDoneInit)
			{
				all_ready = false;
			}
		}
		// ready to begin gameticks
		if(all_ready) 
		{
			GD.Print("Setup Finished");
			GD.Print("Tick rate: " + tick_rate);
			// Remove notickers
			for(int i = 0; i < subcontrollers.Count; i++) 
			{
				if(subcontrollers[i].NoTick)
				{
					subcontrollers.RemoveAt(i);
				}
			}
			// Done
			setup_phase = false;
		}
	}

	private void ServerTick()
	{
		// Update all deligate controllers
		ulong server_start_time = Time.GetTicksMsec();
		for(int i = 0; i < subcontrollers.Count; i++) 
		{
			DeligateController con = subcontrollers[i];
			// Process controllers
			ulong start_time = Time.GetTicksMsec();
			con.did_tick = subcontrollers[i].Tick();
			if(!con.did_tick) continue;
			// debug logging
			con.logged_times.Append(Time.GetTicksMsec() - start_time);
		}
		for(int i = 0; i < client_container.GetChildCount(); i++) 
		{
			NetworkClient client = client_container.GetChild(i) as NetworkClient;
			client.Tick();
		}
		ticks += 1;
		// debug logging
		logged_times.Append(Time.GetTicksMsec() - server_start_time);
	}

	private void EditorTick()
	{
		// TODO - editor mode!=================================================================================================================================
		ticks += 1;
	}

	public static int WorldTicks
	{
		get {return ticks;}
	}
}
