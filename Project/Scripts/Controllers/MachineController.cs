using Godot;
using System;

public partial class MachineController : DeligateController
{
    public override bool CanInit()
    {
        return true; // waiting on the map controller first
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
