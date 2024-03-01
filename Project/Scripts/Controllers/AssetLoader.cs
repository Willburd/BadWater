using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public struct PackRef
{
    public PackRef(PackData data)
    {
        modid = data.GetUniqueModID;
        data_type = data.GetType().ToString();
    }
    public string modid;
    public string data_type;
}

[GlobalClass] 
public partial class AssetLoader : Node
{
    public static Dictionary<string,MapData> loaded_maps = new Dictionary<string,MapData>();
    public static Dictionary<string,AreaData> loaded_areas = new Dictionary<string,AreaData>();
    public static Dictionary<string,TurfData> loaded_turfs = new Dictionary<string,TurfData>();
    public static Dictionary<string,EffectData> loaded_effects = new Dictionary<string,EffectData>();
    public static Dictionary<string,PackData> loaded_items = new Dictionary<string,PackData>(); // TEMP
    public static Dictionary<string,PackData> loaded_structures = new Dictionary<string,PackData>(); // TEMP
    public static Dictionary<string,PackData> loaded_machines = new Dictionary<string,PackData>(); // TEMP
    public static Dictionary<string,PackData> loaded_mobs = new Dictionary<string,PackData>(); // TEMP

    public static Dictionary<string,PackData> all_packs = new Dictionary<string,PackData>();

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
                    GD.Print("-" + fileName);
                    ParseData(map_path + "/" + fileName, MainController.DataType.Map);
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
                    GD.Print("-" + fileName);
                    ParseData(area_path + "/" + fileName, MainController.DataType.Area);
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
                    GD.Print("-" + fileName);
                    ParseData(turf_path + "/" + fileName, MainController.DataType.Turf);
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
                    GD.Print("-" + fileName);
                    ParseData(struct_path + "/" + fileName, MainController.DataType.Structure);
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
                    GD.Print("-" + fileName);
                    ParseData(item_path + "/" + fileName, MainController.DataType.Item);
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
                    GD.Print("-" + fileName);
                    ParseData(effect_path + "/" + fileName, MainController.DataType.Effect);
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
                    GD.Print("-" + fileName);
                    ParseData(mob_path + "/" + fileName, MainController.DataType.Mob);
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
            all_packs[area.GetType()+":"+area.GetUniqueModID] = area;
        }
        {   // Space turf
            TurfData turf = new TurfData();
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data["name"] = "Space";
            data["density"] = 0.0;
            data["opaque"] = 0.0;
            turf.Init( "_", "_", "_", data);
            loaded_turfs[turf.GetUniqueModID] = turf;
            all_packs[turf.GetType()+":"+turf.GetUniqueModID] = turf;
        }
        {   // Fallback player spawn
            EffectData effect = new EffectData();
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data["name"] = "Player Spawn";
            data["spawner_id"] = "PLAYER";
            data["cleanable"] = 0.0;
            effect.Init( "_", "_", "PLAYER_SPAWN", data);
            loaded_effects[effect.GetUniqueModID] = effect;
            all_packs[effect.GetType()+":"+effect.GetUniqueModID] = effect;
        }
        
        GD.Print("BUILDING INHERITANCE");
        BuildInheritance(loaded_maps.Values.ToList<PackData>()); // Could be possible, but why would you?
        BuildInheritance(loaded_areas.Values.ToList<PackData>());
        BuildInheritance(loaded_turfs.Values.ToList<PackData>());
        BuildInheritance(loaded_effects.Values.ToList<PackData>());
        BuildInheritance(loaded_items.Values.ToList<PackData>());
        BuildInheritance(loaded_structures.Values.ToList<PackData>());
        BuildInheritance(loaded_machines.Values.ToList<PackData>());
        BuildInheritance(loaded_mobs.Values.ToList<PackData>());

        GD.Print("CLEANUP");
        foreach( PackData data in all_packs.Values )
        {
            data.ClearTempData();
            data.ShowVars();
        }
    }

    public void BuildInheritance(List<PackData> todo)
    {
        List<PackData> next_todo = new List<PackData>();
        while(todo.Count > 0)
        {
            foreach(PackData data in todo)
            {
                // Check if this is a base type. It'll already have it's parent flag set!
                if(data.ParentFlag)
                {
                    data.SetVars();
                    data.ShowVars();
                    continue;
                }
                // IT HAS A PARENT, so time for GREAT FUN.
                string getID = data.GetType().ToString() + ":" + data.GetDataParent;
                PackData parent = all_packs[getID];
                if(!parent.ParentFlag)
                {
                    // Parent is not yet ready...
                    next_todo.Add(data); // lets wait for a future loop...
                    continue;
                }
                // Parent's data is set, loop through parent chain, and set data repeatedly...
                Stack<string> parent_chain = new Stack<string>();
                string parent_search = data.GetType().ToString() + ":" + data.GetUniqueModID;
                while(true)
                {
                    PackData search_parent = all_packs[parent_search];
                    parent_chain.Push(parent_search);
                    if(search_parent.GetDataParent == "") break;
                    parent_search = data.GetType().ToString() + ":" + search_parent.GetDataParent;
                }
                // Go through all collected parents from the lowest to ourselves, and set values!
                while(parent_chain.Count > 0)
                {
                    PackData parent_step = all_packs[parent_chain.Pop()];
                    data.SetVars(parent_step.GetTempData()); // apply from parent
                }
                data.ParentFlag = true; // we're set!
                data.ShowVars();
            }
            // Next iteration
            todo = next_todo;
            next_todo = new List<PackData>();
        }
    }

    private void ParseData(string file_path, MainController.DataType type)
    {
        Godot.Collections.Dictionary data = TOOLS.ParseJsonFile(file_path);
        string prefix = Path.GetFileNameWithoutExtension(file_path);
        foreach( string key in data.Keys )
        {
            Godot.Collections.Dictionary dict_data = (Godot.Collections.Dictionary)data[key];
            switch(type)
            {
                case MainController.DataType.Map:
                    {
                        MapData data_pack = new MapData();
                        data_pack.Init( file_path, prefix, key,dict_data);
                        loaded_maps[data_pack.GetUniqueModID] = data_pack;
                        all_packs[data_pack.GetType()+":"+data_pack.GetUniqueModID] = data_pack;
                    }
                break;

                case MainController.DataType.Area:
                    {
                        AreaData data_pack = new AreaData();
                        data_pack.Init( file_path, prefix, key,dict_data);
                        loaded_areas[data_pack.GetUniqueModID] = data_pack;
                        all_packs[data_pack.GetType()+":"+data_pack.GetUniqueModID] = data_pack;
                    }
                break;

                case MainController.DataType.Turf:
                    {
                        TurfData data_pack = new TurfData();
                        data_pack.Init( file_path, prefix, key,dict_data);
                        loaded_turfs[data_pack.GetUniqueModID] = data_pack;
                        all_packs[data_pack.GetType()+":"+data_pack.GetUniqueModID] = data_pack;
                    }
                break;

                case MainController.DataType.Effect:
                    {
                        EffectData data_pack = new EffectData();
                        data_pack.Init( file_path, prefix, key,dict_data);
                        loaded_effects[data_pack.GetUniqueModID] = data_pack;
                        all_packs[data_pack.GetType()+":"+data_pack.GetUniqueModID] = data_pack;
                    }       
                break;

                case MainController.DataType.Item:
                    {
                        ItemData data_pack = new ItemData();
                        data_pack.Init( file_path, prefix, key,dict_data);
                        loaded_items[data_pack.GetUniqueModID] = data_pack;
                        all_packs[data_pack.GetType()+":"+data_pack.GetUniqueModID] = data_pack;
                    }
                break;
                
                case MainController.DataType.Structure:
                    {
                        PackData data_pack = new PackData();
                        data_pack.Init( file_path, prefix, key,dict_data);
                        loaded_structures[data_pack.GetUniqueModID] = data_pack;
                        all_packs[data_pack.GetType()+":"+data_pack.GetUniqueModID] = data_pack;
                    }
                break;
                
                case MainController.DataType.Machine:
                    {
                        PackData data_pack = new PackData();
                        data_pack.Init( file_path, prefix, key,dict_data);
                        loaded_machines[data_pack.GetUniqueModID] = data_pack;
                        all_packs[data_pack.GetType()+":"+data_pack.GetUniqueModID] = data_pack;
                    }
                break;
                
                case MainController.DataType.Mob:
                    {
                        PackData data_pack = new PackData();
                        data_pack.Init( file_path, prefix, key,dict_data);
                        loaded_mobs[data_pack.GetUniqueModID] = data_pack;
                        all_packs[data_pack.GetType()+":"+data_pack.GetUniqueModID] = data_pack;
                    }
                break;
            }
        }
    }

    private void ParseMob(string file_path)
    {
        GD.Print(file_path);
    }



    public static PackData GetPackFromModID(PackRef get_pack)
    {
        return all_packs[get_pack.data_type + ":" + get_pack.modid];
    }
}
