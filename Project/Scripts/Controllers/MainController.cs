using Godot;
using System;
using System.Collections.Generic;

public partial class MainController : Node
{
	public static MainController controller;

	private static List<DeligateController> subcontrollers = new List<DeligateController>();
	private static List<Entity> entities = new List<Entity>();

	private Node entity_container;
	private Node client_container;

	public static int tick_rate = 40;
	private static double tick_internal;	// delta_time counter for tick_rate calculation
	private static bool setup_phase = true;
	private static int ticks = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// self singleton for all the others.
		GD.Print("Starting Server");
		controller = this;

		// Create subcontrollers!
		subcontrollers.Add(new MapController());
		subcontrollers.Add(new AtmoController());
		subcontrollers.Add(new MachineController());
		subcontrollers.Add(new MobController());
		
		// Get entity container from godot
		Node main_scene = GetParent();
		for(int i = 0; i < main_scene.GetChildCount(); i++) 
		{
			Node nd = main_scene.GetChild(i);
			if(nd.Name == "Entities")
			{
				GD.Print("Entity Container Linked");
				entity_container = nd;
			}
			if(nd.Name == "Clients")
			{
				GD.Print("Client Container Linked");
				client_container = nd;
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
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

	public void AddEntity(Entity ent, WorldPos pos, bool map_spawn = false)
	{
		entities.Add(ent);
		if(map_spawn)
		{
			ent.MapInit(pos);
		}
		else
		{
			ent.Spawn(pos);
		}
	}

	public void RemoveEntity(Entity ent)
	{
		ent.Destroy();
		entities.Add(ent);
	}
}
