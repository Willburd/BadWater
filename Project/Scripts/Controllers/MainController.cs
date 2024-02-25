using Godot;
using System;
using System.Collections.Generic;

public partial class MainController : Node
{
	public static MainController controller;

	private static List<DeligateController> subcontrollers = new List<DeligateController>();

	public static int tick_rate = 40;
	private static double tick_internal;	// delta_time counter for tick_rate calculation
	private static bool setup_phase = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// self singleton for all the others.
		controller = this;

		// Create subcontrollers!
		subcontrollers.Add(new MapController());
		subcontrollers.Add(new AtmoController());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{	
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
			if(all_ready) setup_phase = false;
			return;
		}
		
		// server ticker
		tick_internal += delta;
		double delta_rate = 1.0 / (double)tick_rate;
		while(tick_internal >= delta_rate)
		{
			delta_rate -= delta_rate;
			ServerTick();
		}
	}

	private void ServerTick()
	{
		// Update all deligate controllers
		for(int i = 0; i < subcontrollers.Count; i++) 
		{
			subcontrollers[i].Tick();
		}
	}
}
