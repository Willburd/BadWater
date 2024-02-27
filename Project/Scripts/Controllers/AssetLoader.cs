using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO; 

[GlobalClass] 
public partial class AssetLoader : Node
{
    public static Dictionary<string,MapData> loaded_maps = new Dictionary<string,MapData>();
    public static Dictionary<string,AreaData> loaded_areas = new Dictionary<string,AreaData>();
    public static Dictionary<string,TurfData> loaded_turfs = new Dictionary<string,TurfData>();

    public void Load()
    {
        GD.Print("LOADING ASSETS");
        string map_path = "res://Library/Maps";
        string area_path = "res://Library/Areas";
        string turf_path = "res://Library/Turfs";
        string struct_path = "res://Library/Struct";
        string item_path = "res://Library/Items";
        string mob_path = "res://Library/Mobs";

        GD.Print("-MAPS");
        DirAccess dir = DirAccess.Open(map_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    ParseMap(map_path + "/" + fileName);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-AREAS");
        dir = DirAccess.Open(area_path);
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




    private void ParseMap(string file_path)
    {
        Godot.Collections.Dictionary data = TOOLS.ParseJson(file_path);
        string prefix = Path.GetFileNameWithoutExtension(file_path);
        foreach( string key in data.Keys )
        {
            Godot.Collections.Dictionary map_data = (Godot.Collections.Dictionary)data[key];
            MapData map = new MapData();
            map.Init(prefix, key,map_data["name"].AsString(), (int)map_data["width"].AsDouble(), (int)map_data["height"].AsDouble(), (int)map_data["depth"].AsDouble());
            loaded_maps[map.GetUniqueID] = map;
        }
    }
    private void ParseArea(string file_path)
    {
        Godot.Collections.Dictionary data = TOOLS.ParseJson(file_path);
        string prefix = Path.GetFileNameWithoutExtension(file_path);
        foreach( string key in data.Keys )
        {
            Godot.Collections.Dictionary area_data = (Godot.Collections.Dictionary)data[key];
            AreaData area = new AreaData();
            area.Init(prefix, key,area_data["name"].AsString(), area_data["is_space"].AsDouble() > 0, area_data["always_powered"].AsDouble() > 0);
            loaded_areas[area.GetUniqueID] = area;
        }
    }
    private void ParseTurf(string file_path)
    {
        Godot.Collections.Dictionary data = TOOLS.ParseJson(file_path);
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
