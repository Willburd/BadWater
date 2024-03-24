using Godot;
using System;

public partial class EditorController : DeligateController
{
    public static EditorController controller;    // Singleton reference for each controller, mostly used during setup to check if controller has init.
    public EditorController()
    {
        controller = this;
    }

    public override bool CanInit()
    {
        return IsSubControllerInit(MapController.controller); // waiting on the map controller first
    }

    public override bool Init()
    {
        display_name = "Editor";
        tick_rate = 1;
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
