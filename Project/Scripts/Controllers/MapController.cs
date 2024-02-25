using Godot;
using System;

public partial class MapController : DeligateController
{
    public static MapController controller;

    public override bool CanInit()
    {
        return true;
    }

    public override bool Init()
    {
        tick_rate = 3;
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
