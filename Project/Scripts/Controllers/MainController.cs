using Godot;
using System;
using System.Collections.Generic;

[GlobalClass] 
public partial class MainController : Node
{
	public static MainController controller;

	private static List<DeligateController> subcontrollers = new List<DeligateController>();

	public enum ServerConfig
	{
		Standard = 0,
		Editor = 1
	}

	[Export]
	public static ServerConfig server_state = ServerConfig.Standard;

	public static int tick_rate = 40;
	private static double tick_internal;	// delta_time counter for tick_rate calculation
	private static bool setup_phase = true;
	private static int ticks = 0;

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
				subcontrollers.Add(new MapController());
				subcontrollers.Add(new AtmoController());
				subcontrollers.Add(new MachineController());
				subcontrollers.Add(new MobController());
			break;

			case ServerConfig.Editor:
				subcontrollers.Add(new MapController());
				subcontrollers.Add(new EditorController());
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
		ticks += 1;
	}

	private void EditorTick()
	{
		// TODO - editor mode!
		ticks += 1;
	}
}
