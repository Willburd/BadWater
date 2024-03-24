using Godot;
using System;

public partial class MobController : DeligateController
{
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
        tick_rate = 4;
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
