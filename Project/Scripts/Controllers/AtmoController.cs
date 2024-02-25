using Godot;
using System;

public partial class AtmoController : DeligateController
{
    public static AtmoController controller;

    public override bool CanInit()
    {
        return IsSubControllerInit(MapController.controller); // waiting on the map controller first
    }

    public override bool Init()
    {
        tick_rate = 5;
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

