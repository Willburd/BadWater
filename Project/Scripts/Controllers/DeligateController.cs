using Godot;
using System;

public class DeligateController
{
    enum State
    {
        not_init,   // Not yet starting
        started,    // Starting up (threaded could might be happening still)
        ready       // All setup finished
    }

    private State current_state = State.not_init;
    private int tick_rate = 1;
    private int ticks = 0;
    private int pause_ticks = 0;

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
        FinishInit();
        return true;
    }

    public void Started()
    {
        GD.Print("Subsystem Started Init: " + this.GetType().Name);
        current_state = State.started;
    }

    public void FinishInit()            // Called when this controller has finished its initilization
    {
        current_state = State.ready;
        GD.Print("Subsystem Finished Init: " + this.GetType().Name);
    }

    
    public bool IsStarted               // Used by main controller during setup, to prevent double init
    {
        get { return current_state >= State.started; }
    }
    public bool IsDoneInit              // Used by main controller to know that all controllers are ready for first game tick
    {
        get { return current_state >= State.ready; }
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
            Fire();
            ticks = 0;
            return true;
        }
        return false;
    }

    public virtual void Fire() {}       // Called by Tick() at the rate the controller specifies

    public void Pause(int ticks)        // Sets a tick delay that must be reached before ticks may continue
    {
        pause_ticks = ticks;
        ticks = 0;
    }
    
    public virtual void Shutdown() {}
}
