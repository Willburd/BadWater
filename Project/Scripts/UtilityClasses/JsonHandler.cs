using Godot;
using System;

public static class JsonHandler
{
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
}
