using Godot;
using System;
using System.Collections.Generic;

public interface IPullable
{
    public ICanPull I_Pulledby {get;set;}
    public static bool IsBeingPulling(IPullable pulled)
    {
        return pulled.I_Pulledby != null;
    }
}

public interface ICanPull
{
    protected static void BeginPull(ICanPull puller, IPullable pulled)
    {
        if(IsPulling(puller) != null) EndPull(puller); // release last pull
        // Start new pull
        pulled.I_Pulledby = puller;
        puller.I_Pulling = pulled;
    }
    protected static void EndPull(ICanPull puller)
    {
        if(IsPulling(puller) == null) return; // Not pulling so don't bother
        // End pull
        puller.I_Pulling.I_Pulledby = null;
        puller.I_Pulling = null;
    }
    public static IPullable IsPulling(ICanPull puller)
    {
        return puller.I_Pulling;
    }
    public void I_TryStartPulling(IPullable pulling);
    public void I_StopPulling();

    public IPullable I_Pulling {get;set;}
}