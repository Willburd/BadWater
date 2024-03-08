using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

[GlobalClass] 
public partial class MainController : Node
{
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

	public static MainController controller;

	[Export]
	public ConfigData config;

	private static List<DeligateController> subcontrollers = new List<DeligateController>();

	public enum ServerConfig
	{
		Standard = 0,
		Editor = 1
	}

	[Export]
	public static ServerConfig server_state = ServerConfig.Standard;

	public const int max_zoom = 8;
	public const int tick_rate = 40;
	
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
			for(int i = 0; i < controller.client_container.GetChildCount(); i++) 
			{
				ret.Add(controller.client_container.GetChild(i) as NetworkClient);
			}
			return ret;
		}
	} 

	public void Init(ServerConfig state)
	{
		// self singleton for all the others.
		GD.Print("Starting Server in state: " + state);
		server_state = state;
		controller = this;

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
				subcontrollers.Add(new ChatController());
			break;

			case ServerConfig.Editor:
				subcontrollers.Add(new AccountController());
				subcontrollers.Add(new MapController());
				subcontrollers.Add(new EditorController());
				subcontrollers.Add(new ChunkController());
				subcontrollers.Add(new ChatController());
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
		while(tick_internal >= delta_rate)
		{	
			ServerTick();
			tick_internal -= delta_rate;
		}
	}

	private void ServerTick()
	{
		// Update all deligate controllers
		for(int i = 0; i < subcontrollers.Count; i++) 
		{
			subcontrollers[i].Tick();
		}
		for(int i = 0; i < client_container.GetChildCount(); i++) 
		{
			NetworkClient client = client_container.GetChild(i) as NetworkClient;
			client.Tick();
		}
		ticks += 1;
	}

	private void EditorTick()
	{
		// TODO - editor mode!
		ticks += 1;
	}
}
