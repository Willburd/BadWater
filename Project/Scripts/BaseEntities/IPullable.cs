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
        // Recursive pull search to avoid crashes
        IPullable check_pulling = pulled;
        while(check_pulling != null)
        {
            if(check_pulling is ICanPull recursive_puller)
            {
                if(recursive_puller.I_Pulling == null) break;
                if(recursive_puller.I_Pulling == puller)
                {
                    recursive_puller.I_StopPulling();
                    break;
                }
                check_pulling = recursive_puller.I_Pulling;
            }
        }
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
        // Get pulling intensity
        float dist = TOOLS.VecDist(pulled_ent.GridPos.WorldPos(),pulling_ent.GridPos.WorldPos());
        if(dist < 0.35f) return Vector3.Zero;
        float pullspeed = Mathf.InverseLerp(0.25f,2f,dist);
        // Tug entity to new world pos!
        return TOOLS.DirVec(pulled_ent.GridPos.WorldPos(),pulling_ent.GridPos.WorldPos()) * Mathf.Clamp(pullspeed,0f,1f);
    }

    public void I_TryStartPulling(IPullable pulling);
    public void I_StopPulling();

    public IPullable I_Pulling {get;set;}
}