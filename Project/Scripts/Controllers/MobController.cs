using Godot;
using System;
using System.Collections.Generic;

public partial class MobController : DeligateController
{
    public List<AbstractEntity> living_entities = new List<AbstractEntity>();
    public List<AbstractEntity> dead_entities = new List<AbstractEntity>();
    public List<AbstractEntity> ghost_entities = new List<AbstractEntity>();


    public const int life_tick_mod = 4;  // ticks between life ticks


    public static MobController controller;    // Singleton reference for each controller, mostly used during setup to check if controller has init.
	public MobController()
    {
        controller = this;
    }


    public override bool CanInit()
    {
        return IsSubControllerInit(AtmoController.controller); // waiting on the Atmo controller, and by proxy: Map and Chem controllers!
    }

    public override bool Init()
    {
        display_name = "Mob";
        tick_rate = 1;
        return true;
    }

    public override void SetupTick()
    {
        FinishInit();
    }

    public override bool Fire()
    {
        //GD.Print(Name + " Fired");

        for(int i = 0; i < ghost_entities.Count; i++) 
        {
            ghost_entities[i].Process(MainController.WorldTicks);
        }

        if(MainController.server_state == MainController.ServerConfig.Editor) return MainController.WorldTicks % life_tick_mod == 0; // No life tick in edit mode

        for(int i = 0; i < living_entities.Count; i++) 
        {
            living_entities[i].Process(MainController.WorldTicks);
        }
        for(int i = 0; i < dead_entities.Count; i++) 
        {
            dead_entities[i].Process(MainController.WorldTicks);
        }

        return MainController.WorldTicks % life_tick_mod == 0;
    }

    public override void Shutdown()
    {
        
    }
}
