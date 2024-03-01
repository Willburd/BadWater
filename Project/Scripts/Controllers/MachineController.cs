using Godot;
using System;

public partial class MachineController : DeligateController
{
    public override bool CanInit()
    {
        return IsSubControllerInit(AtmoController.controller); // waiting on the Atmo controller, and by proxy: Map and Chem controllers!
    }

    public override bool Init()
    {
        tick_rate = 10;
        controller = this;
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
