using Godot;
using System;
using System.Collections.Generic;

public partial class AudioController : DeligateController
{
    public override bool CanInit()
    {
        return true;
    }

    public override bool Init()
    {
        tick_rate = -1; // NO TICK
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


    public static void Play()
    {

    }

    public static void Loop()
    {
        
    }
}
