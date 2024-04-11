using Godot;
using System;
using System.Collections.Generic;

public class DeligateController
{
    enum State
    {
        not_init,   // Not yet starting
        started,    // Starting up
        ready       // All setup finished
    }

    public string display_name = "";
    public TickRecord logged_times = new TickRecord();

    private State current_state = State.not_init;
    protected int tick_rate = 1;                    // Ticks needed to Fire()
    public int GetTickRate()
    {
        return tick_rate;
    }
    private int ticks = 0;                          // Resets to 0 when it hits the tick_rate.
    private int pause_ticks = 0;
    public bool did_tick = false;

    public string Name               // Used by main controller during setup, to prevent double init
    {
        get { return GetType().Name; }
    }

	public static bool IsSubControllerInit(DeligateController check_con) // Used to simplify checking if othercontrollers are init
	{
		if(check_con == null) return false;
		return check_con.IsDoneInit;
	}

    public virtual bool CanInit()      // Used to check if other controllers are setup before this one inits. ex: atmo controller wanting the map to be loaded first
    {
        return true;
    }

    public virtual bool Init()          // Called when setting up, if returns true, calls Started() to prevent multiple inits
    {
        return true;
    }

    public void Started()
    {
        GD.Print("Subsystem Started Init: " + Name);
        current_state = State.started;
    }

    public void FinishInit()            // Called when this controller has finished its initilization
    {
        current_state = State.ready;
        GD.Print("Subsystem Finished Init: " + Name);
    }

    public bool IsStarted               // Used by main controller during setup, to prevent double init
    {
        get { return current_state >= State.started; }
    }
    public bool IsDoneInit              // Used by main controller to know that all controllers are ready for first game tick
    {
        get { return current_state >= State.ready; }
    }

    public virtual void SetupTick()
    {
        FinishInit();
    }

    public bool NoTick
    {
        get {return tick_rate <= 0;}
    }

    public bool Tick()
    {
        // Controller is paused
        ticks += 1;
        if(pause_ticks > 0)
        {
            if(ticks >= pause_ticks)
            {
                pause_ticks = 0;
            }
            return false;
        }
        // Update!
        if(ticks >= tick_rate)
        {
            // Fire subsystem, and process entities if the controller has them!
            ticks = 0;
            return Fire();
        }
        return false;
    }

    public virtual bool Fire()          // Called by Tick() at the rate the controller specifies
    {
        GD.Print(Name + " Fired");
        return true;
    }

    public void Pause(int ticks)        // Sets a tick delay that must be reached before ticks may continue
    {
        pause_ticks = ticks;
        ticks = 0;
    }

    public bool IsPaused
    {
        get {return pause_ticks > 0;}
    }
    
    public virtual void Shutdown() {}
}
