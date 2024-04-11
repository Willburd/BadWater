using Godot;
using System;

public partial class ChemController : DeligateController
{
    public static ChemController controller;    // Singleton reference for each controller, mostly used during setup to check if controller has init.
    public ChemController()
    {
        controller = this;
    }

    public override bool CanInit()
    {
        return true;
    }

    public override bool Init()
    {
        display_name = "Chem";
        tick_rate = -1; // NO TICK
        return true;
    }

    public override void SetupTick()
    {
        FinishInit();
    }

    public override bool Fire()
    {
        //GD.Print(Name + " Fired");

        return true;
    }

    public override void Shutdown()
    {
        
    }
}
