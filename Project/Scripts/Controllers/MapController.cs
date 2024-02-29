using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class MapController : DeligateController
{
    public static int TileSize = 1; // size in 3D units that world tiles are

    protected static List<NetworkTurf> all_turfs = new List<NetworkTurf>();
    public static Dictionary<string,NetworkTurf> turf_at_location = new Dictionary<string,NetworkTurf>();
    public static Dictionary<string,NetworkArea> areas = new Dictionary<string,NetworkArea>();
    public static List<NetworkEffect> all_effects = new List<NetworkEffect>();
    public static Dictionary<string,List<NetworkEffect>> spawners = new Dictionary<string,List<NetworkEffect>>();

    public override bool CanInit()
    {
        return IsSubControllerInit(MachineController.controller);
    }

    public override bool Init()
    {
        tick_rate = 3;
        controller = this;
        InitAreas();
        // For each map loaded, init them!
        string[] loaded = MainController.controller.config.loaded_maps;
        for(int i = 0; i < loaded.Length; i++) 
        {
            string map_id = loaded[i];
            if(!AssetLoader.loaded_maps.ContainsKey(map_id)) continue;
            MapData map_data = AssetLoader.loaded_maps[map_id];
            GD.Print("-Loading map: " + map_data.display_name);
            InitMap(map_id,map_data.width,map_data.height,map_data.depth);
        }
        InitTurfs();
        InitEffects();
        InitEntities();
        FinishInit();
        return true;
    }

    private void InitMap(string map_id,int width, int height,int depth)
    {
        MapData map_data = AssetLoader.loaded_maps[map_id];
        Godot.Collections.Dictionary map_list = TOOLS.ParseJsonFile(map_data.GetFilePath);
        Godot.Collections.Dictionary map_json = (Godot.Collections.Dictionary)map_list[map_data.GetUniqueID];
        Godot.Collections.Dictionary area_data = (Godot.Collections.Dictionary)map_json["area_data"];
        Godot.Collections.Dictionary turf_data = (Godot.Collections.Dictionary)map_json["turf_data"];
        int steps = 0;
        int max_steps = depth * width * height;
        for(int h = 0; h < depth; h++) 
        {
            Godot.Collections.Dictionary area_depth = null;
            if(area_data.ContainsKey(h.ToString())) area_depth = (Godot.Collections.Dictionary)area_data[h.ToString()];
            Godot.Collections.Dictionary turf_depth = null;
            if(turf_data.ContainsKey(h.ToString())) turf_depth = (Godot.Collections.Dictionary)turf_data[h.ToString()];

            if(area_depth == null || turf_data == null)
            {
                // Empty Z, full fill of empty tiles
                for(int i = 0; i < height; i++) 
                {
                    for(int t = 0; t < width; t++) 
                    {
                        NetworkTurf turf = AddTurf("_:_",map_id, new Vector3(t,h,i), areas["_:_"], false);
                        TOOLS.PrintProgress(steps, max_steps);
                        steps += 1;
                    }
                }
                continue;
            }

            for(int i = 0; i < height; i++) 
            {
                // Assume data will overrun the buffer, and provide dummy lists
                string[] area_ylist = new string[]{"_:_"};
                if(area_depth.ContainsKey(i.ToString())) 
                {
                    area_ylist = area_depth[i.ToString()].AsStringArray();
                }
                Godot.Collections.Array<string[]> turf_ylist = new Godot.Collections.Array<string[]>{ new string[] { "_:_", "" }};
                if(area_depth.ContainsKey(i.ToString())) 
                {
                    turf_ylist = (Godot.Collections.Array<string[]>)turf_depth[i.ToString()];
                }
                // Create turfs!
                for(int t = 0; t < width; t++) 
                {
                    // Base data...
                    string area_id = "_:_";
                    string make_turf_id = "_:_";
                    string embedded_json = "";
                    // Load from current map!
                    if(t < area_ylist.Length)
                    {
                        area_id = area_ylist[t];
                    }
                    if(t < turf_ylist.Count)
                    {
                        string[] construct_strings = turf_ylist[t];
                        make_turf_id = construct_strings[0]; // Set ID
                        embedded_json = construct_strings[1];
                    }
                    // It's turfin time... How awful.
                    NetworkTurf turf = AddTurf(make_turf_id,map_id, new Vector3(t,h,i), areas[area_id], false);
                    turf.ApplyMapCustomData(TOOLS.ParseJson(embedded_json)); // Set this object's flags using an embedded string of json!
                    TOOLS.PrintProgress(steps, max_steps);
                    steps += 1;
                }
            }
        }
    }

    private void InitAreas()
    {
        // Create all areas from resources
        foreach(KeyValuePair<string, AreaData> entry in AssetLoader.loaded_areas)
        {
            NetworkArea area = NetworkEntity.CreateEntity("_",entry.Value.GetUniqueModID,MainController.DataType.Area) as NetworkArea;
            areas[entry.Value.GetUniqueModID] = area;
            area.Init();
        }
        // Init the areas now that they are all prepared
        foreach(KeyValuePair<string, NetworkArea> entry in areas)
        {
            entry.Value.Init();
        }
    }

    private void InitTurfs()
    {
        // Setup all turfs...
        for(int i = 0; i < all_turfs.Count; i++) 
        {
            all_turfs[i].Init();
        }
        // And then give them all a graphics update, lets do this heavy stuff here to get it over with.
        // Instead of making objects figure it out in the middle of their Init()... Which would get goofy because other stuff might not be init.
        for(int i = 0; i < all_turfs.Count; i++) 
        {
            all_turfs[i].UpdateIcon();
        }
    }

    private void InitEffects()
    {
        for(int i = 0; i < all_effects.Count; i++) 
        {
            all_effects[i].Init();
            if(all_effects[i].spawner_id != "")
            {
                if(!spawners.ContainsKey(all_effects[i].spawner_id))
                {
                    spawners[all_effects[i].spawner_id] = new List<NetworkEffect>();
                }
                spawners[all_effects[i].spawner_id].Add(all_effects[i]);
            }
        }
    }

    private void InitEntities()
    {
        // Map controller handles the other controllers entity lists for this too, instead of spagetti. So those controllers can assume the Init() work has been done!
        List<NetworkEntity> all_entities = new List<NetworkEntity>();
        all_entities.AddRange(MapController.entities);
        all_entities.AddRange(MachineController.entities);
        all_entities.AddRange(MobController.entities);
        for(int i = 0; i < all_entities.Count; i++) 
        {
            // Directly add to turf's contents, we're still initting, no need to call Crossed() or Entered()
            NetworkTurf turf = all_entities[i].GetTurf();
            turf.Init();
            turf.EntityEntered(all_entities[i],false);
        }
        // Time for their graphical update too!
        for(int i = 0; i < all_entities.Count; i++) 
        {
            all_entities[i].UpdateIcon();
        }
    }


    public override void Fire()
    {
        //GD.Print(Name + " Fired");
        // All areas get their update call
        foreach(KeyValuePair<string, NetworkArea> entry in areas)
        {
            // do something with entry.Value or entry.Key
            entry.Value.Tick();
        }
    }

    public override void Shutdown()
    {
        
    }

    public static NetworkTurf AddTurf(string turfID, string mapID, Vector3 grid_pos, NetworkArea area, bool replace = true)
    {
        // Replace old turf
        if(replace)
        {
            NetworkTurf check_turf = GetTurfAtPosition(mapID,grid_pos);
            if(check_turf != null)
            {
                RemoveTurf(check_turf, false);
            }
        }
        // Spawn new turf
        NetworkTurf turf = NetworkEntity.CreateEntity(mapID,turfID,MainController.DataType.Turf) as NetworkTurf;
        turf.SetGridPosition(grid_pos);
        area.AddTurf(turf);
        all_turfs.Add(turf);
        return turf;
    }

    public static void RemoveTurf(NetworkTurf turf, bool make_area_baseturf = true)
    {
        // Get data for later
        string mapID = turf.map_id_string;
        NetworkArea get_area = turf.Area;
        Vector3 grid_pos = turf.GetGridPosition();

        // Remove from area
        get_area.turfs.Remove(turf);
        
        // Destroy turf in main lists
        all_turfs.Remove(turf);
        if(make_area_baseturf)
        {
            // Spawn a new turf in the same spot to replace it...
            // TODO - area base turf definitions from mapper
            AddTurf(get_area.base_turf_ID, mapID,grid_pos,get_area,false);
        }
        else
        {
            // Or void it
            turf_at_location[FormatWorldPosition(mapID,grid_pos)] = null;
        }
    }


    public static string FormatWorldPosition(string mapID,Vector3 pos)      // NetworkTurfs are stored in a dictionary for fast lookup, based on their map as well as XYZ. Endless submaps ahoy!
    {
        return mapID + ":" + pos.X + ":" + pos.Y + ":" + pos.Z;
    }

    public static NetworkTurf GetTurfAtPosition(string mapID, Vector3 pos)
    {
        string loc = FormatWorldPosition(mapID,pos);
        NetworkTurf get;
        if(turf_at_location.TryGetValue(loc, out get))
        {
            return get;
        }
        return null;
    }

    public static NetworkArea GetAreaAtPosition(string mapID, Vector3 pos)
    {
        NetworkTurf turf = GetTurfAtPosition(mapID, pos);
        if(turf == null) return null;
        return turf.Area;
    }
}