using Godot;
using System;

public partial class MachineController : DeligateController
{
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

    public override void Fire()
    {
        //GD.Print(Name + " Fired");
    }

    public override void Shutdown()
    {
        
    }
}
