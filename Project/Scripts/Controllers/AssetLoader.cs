using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO; 

[GlobalClass] 
public partial class AssetLoader : Node
{
    public static Dictionary<string,AreaData> loaded_areas = new Dictionary<string,AreaData>();
    public static Dictionary<string,TurfData> loaded_turfs = new Dictionary<string,TurfData>();

    public void Load()
    {
        GD.Print("LOADING ASSETS");
        string area_path = "res://Library/Areas";
        string turf_path = "res://Library/Turfs";
        string struct_path = "res://Library/Struct";
        string item_path = "res://Library/Items";
        string mob_path = "res://Library/Mobs";

        GD.Print("-AREAS");
        DirAccess dir = DirAccess.Open(area_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    ParseArea(area_path + "/" + fileName);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-TURFs");
        dir = DirAccess.Open(turf_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    ParseTurf(turf_path + "/" + fileName);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-STRUCTURES");
        dir = DirAccess.Open(struct_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    ParseStructure(struct_path + "/" + fileName);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-ITEMS");
        dir = DirAccess.Open(item_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    ParseItem(item_path + "/" + fileName);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-MOBS");
        dir = DirAccess.Open(mob_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    ParseMob(mob_path + "/" + fileName);
                }
                fileName = dir.GetNext();
            }
        }
    }



    
    private Godot.Collections.Dictionary ParseJson(string file_path)
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


    private void ParseArea(string file_path)
    {
        Godot.Collections.Dictionary data = ParseJson(file_path);
        string prefix = Path.GetFileNameWithoutExtension(file_path);
        foreach( string key in data.Keys )
        {
            Godot.Collections.Dictionary turf_data = (Godot.Collections.Dictionary)data[key];
            AreaData area = new AreaData();
            area.Init(prefix, key,turf_data["name"].AsString(), turf_data["is_space"].AsDouble() > 0, turf_data["always_powered"].AsDouble() > 0);
            loaded_areas[area.GetUniqueID] = area;
        }
    }
    private void ParseTurf(string file_path)
    {
        Godot.Collections.Dictionary data = ParseJson(file_path);
        string prefix = Path.GetFileNameWithoutExtension(file_path);
        foreach( string key in data.Keys )
        {
            Godot.Collections.Dictionary turf_data = (Godot.Collections.Dictionary)data[key];
            TurfData turf = new TurfData();
            turf.Init(prefix, key,turf_data["name"].AsString(), turf_data["density"].AsDouble() > 0, turf_data["opaque"].AsDouble() > 0);
            loaded_turfs[turf.GetUniqueID] = turf;
        }
    }

    private void ParseStructure(string file_path)
    {
        GD.Print(file_path);
    }

    private void ParseItem(string file_path)
    {
        GD.Print(file_path);
    }

    private void ParseMob(string file_path)
    {
        GD.Print(file_path);
    }
}
