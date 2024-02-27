using Godot;
using System;

static class TOOLS
{
    static public Godot.Collections.Dictionary ParseJson(string file_path)
    {
        // Read text from file
        Godot.FileAccess file = Godot.FileAccess.Open(file_path, Godot.FileAccess.ModeFlags.Read);
        Variant json_dat = Json.ParseString(file.GetAsText());
        file.Close();
        // Parse to dict
        Json jsonLoader = new Json();
        jsonLoader.Parse((string)json_dat);
        return (Godot.Collections.Dictionary)jsonLoader.Data;
    }
}