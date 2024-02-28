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
    public static Dictionary<string,EffectData> loaded_effects = new Dictionary<string,EffectData>();

    public void Load()
    {
        GD.Print("LOADING ASSETS");
        string map_path = "res://Library/Maps";
        string area_path = "res://Library/Areas";
        string turf_path = "res://Library/Turfs";
        string struct_path = "res://Library/Struct";
        string item_path = "res://Library/Items";
        string effect_path = "res://Library/Effects";
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

        GD.Print("-TURFS");
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

        GD.Print("-EFFECTS");
        dir = DirAccess.Open(effect_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    ParseEffect(effect_path + "/" + fileName);
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

        GD.Print("-PREFABS");
        {   // Base area
            AreaData area = new AreaData();
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data["name"] = "Unknown";
            data["is_space"] = 1.0;
            data["always_powered"] = 1.0;
            area.Init( "_", "_", "_", data);
            loaded_areas[area.GetUniqueModID] = area;
        }
        {   // Space turf
            TurfData turf = new TurfData();
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data["name"] = "Space";
            data["density"] = 0.0;
            data["opaque"] = 0.0;
            turf.Init( "_", "_", "_", data);
            loaded_turfs[turf.GetUniqueModID] = turf;
        }
        {   // Fallback player spawn
            EffectData effect = new EffectData();
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data["name"] = "Player Spawn";
            data["spawner_id"] = "PLAYER";
            data["cleanable"] = 0.0;
            effect.Init( "_", "_", "PLAYER_SPAWN", data);
            loaded_effects[effect.GetUniqueModID] = effect;
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
            map.Init( file_path, prefix, key,map_data);
            loaded_maps[map.GetUniqueModID] = map;
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
            area.Init( file_path, prefix, key, area_data);
            loaded_areas[area.GetUniqueModID] = area;
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
            turf.Init( file_path, prefix, key,turf_data);
            loaded_turfs[turf.GetUniqueModID] = turf;
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

    private void ParseEffect(string file_path)
    {
        Godot.Collections.Dictionary data = TOOLS.ParseJson(file_path);
        string prefix = Path.GetFileNameWithoutExtension(file_path);
        foreach( string key in data.Keys )
        {
            Godot.Collections.Dictionary effect_data = (Godot.Collections.Dictionary)data[key];
            EffectData effect = new EffectData();
            effect.Init( file_path, prefix, key,effect_data);
            loaded_effects[effect.GetUniqueModID] = effect;
        }
    }

    private void ParseMob(string file_path)
    {
        GD.Print(file_path);
    }
}
