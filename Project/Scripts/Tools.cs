using Godot;
using System;

public static class TOOLS
{
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
            GD.Print("Missing file: " + file_path);
            return new Godot.Collections.Dictionary();
        }
        Godot.FileAccess file = Godot.FileAccess.Open(file_path, Godot.FileAccess.ModeFlags.Read);
        string json_dat = file.GetAsText();
        file.Close();
        if(json_dat == "") 
        {
            GD.Print("Empty Json passed from: " + file_path);
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
            if(steps == Mathf.Floor(Mathf.Floor(max_steps / 10) * b)) GD.Print("-" + (b * 10) + "%");
        }
    }
}