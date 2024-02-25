using Godot;
using System;

public partial class MobController : DeligateController
{
    public override bool CanInit()
    {
        return IsSubControllerInit(MapController.controller); // waiting on the map controller first
    }

    public override bool Init()
    {
        tick_rate = 4;
        controller = this;
        FinishInit();
        return true;
    }

    public override void Fire()
    {
        //GD.Print(Name + " Fired");
    }

    public override void Shutdown()
    {
        
    }
}
