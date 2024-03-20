using Godot;
using System;
using System.Collections.Generic;

public interface IPullable
{
    public ICanPull I_Pulledby {get;set;}
}

public interface ICanPull
{
    protected static void Internal_BeginPull(ICanPull puller, IPullable pulled)
    {
        if(puller.I_Pulling != null) Internal_EndPull(puller); // release last pull
        // Start new pull
        pulled.I_Pulledby = puller;
        puller.I_Pulling = pulled;
    }
    protected static void Internal_EndPull(ICanPull puller)
    {
        if(puller.I_Pulling == null) return; // Not pulling so don't bother
        // End pull
        puller.I_Pulling.I_Pulledby = null;
        puller.I_Pulling = null;
    }

    protected static Vector3 Internal_HandlePull(ICanPull puller)
    {
        if(puller.I_Pulling == null) return Vector3.Zero; // Not pulling so don't bother

        AbstractEntity pulling_ent = puller as AbstractEntity;
        AbstractEntity pulled_ent = puller.I_Pulling as AbstractEntity;
        // Tug entity to new world pos!
        float pullspeed = TOOLS.VecDist(pulled_ent.GridPos.WorldPos(),pulling_ent.GridPos.WorldPos());
        if(pullspeed < 0.2f) pullspeed = 0.2f; // TODO proper pulling rates ============================================================================================================
        if(pullspeed > 1f) pullspeed = 1f;
        return TOOLS.DirVec(pulled_ent.GridPos.WorldPos(),pulling_ent.GridPos.WorldPos()) * pullspeed;
    }

    public void I_TryStartPulling(IPullable pulling);
    public void I_StopPulling();

    public IPullable I_Pulling {get;set;}
}