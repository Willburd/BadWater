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
        Mob
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
    public List<ulong> logged_times = new List<ulong>();

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
	
	private static double tick_internal;	// delta_time counter for tick_rate calculation
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
		switch(server_state)
		{
			case ServerConfig.Standard:
				subcontrollers.Add(new AccountController());
				subcontrollers.Add(new MapController());
				subcontrollers.Add(new ChemController());
				subcontrollers.Add(new AtmoController());
				subcontrollers.Add(new MachineController());
				subcontrollers.Add(new MobController());
				subcontrollers.Add(new ChunkController());
			break;

			case ServerConfig.Editor:
				subcontrollers.Add(new AccountController());
				subcontrollers.Add(new MapController());
				subcontrollers.Add(new EditorController());
				subcontrollers.Add(new ChunkController());
			break;
		}
	}

	public override void _Process(double delta)
	{	
		// delta threshold for ticks
		double delta_rate = 1.0 / (double)tick_rate;

		// setup runs as fast as possible
		if(setup_phase)
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
				GD.Print("Delta Threshold " + delta_rate);
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
			return;
		}
		
		// server ticker
		tick_internal += delta;
		while(tick_internal > delta_rate)
		{	
			ServerTick();
			tick_internal -= delta_rate;
		}
	}

	private void ServerTick()
	{
		// Update all deligate controllers
		ulong server_start_time = Time.GetTicksUsec();
		for(int i = 0; i < subcontrollers.Count; i++) 
		{
			DeligateController con = subcontrollers[i];
			// Process controllers
			ulong start_time = Time.GetTicksUsec();
			con.did_tick = subcontrollers[i].Tick();
			ulong end_time = Time.GetTicksUsec();
			if(!con.did_tick) continue;
			// debug logging
			if(con.logged_times.Count > 10) con.logged_times.RemoveAt(0);
			con.logged_times.Add(end_time - start_time);
		}
		for(int i = 0; i < client_container.GetChildCount(); i++) 
		{
			NetworkClient client = client_container.GetChild(i) as NetworkClient;
			client.Tick();
		}
		ticks += 1;
		ulong server_end_time = Time.GetTicksUsec();
		// debug logging
		if(logged_times.Count > 10) logged_times.RemoveAt(0);
		logged_times.Add(server_end_time - server_start_time);
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
