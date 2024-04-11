using Godot;
using System;
using System.Collections.Generic;

public partial class MachineController : DeligateController
{
    public List<AbstractEntity> entities = new List<AbstractEntity>();

    public static MachineController controller;    // Singleton reference for each controller, mostly used during setup to check if controller has init.
    public MachineController()
    {
        controller = this;
    }

    public override bool CanInit()
    {
        return IsSubControllerInit(AtmoController.controller); // waiting on the Atmo controller, and by proxy: Map and Chem controllers!
    }

    public override bool Init()
    {
        display_name = "Machines";
        tick_rate = 10;
        return true;
    }

    public override void SetupTick()
    {
        FinishInit();
    }

    public override bool Fire()
    {
        //GD.Print(Name + " Fired");
        if(MainController.server_state == MainController.ServerConfig.Editor) return false; // No random ticks in edit mode

        for(int i = 0; i < entities.Count; i++) 
        {
            entities[i].Process(MainController.WorldTicks);
        }

        return true;
    }

    public override void Shutdown()
    {
        
    }
}
