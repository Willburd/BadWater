using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

public static class TOOLS
{
    public static T Pick<T>(List<T> list)
    {   
        return list[RandI(list.Count)];
    }

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
    static public Vector3 GridToPosWithOffset(MapController.GridPos grid)
    {
        return new Vector3(grid.hor,grid.dep,grid.ver) * MapController.tile_size;
    }
    static public Vector3 ChunkGridToPos(MapController.ChunkPos grid)
    {
        return new Vector3(grid.hor,grid.dep,grid.ver) * (ChunkController.chunk_size * MapController.tile_size);
    }
    static public Godot.Collections.Dictionary ParseJsonFile(string file_path)
    {
        // Read text from file and then call parsejson.
        if(!Godot.FileAccess.FileExists(file_path)) 
        {
            ChatController.DebugLog("Missing file: " + file_path);
            return new Godot.Collections.Dictionary();
        }
        Godot.FileAccess file = Godot.FileAccess.Open(file_path, Godot.FileAccess.ModeFlags.Read);
        string json_dat = file.GetAsText();
        file.Close();
        if(json_dat == "") 
        {
            ChatController.DebugLog("Empty Json passed from: " + file_path);
            return new Godot.Collections.Dictionary();
        }
        return ParseJson(json_dat);
    }

    static public Godot.Collections.Dictionary ParseJson(string json_string)
    {
        // Nothing here...
        if(json_string == null || json_string == "") return new Godot.Collections.Dictionary();
        // cleanup 's
        json_string = json_string.Replace("'", "`");
        // Parse to dict
        Variant json_dat = Json.ParseString(json_string);
        Json jsonLoader = new Json();
        jsonLoader.Parse((string)json_dat);
        return (Godot.Collections.Dictionary)jsonLoader.Data;
    }

    public static string ApplyExistingTag(Godot.Collections.Dictionary data, string tag, string current_val)
    {
        if(data.ContainsKey(tag)) 
        {
            return data[tag].AsString();
        }
        return current_val;
    }

    public static bool ApplyExistingTag(Godot.Collections.Dictionary data, string tag, bool current_val)
    {
        if(data.ContainsKey(tag)) 
        {
            return data[tag].AsDouble() > 0;
        }
        return current_val;
    }

    public static float ApplyExistingTag(Godot.Collections.Dictionary data, string tag, float current_val)
    {
        if(data.ContainsKey(tag)) 
        {
            return (float)data[tag].AsDouble();
        }
        return current_val;
    }
    public static double ApplyExistingTag(Godot.Collections.Dictionary data, string tag, double current_val)
    {
        if(data.ContainsKey(tag)) 
        {
            return data[tag].AsDouble();
        }
        return current_val;
    }

    public static int ApplyExistingTag(Godot.Collections.Dictionary data, string tag, int current_val)
    {
        if(data.ContainsKey(tag)) 
        {
            return data[tag].AsInt32();
        }
        return current_val;
    }

    public static string[] ApplyExistingTag(Godot.Collections.Dictionary data, string tag, string[] current_val)
    {
        if(data.ContainsKey(tag)) 
        {
            return data[tag].AsStringArray();
        }
        return current_val;
    }

    public static void PrintProgress(int steps, int max_steps)
    {
        for(int b = 1; b <= 10; b++) 
        {
            if(steps == Mathf.Floor(Mathf.Floor(max_steps / 10) * b)) ChatController.DebugLog("-" + (b * 10) + "%");
        }
    }

    /*****************************************************************
     * Entity tools, things like adjacency checks etc
     ****************************************************************/
    public static bool Adjacent(AbstractEntity A,AbstractEntity B, bool ignore_corner_density)
    {
        // different maps, and depth doesn't count
        if(A.map_id_string != B.map_id_string || A.GridPos.dep != B.GridPos.dep) return false;
        MapController.GridPos A_pos = A.GridPos;
        MapController.GridPos B_pos = B.GridPos;
        Vector3 dir_vec;
        // center of turfs
        if(A is AbstractTurf || B is AbstractTurf)
        {
            A_pos.hor = Mathf.Floor(A_pos.hor) + 0.5f; 
            A_pos.ver = Mathf.Floor(A_pos.ver) + 0.5f; 
            B_pos.hor = Mathf.Floor(B_pos.hor) + 0.5f; 
            B_pos.ver = Mathf.Floor(B_pos.ver) + 0.5f; 
            dir_vec = DirVec(A_pos.WorldPos(),B_pos.WorldPos());
            if(!ignore_corner_density && DAT.DirIsDiagonal( DAT.VectorToDir(dir_vec.X,dir_vec.Y)))
            {
                // Check corner blockages
            }
            return Mathf.Abs(A.GridPos.hor - B_pos.hor) < 1 || Mathf.Abs(A.GridPos.ver - B_pos.ver) < 1;
        }

        // Entity checking
        dir_vec = DirVec(A_pos.WorldPos(),B_pos.WorldPos());
        if(!ignore_corner_density && DAT.DirIsDiagonal( DAT.VectorToDir(dir_vec.X,dir_vec.Y)))
        {
            // Check corner blockages
        }
        return TOOLS.VecDist(A_pos.WorldPos(),B_pos.WorldPos()) <= DAT.ADJACENT_DISTANCE;
    }

    public static DAT.Dir RotateTowardEntity(AbstractEntity A,AbstractEntity B)
    {
        if(A.map_id_string != B.map_id_string || B.GetLocation() is not AbstractTurf)
        {
            // ignore...
            return A.direction;
        }
        Vector3 offsetA = Vector3.Zero;
        Vector3 offsetB = Vector3.Zero;
        if(A is AbstractTurf) offsetA = new Vector3(0.5f,0,0.5f);
        if(B is AbstractTurf) offsetB = new Vector3(0.5f,0,0.5f);
        Vector3 dirvec = TOOLS.DirVec(A.GridPos.WorldPos() + offsetA,B.GridPos.WorldPos() + offsetB);
        // Final sanity check
        DAT.Dir ret = DAT.VectorToCardinalDir(dirvec.X,dirvec.Z);
        if(ret == DAT.Dir.None) return A.direction;
        return ret;
    }
}