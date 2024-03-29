using Godot;
using System;
using System.Collections.Generic;

public static class TOOLS
{
    /*****************************************************************
     * Mouse and clicking
     ****************************************************************/
    public static Godot.Collections.Dictionary AssembleStandardClick(Vector3 pos)
    {
        Godot.Collections.Dictionary new_inputs = new Godot.Collections.Dictionary();
        new_inputs["mod_control"]   = Input.IsActionPressed("mod_control");
        new_inputs["mod_alt"]       = Input.IsActionPressed("mod_alt");
        new_inputs["mod_shift"]     = Input.IsActionPressed("mod_shift");
        new_inputs["button"]        = (int)MouseButton.None;
        new_inputs["state"]         = false;
        new_inputs["x"]             = pos.X;
        new_inputs["y"]             = pos.Y;
        new_inputs["z"]             = pos.Z;
        return new_inputs;
    }


    /*****************************************************************
     * Connection checks
     ****************************************************************/
    public static bool PeerDisconnected(Node node)
    {
        if(node == null) return true;
        if(node.Multiplayer.MultiplayerPeer == null) return true;
        return node.Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Disconnected;
    }
    public static bool PeerConnecting(Node node)
    {
        if(node == null) return false;
        if(node.Multiplayer.MultiplayerPeer == null) return false;
        return node.Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connecting;
    }
    public static bool PeerConnected(Node node)
    {
        if(node == null) return false;
        if(node.Multiplayer.MultiplayerPeer == null) return false;
        return node.Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;
    }


    /*****************************************************************
     * Random rolls and picks
     ****************************************************************/
    public static T Pick<T>(List<T> list)
    {   
        return list[RandI(list.Count)];
    }
    public static bool Prob(float percentile) // Rand check that is at or under value presented, float version of Byond's Prob()!
    {
        return Mathf.Abs(GD.Randf() % 100) <= percentile;
    }
    public static float RandF(float max)
    {
        return GD.Randf() * max;
    }
    public static float RandF(float min, float max)
    {
        return min + RandF(max - min);
    }
    public static int RandI(int max)
    {
        return Mathf.Abs((int)GD.Randi()) % max;
    }
    public static int RandI(int min, int max)
    {
        return min + RandI(max - min);
    }


    /*****************************************************************
     * Vector tools
     ****************************************************************/
    static public float VecDist(float x1, float y1, float x2, float y2)
    {
        return VecDist(new Vector2(x1,y1) , new Vector2(x2,y2) );
    }
    static public float VecDist(Vector2 start, Vector2 goal)
    {
        return (goal-start).Length();
    }
    static public float VecDist(Vector3 start, Vector3 goal)
    {
        return (goal-start).Length();
    }
    static public Vector2 DirVec(float x1, float y1, float x2, float y2)
    {
        return DirVec(new Vector2(x1,y1) , new Vector2(x2,y2) );
    }
    static public Vector2 DirVec(Vector2 start, Vector2 goal)
    {
        return (goal-start).Normalized();
    }
    static public Vector3 DirVec(Vector3 start, Vector3 goal)
    {
        return (goal-start).Normalized();
    }
    static public Vector3 GridToPosWithOffset(GridPos grid)
    {
        return new Vector3(grid.hor,grid.dep,grid.ver) * MapController.tile_size;
    }
    static public Vector3 ChunkGridToPos(ChunkPos grid)
    {
        return new Vector3(grid.hor,grid.dep,grid.ver) * (ChunkController.chunk_size * MapController.tile_size);
    }

    /*****************************************************************
     * Entity tools
     ****************************************************************/
    public static DAT.Dir RotateTowardEntity(AbstractEntity A,AbstractEntity B)
    {
        if(!MapTools.OnSameMap(A,B) || B.GetLocation() is not AbstractTurf)
        {
            // ignore...
            return A.direction;
        }
        Vector3 dir_vec = MapTools.GetMapDirection(A,B);
        DAT.Dir ret = DAT.VectorToCardinalDir(dir_vec.X,dir_vec.Z);
        if(ret == DAT.Dir.None) return A.direction; // Final sanity check...
        return ret;
    }

    /*****************************************************************
     * Debug tools
     ****************************************************************/
    public static void PrintProgress(int steps, int max_steps)
    {
        for(int b = 1; b <= 10; b++) 
        {
            if(steps == Mathf.Floor(Mathf.Floor(max_steps / 10) * b)) ChatController.DebugLog("-" + (b * 10) + "%");
        }
    }
}