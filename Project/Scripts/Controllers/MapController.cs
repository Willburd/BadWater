using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class MapController : DeligateController
{
    public static int TileSize = 1; // size in 3D units that world tiles are


    public static Dictionary<string,NetworkArea> areas = new Dictionary<string,NetworkArea>();
    public static List<NetworkEffect> all_effects = new List<NetworkEffect>();
    public static Dictionary<string,List<NetworkEffect>> spawners = new Dictionary<string,List<NetworkEffect>>();

    private static Dictionary<string,MapContainer> active_maps = new Dictionary<string,MapContainer>();


    public override bool CanInit()
    {
        return IsSubControllerInit(MachineController.controller);
    }

    private List<MapLoader> loading = new List<MapLoader>();
    private List<MapInitilizer> initing = new List<MapInitilizer>();
    private List<MapIconUpdater> iconupdating = new List<MapIconUpdater>();
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
            loading.Add(new MapLoader(map_id,map_data.width,map_data.height,map_data.depth));
        }
        return true;
    }

    public override void SetupTick()
    {
        // Process loading maps
        bool finished = true;
        if(loading.Count > 0)
        {
            foreach(MapOperator loader in loading)
            {
                if(!loader.Finished())
                {
                    finished = false;
                    loader.Process();
                }
            }
            if(!finished) return;
            foreach(MapOperator loader in loading)
            {
                active_maps[loader.GetMapID()] = loader.GetMap();
                initing.Add(new MapInitilizer(active_maps[loader.GetMapID()]));
            }
            loading.Clear();
            return;
        }
        // Time to initilize!
        if(initing.Count > 0)
        {
            foreach(MapInitilizer init in initing)
            {
                if(!init.Finished())
                {
                    finished = false;
                    init.Process();
                }
            }
            if(!finished) return;
            foreach(MapOperator loader in initing)
            {
                iconupdating.Add(new MapIconUpdater(active_maps[loader.GetMapID()]));
            }
            initing.Clear();
            return;
        }
        // Time to update first time icons!
        if(iconupdating.Count > 0)
        {
            foreach(MapIconUpdater iconing in iconupdating)
            {
                if(!iconing.Finished())
                {
                    finished = false;
                    iconing.Process();
                }
            }
            if(!finished) return;
            iconupdating.Clear();
            return;
        }
        
        // Finalize
        InitEffects();
        InitEntities();
        FinishInit();
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
            AbstractTurf turf = all_entities[i].GetTurf();
            turf.Init();
            turf.EntityEntered(all_entities[i],false);
        }
        // Time for their graphical update too!
        for(int i = 0; i < all_entities.Count; i++) 
        {
            all_entities[i].UpdateIcon();
        }
    }


    public static AbstractTurf AddTurf(string turfID, string mapID, GridPos grid_pos, NetworkArea area, bool replace = true)
    {
        return active_maps[mapID].AddTurf(turfID, grid_pos, area, replace);
    }
    public static void RemoveTurf(AbstractTurf turf, string mapID, bool make_area_baseturf = true)
    {
        active_maps[mapID].RemoveTurf(turf, make_area_baseturf);
    }
    public static void SwapTurfs(AbstractTurf old_turf, AbstractTurf new_turf)
    {
        string old_map = old_turf.map_id_string;
        GridPos old_pos = old_turf.GetGridPosition();
        AbstractTurf buffer = active_maps[new_turf.map_id_string].SwapTurf(old_turf,new_turf.GetGridPosition());
        active_maps[old_map].SwapTurf(buffer,old_pos);
    }


    public static AbstractTurf GetTurfAtPosition(string mapID, GridPos grid_pos)
    {
        return active_maps[mapID].GetTurfAtPosition(grid_pos);
    }
    public static NetworkArea GetAreaAtPosition(string mapID, GridPos grid_pos)
    {
        return active_maps[mapID].GetAreaAtPosition(grid_pos);
    }
    public static AbstractTurf GetTurfAtPosition(string mapID, Vector3 pos)
    {
        return active_maps[mapID].GetTurfAtPosition(new GridPos(pos));
    }
    public static NetworkArea GetAreaAtPosition(string mapID, Vector3 pos)
    {
        return active_maps[mapID].GetAreaAtPosition(new GridPos(pos));
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

    public struct GridPos
    {
        public GridPos(int set_hor, int set_ver, int set_dep)
        {
            hor = set_hor;
            ver = set_ver;
            dep = set_dep;
        }
        public GridPos(Vector3 worldPos)
        {
            hor = (int)(worldPos.X / MapController.TileSize);
            ver = (int)(worldPos.Z / MapController.TileSize);
            dep = (int)(worldPos.Y / MapController.TileSize);
        }

        public bool Equals(GridPos other)
        {
            return hor == other.hor && ver == other.ver && dep == other.dep;
        }

        public int hor;
        public int ver;
        public int dep;
    }

    private class MapContainer
    {
        private AbstractTurf[,,] turfs;
        private string map_id;
        private int width;
        private int height;
        private int depth;

        public float draw_offset_hor = 0;
        public float draw_offset_vert = 0;

        public MapContainer(string set_map_id,int set_width, int set_height,int set_depth)
        {
            map_id = set_map_id;
            width = set_width;
            height = set_height;
            depth = set_depth;
            turfs = new AbstractTurf[width,height,depth];
        }

        public string MapID
        {
            get {return map_id;}
        } 
        public int Width
        {
            get {return width;}
        } 
        public int Height
        {
            get {return height;}
        } 
        public int Depth
        {
            get {return depth;}
        } 
        public AbstractTurf AddTurf(string turfID, GridPos grid_pos, NetworkArea area, bool replace = true)
        {
            // Replace old turf
            if(replace)
            {
                AbstractTurf check_turf = GetTurfAtPosition(grid_pos);
                if(check_turf != null)
                {
                    RemoveTurf(check_turf, false);
                }
            }
            // Spawn new turf
            AbstractTurf turf = AbstractEntity.CreateEntity(map_id, turfID, MainController.DataType.Turf) as AbstractTurf;
            SetTurfPosition(turf,grid_pos);
            area.AddTurf(turf);
            return turf;
        }
        public AbstractTurf SwapTurf(AbstractTurf turf, GridPos grid_pos) // returns the turf that SWAPPED with it!
        {
            // Replace old turf
            AbstractTurf check_turf = GetTurfAtPosition(grid_pos);
            // Clear old data
            if(check_turf != null)
            {
                GridPos old_pos = check_turf.GetGridPosition();
                turfs[old_pos.hor,old_pos.ver,old_pos.dep] = null;
            }
            // Move new turf
            turf.map_id_string = map_id;
            SetTurfPosition(turf,grid_pos);
            return check_turf;
        }

        private void SetTurfPosition(AbstractTurf turf, GridPos grid_pos)
        {
            // Very dangerous function... Lets keep this internal, and only accessed by safe public calls!
            turf.Position = TOOLS.GridToPos(grid_pos);
            turfs[grid_pos.hor,grid_pos.ver,grid_pos.dep] = turf;
        }

        public void RemoveTurf(AbstractTurf turf, bool make_area_baseturf = true)
        {
            // Remove from areas
            NetworkArea get_area = turf.Area;

            // Destroy turf in main lists
            GridPos grid_pos = turf.GetGridPosition();
            if(make_area_baseturf)
            {
                // Spawn a new turf in the same spot to replace it...
                AddTurf(get_area.base_turf_ID, grid_pos,get_area,false);
            }
            else
            {
                // Or void it
                turfs[grid_pos.hor,grid_pos.ver,grid_pos.dep] = null;
            }
        }

        public AbstractTurf GetTurfAtPosition(GridPos grid_pos)
        {
            return turfs[grid_pos.hor,grid_pos.ver,grid_pos.dep];
        }

        public NetworkArea GetAreaAtPosition(GridPos grid_pos)
        {
            return turfs[grid_pos.hor,grid_pos.ver,grid_pos.dep].Area;
        }
        
        public void RandomTurfUpdate()
        {
            // Lower chance of random ticks heavily 
            if((Mathf.Abs((int)GD.Randi()) % 100) < 80) return;

            // Perform a random number of random turf updates
            int repeat = 5;
            while(repeat-- > 0)
            {
                int randx = Mathf.Abs((int)GD.Randi()) % width;
                int randy = Mathf.Abs((int)GD.Randi()) % height;
                int randz = Mathf.Abs((int)GD.Randi()) % depth;
                AbstractTurf turf = turfs[randx,randy,randz];
                turf.RandomTick();
                turf.AtmosphericsCheck();
            }
        }
    }


    private class MapOperator
    {
        protected MapContainer output_map;
        public int max_steps
        {
            get {return output_map.Depth * output_map.Width * output_map.Height;}
        } 

        protected string map_id;
        protected int steps = 0; // for logging
        protected int current_x = 0;
        protected int current_y = 0;
        protected int current_z = 0;

        protected void HandleLoop()
        {
            // Next loop!
            steps += 1;
            current_x += 1;
            if(current_x >= output_map.Width)
            {
                current_x = 0;
                current_y += 1;
            }
            if(current_y >= output_map.Height)
            {
                current_y = 0;
                current_z += 1;
            }
            if(current_z >= output_map.Depth)
            {
                finished = true;
            }
            TOOLS.PrintProgress(steps,max_steps);
        }

        public virtual void Process()
        {
            // replace with controlled functions!
            HandleLoop();
        }

        protected bool finished = false;
        public bool Finished()
        {
            return finished;
        }


        public string GetMapID()
        {
            return map_id;
        }
        public MapContainer GetMap()
        {
            return output_map;
        }
    }

    private class MapLoader : MapOperator
    {
        Godot.Collections.Dictionary area_data;
        Godot.Collections.Dictionary turf_data;

        public MapLoader(string set_map_id,int set_width, int set_height,int set_depth)
        {
            map_id = set_map_id;
            MapData map_data = AssetLoader.loaded_maps[set_map_id];
            output_map = new MapContainer(set_map_id,set_width, set_height,set_depth);

            Godot.Collections.Dictionary map_list = TOOLS.ParseJsonFile(map_data.GetFilePath);
            Godot.Collections.Dictionary map_json = (Godot.Collections.Dictionary)map_list[map_data.GetUniqueID];
            area_data = (Godot.Collections.Dictionary)map_json["area_data"];
            turf_data = (Godot.Collections.Dictionary)map_json["turf_data"];
            GD.Print("LOADING MAP" + map_id + " =========================");
        }

        public override void Process()
        {
            if(finished) return;

            Godot.Collections.Dictionary area_depth = null;
            if(area_data.ContainsKey(current_z.ToString())) area_depth = (Godot.Collections.Dictionary)area_data[current_z.ToString()];
            Godot.Collections.Dictionary turf_depth = null;
            if(turf_data.ContainsKey(current_z.ToString())) turf_depth = (Godot.Collections.Dictionary)turf_data[current_z.ToString()];

            int repeats = 100;
            while(repeats-- > 0 && !finished)
            {
                string make_area_id = "_:_";
                string make_turf_id = "_:_";
                string turf_json = "";
                if(area_depth != null && turf_depth != null)
                {
                    // MUST not be an empty Z level, these should NEVER be invalid on a real map file. So it's probably an empty one... Paint a fresh map!
                    // Assume data will overrun the buffer, and provide dummy lists
                    string[] area_ylist = new string[]{"_:_"};
                    if(area_depth.ContainsKey(current_x.ToString())) 
                    {
                        area_ylist = area_depth[current_x.ToString()].AsStringArray();
                    }
                    Godot.Collections.Array<string[]> turf_ylist = new Godot.Collections.Array<string[]>{ new string[] { "_:_", "" }};
                    if(area_depth.ContainsKey(current_x.ToString())) 
                    {
                        turf_ylist = (Godot.Collections.Array<string[]>)turf_depth[current_x.ToString()];
                    }

                    if(current_y < area_ylist.Length)
                    {
                        make_area_id = area_ylist[current_y];
                    }
                    if(current_y < turf_ylist.Count)
                    {
                        string[] construct_strings = turf_ylist[current_y];
                        make_turf_id = construct_strings[0]; // Set ID
                        turf_json = construct_strings[1];
                    }
                }

                // It's turfin time... How awful.
                AbstractTurf turf = output_map.AddTurf(make_turf_id, new GridPos(current_x,current_y,current_z), areas[make_area_id], false);
                turf.ApplyMapCustomData(TOOLS.ParseJson(turf_json)); // Set this object's flags using an embedded string of json!
                HandleLoop();
            }
        }
    }


    private class MapInitilizer : MapOperator
    {
        public MapInitilizer(MapContainer input_map)
        {
            map_id = input_map.MapID;
            output_map = input_map;
            GD.Print("INITING MAP" + map_id + " =========================");
        }

        public override void Process()
        {
            int repeats = 100;
            while(repeats-- > 0 && !finished)
            {
                GetTurfAtPosition(current_x, current_y, current_z).Init();
                HandleLoop();
            }
        }

        public AbstractTurf GetTurfAtPosition(int x, int y, int z)
        {
            return output_map.GetTurfAtPosition(new GridPos(x,y,z));
        }
    }


    private class MapIconUpdater : MapOperator
    {
        public MapIconUpdater(MapContainer input_map)
        {
            map_id = input_map.MapID;
            output_map = input_map;
            GD.Print("UPDATING MAP" + map_id + " =========================");
        }

        public override void Process()
        {
            int repeats = 100;
            while(repeats-- > 0 && !finished)
            {
                GetTurfAtPosition(current_x, current_y, current_z).UpdateIcon();
                HandleLoop();
            }
        }

        public AbstractTurf GetTurfAtPosition(int x, int y, int z)
        {
            return output_map.GetTurfAtPosition(new GridPos(x,y,z));
        }
    }
}