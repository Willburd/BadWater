using Godot;
using System;

public partial class EventController : DeligateController
{
    public static EventController controller;    // Singleton reference for each controller, mostly used during setup to check if controller has init.
    public EventController()
    {
        controller = this;
    }

    public override bool CanInit()
    {
        return IsSubControllerInit(MapController.controller);
    }

    public override bool Init()
    {
        display_name = "Event";
        tick_rate = MainController.tick_rate; // 1 tick a second!
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
