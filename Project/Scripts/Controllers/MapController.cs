using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection.Metadata;

public partial class MapController : DeligateController
{
    public static int tile_size = 1; // size in 3D units that world tiles are

    /*****************************************************************
     * MAP LOADING PHASES
     ****************************************************************/

    private List<MapLoader> loading = new List<MapLoader>();
    private List<MapInitilizer> initing = new List<MapInitilizer>();
    private List<MapLateInitilizer> iconupdating = new List<MapLateInitilizer>();
    private List<MapEntityCreator> entitycreating = new List<MapEntityCreator>();


    /*****************************************************************
     * CONTROLLER SETUP
     ****************************************************************/
    public static Dictionary<string,AbstractArea> areas = new Dictionary<string,AbstractArea>();
    public static List<AbstractEffect> effects = new List<AbstractEffect>();
    public static Dictionary<string,List<AbstractEffect>> spawners = new Dictionary<string,List<AbstractEffect>>();

    private static Dictionary<string,MapContainer> active_maps = new Dictionary<string,MapContainer>();

    public override bool CanInit()
    {
        return IsSubControllerInit(ChemController.controller);
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
                iconupdating.Add(new MapLateInitilizer(active_maps[loader.GetMapID()]));
            }
            initing.Clear();
            return;
        }
        // Late init, and UpdateIcons();
        if(iconupdating.Count > 0)
        {
            foreach(MapLateInitilizer iconing in iconupdating)
            {
                if(!iconing.Finished())
                {
                    finished = false;
                    iconing.Process();
                }
            }
            if(!finished) return;
            foreach(MapOperator loader in iconupdating)
            {
                entitycreating.Add(new MapEntityCreator(active_maps[loader.GetMapID()]));
            }
            iconupdating.Clear();
            return;
        }
        // Create entities!
        if(entitycreating.Count > 0)
        {
            foreach(MapEntityCreator creator in entitycreating)
            {
                if(!creator.Finished())
                {
                    finished = false;
                    creator.Process();
                }
            }
            if(!finished) return;
            entitycreating.Clear();
            return;
        }
        // Finalize
        InitEffects();
        InitEntities();
        FinishInit();
    }
    private void InitAreas()
    {
        GD.Print("INIT AREAS " + AssetLoader.loaded_areas.Count + " ------------------------------------------------");
        // Create all areas from resources
        foreach(KeyValuePair<string, AreaData> entry in AssetLoader.loaded_areas)
        {
            AbstractArea area = AbstractEntity.CreateEntity("_",entry.Value.GetUniqueModID,MainController.DataType.Area) as AbstractArea;
            areas[entry.Value.GetUniqueModID] = area;
            area.Init();
        }
    }
    private void InitEffects()
    {
        GD.Print("INIT EFFECTS " + effects.Count + " ------------------------------------------------");
        for(int i = 0; i < effects.Count; i++) 
        {
            AbstractTurf turf = effects[i].GetTurf();
            turf.EntityEntered(effects[i],false);
            effects[i].Init();
        }
        // Time for their graphical update too!
        for(int i = 0; i < effects.Count; i++) 
        {
            effects[i].LateInit();
            effects[i].UpdateIcon();
            if(effects[i].is_spawner)
            {
                string spawn_tag = effects[i].GetTag();
                if(!spawners.ContainsKey(spawn_tag))
                {
                    spawners[spawn_tag] = new List<AbstractEffect>();
                }
                GD.Print("-Added spawner, tag: " + spawn_tag);
                spawners[spawn_tag].Add(effects[i]);
            }
        }
    }
    private void InitEntities()
    {
        // Map controller handles the other controllers entity lists for this too, instead of spagetti. So those controllers can assume the Init() work has been done!
        List<AbstractEntity> all_entities = new List<AbstractEntity>();
        all_entities.AddRange(MapController.entities);
        all_entities.AddRange(MachineController.entities);
        all_entities.AddRange(MobController.entities);
        GD.Print("INIT ENTITIES " + all_entities.Count + " ------------------------------------------------");
        for(int i = 0; i < all_entities.Count; i++) 
        {
            // Directly add to turf's contents, we're still initting, no need to call Crossed() or Entered()
            AbstractTurf turf = all_entities[i].GetTurf();
            turf.EntityEntered(all_entities[i],false);
            turf.Init();
        }
        // Time for their graphical update too!
        for(int i = 0; i < all_entities.Count; i++) 
        {
            all_entities[i].LateInit();
            all_entities[i].UpdateIcon();
        }
    }


    /*****************************************************************
     * MAP MANAGEMENT
     ****************************************************************/
    public static bool IsMapLoaded(string mapID)
    {
        if(mapID == null) return false;
        return active_maps.ContainsKey(mapID);
    }
    public static string FallbackMap()
    {
        return active_maps.First().Key;
    }


    /*****************************************************************
     * CHUNK MANAGEMENT
     ****************************************************************/
    public static Dictionary<string,List<NetworkChunk>> GetAllMapChunks()
    {
        Dictionary<string,List<NetworkChunk>> ret = new Dictionary<string,List<NetworkChunk>>();
        foreach(KeyValuePair<string,MapContainer> entry in active_maps)
        {
            ret[entry.Key] = entry.Value.GetLoadedChunks();
        }
        return ret;
    }
    public static NetworkChunk[,,] GetLoadedChunkGrid(string mapID)
    {
        return active_maps[mapID].GetLoadedChunkGrid();
    }
    public static List<NetworkChunk> GetAllLoadedChunks()
    {
        List<NetworkChunk> all_loaded = new List<NetworkChunk>();
        foreach(MapContainer map in active_maps.Values)
        {
            all_loaded.AddRange(map.GetLoadedChunks());
        }
        return all_loaded;
    }
    public static List<NetworkChunk> GetMapLoadedChunks(string mapID)
    {
        return active_maps[mapID].GetLoadedChunks();
    }
    public static bool IsChunkValid(string mapID, ChunkPos chunk_pos)
    {
        return active_maps[mapID].IsChunkValid(chunk_pos);
    }
    public static bool IsChunkLoaded(string mapID, ChunkPos chunk_pos)
    {
        return active_maps[mapID].IsChunkLoaded(chunk_pos);
    }
    public static NetworkChunk GetChunk(string mapID, ChunkPos chunk_pos)
    {
        return active_maps[mapID].GetChunk(chunk_pos);
    }
    public static void ChunkUnload(NetworkChunk chunk)
    {
        active_maps[chunk.map_id_string].UnloadChunk(chunk);
    }


    /*****************************************************************
     * TURF MANAGEMENT
     ****************************************************************/
    public static bool IsTurfValid(string mapID, GridPos chunk_pos)
    {
        return active_maps[mapID].IsTurfValid(chunk_pos);
    }
    public static AbstractTurf AddTurf(string turfID, string mapID, GridPos grid_pos, AbstractArea area, bool replace, bool submaps)
    {
        return active_maps[mapID].AddTurf(turfID, grid_pos, area, replace, submaps);
    }
    public static void RemoveTurf(AbstractTurf turf, string mapID, bool make_area_baseturf, bool submaps)
    {
        active_maps[mapID].RemoveTurf(turf, make_area_baseturf, submaps);
    }
    public static void SwapTurfs(AbstractTurf old_turf, AbstractTurf new_turf, bool submaps)
    {
        string old_map = old_turf.map_id_string;
        GridPos old_pos = old_turf.GridPos;
        AbstractTurf buffer = active_maps[new_turf.map_id_string].SwapTurf(old_turf,new_turf.GridPos,submaps);
        active_maps[old_map].SwapTurf(buffer,old_pos,submaps);
    }
    public static AbstractTurf GetTurfAtPosition(string mapID, GridPos grid_pos, bool submaps)
    {
        return active_maps[mapID].GetTurfAtPosition(grid_pos,submaps);
    }

    
    /*****************************************************************
     * AREA MANAGEMENT
     ****************************************************************/
    public static AbstractArea GetAreaAtPosition(string mapID, GridPos grid_pos, bool submaps)
    {
        return active_maps[mapID].GetAreaAtPosition(grid_pos,submaps);
    }
    public static AbstractTurf GetTurfAtPosition(string mapID, Vector3 pos, bool submaps)
    {
        return active_maps[mapID].GetTurfAtPosition(new GridPos(pos),submaps);
    }
    public static AbstractArea GetAreaAtPosition(string mapID, Vector3 pos, bool submaps)
    {
        return active_maps[mapID].GetAreaAtPosition(new GridPos(pos),submaps);
    }


    /*****************************************************************
     * TAGGED OBJECT MANAGEMENT
     ****************************************************************/
    private static Dictionary<string,List<AbstractEntity>> tagged_abstracts = new Dictionary<string,List<AbstractEntity>>();

    // DO NOT CALL THESE DIRECTLY, CALL THE ENTITIES SetTag()/GetTag()!
    public static void Internal_UpdateTag(AbstractEntity ent, string new_tag)
    {
        string old_tag = ent.GetTag();
        if(old_tag == new_tag) return;
        if(new_tag != "") 
        {
            if(!tagged_abstracts.ContainsKey(new_tag)) tagged_abstracts.Add(new_tag, new List<AbstractEntity>());
            tagged_abstracts[new_tag].Add(ent);
        }
        if(old_tag != "") tagged_abstracts[old_tag].Remove(ent);
    }

    // NOTE: you should be calling both of these when checking for tagged objects!
    public static List<AbstractEntity> GetTaggedAbstracts(string tag)
    {
        if(!tagged_abstracts.ContainsKey(tag))
        {
            return new List<AbstractEntity>();
        }
        return tagged_abstracts[tag];
    }

    // Get random entity, then pick whatever one you want to use! Be sure to use BOTH!
    public static AbstractEntity GetRandomTaggedAbstract(string tag)
    {
        List<AbstractEntity> ents = GetTaggedAbstracts(tag);
        if(ents.Count == 0) return null;
        return tagged_abstracts[tag][TOOLS.RandI(tagged_abstracts[tag].Count)];
    }


    /*****************************************************************
     * GAME UPDATE
     ****************************************************************/
    public override void Fire()
    {
        //GD.Print(Name + " Fired");
        // All areas get their update call
        foreach(KeyValuePair<string, AbstractArea> entry in areas)
        {
            // do something with entry.Value or entry.Key
            entry.Value.Tick();
        }
    }
    public override void Shutdown()
    {
        
    }

    
    /*****************************************************************
     * SUPPORT OBJECTS
     ****************************************************************/
    public struct GridPos
    {
        public GridPos(float set_hor, float set_ver, float set_dep)
        {
            hor = set_hor;
            ver = set_ver;
            dep = set_dep;
        }
        public GridPos(Vector3 worldPos)
        {
            hor = (float)(worldPos.X / MapController.tile_size);
            ver = (float)(worldPos.Z / MapController.tile_size);
            dep = (float)(worldPos.Y / MapController.tile_size);
        }

        public readonly Vector3 WorldPos()
        {
            return new Vector3(hor * MapController.tile_size,dep * MapController.tile_size,ver * MapController.tile_size);
        }

        public readonly bool Equals(GridPos other)
        {
            return Mathf.FloorToInt(hor) == Mathf.FloorToInt(other.hor) && Mathf.FloorToInt(ver) == Mathf.FloorToInt(other.ver) && Mathf.FloorToInt(dep) == Mathf.FloorToInt(other.dep);
        }

        public readonly ChunkPos ChunkPos()
        {
            return new ChunkPos(WorldPos());
        }
        public float hor;
        public float ver;
        public float dep;
    }

    public struct ChunkPos
    {
        public ChunkPos(int set_hor, int set_ver, int set_dep)
        {
            hor = set_hor;
            ver = set_ver;
            dep = set_dep;
        }
        public ChunkPos(Vector3 worldPos)
        {
            hor = Mathf.FloorToInt(worldPos.X / (ChunkController.chunk_size * MapController.tile_size));
            ver = Mathf.FloorToInt(worldPos.Z / (ChunkController.chunk_size * MapController.tile_size));
            dep = Mathf.FloorToInt(worldPos.Y);
        }

        public readonly bool Equals(GridPos other)
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

        private List<NetworkChunk> loaded_chunks = new List<NetworkChunk>();
        private NetworkChunk[,,] chunk_grid;

        public MapContainer(string set_map_id,int set_width, int set_height,int set_depth)
        {
            // Primary data for server!
            map_id = set_map_id;
            width = set_width;
            height = set_height;
            depth = set_depth;
            turfs = new AbstractTurf[width,height,depth];
            // Chunks for clients!
            int chunk_wid = (int)Mathf.Ceil(width / ChunkController.chunk_size);
            int chunk_hig = (int)Mathf.Ceil(height / ChunkController.chunk_size);
            int chunk_dep = set_depth;
            chunk_grid = new NetworkChunk[chunk_wid,chunk_hig,chunk_dep];
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

        public AbstractTurf AddTurf(string turfID, GridPos grid_pos, AbstractArea area, bool replace, bool submaps)
        {
            // Replace old turf
            if(replace)
            {
                AbstractTurf check_turf = GetTurfAtPosition(grid_pos,true);
                if(check_turf != null)
                {
                    RemoveTurf(check_turf, false, submaps);
                }
            }
            // Spawn new turf
            AbstractTurf turf = AbstractEntity.CreateEntity(map_id, turfID, MainController.DataType.Turf) as AbstractTurf;
            SetTurfPosition(turf,grid_pos);
            area.AddTurf(turf);
            return turf;
        }
        public AbstractTurf SwapTurf(AbstractTurf turf, GridPos grid_pos, bool submaps) // returns the turf that SWAPPED with it!
        {
            // Replace old turf
            AbstractTurf check_turf = GetTurfAtPosition(grid_pos,submaps);
            // Clear old data
            if(check_turf != null)
            {
                GridPos old_pos = check_turf.GridPos;
                Internal_SetTurf(old_pos, null, submaps);
            }
            // Move new turf
            turf.map_id_string = map_id;
            SetTurfPosition(turf,grid_pos);
            return check_turf;
        }

        private void SetTurfPosition(AbstractTurf turf, GridPos grid_pos)
        {
            // Very dangerous function... Lets keep this internal, and only accessed by safe public calls!
            turf.Move( turf.map_id_string, grid_pos, false);
            Internal_SetTurf(grid_pos, turf, true);
        }

        public void RemoveTurf(AbstractTurf turf, bool make_area_baseturf, bool submaps)
        {
            // Remove from areas
            AbstractArea get_area = turf.Area;

            // Destroy turf in main lists
            GridPos grid_pos = turf.GridPos;
            if(make_area_baseturf)
            {
                // Spawn a new turf in the same spot to replace it...
                AddTurf(get_area.base_turf_ID, grid_pos,get_area,false,submaps);
            }
            else
            {
                // Or void it
                Internal_GetTurf(grid_pos, submaps).Kill();
                Internal_SetTurf(grid_pos, null, submaps);
            }
        }

        public AbstractTurf GetTurfAtPosition(GridPos grid_pos, bool submaps)
        {
            return Internal_GetTurf(grid_pos, submaps);
        }

        public AbstractArea GetAreaAtPosition(GridPos grid_pos, bool submaps)
        {
            return GetTurfAtPosition(grid_pos,submaps)?.Area;
        }



        // MAJOR TODO - handle submaps in maps,
        // Use a list of submaps injected into a map
        // Compare their rectangle to the current XY lookup
        // Return their tiles instead if on submaps mode!

        private AbstractTurf Internal_GetTurf(GridPos grid_pos, bool submaps)
        {
            if(!IsTurfValid(grid_pos)) return null; // handle submap's internal X and Y too... eventually
            return turfs[(int)grid_pos.hor,(int)grid_pos.ver,(int)grid_pos.dep];
        }

        private void Internal_SetTurf(GridPos grid_pos, AbstractTurf set, bool submaps)
        {
            turfs[(int)grid_pos.hor,(int)grid_pos.ver,(int)grid_pos.dep] = set;
        }

        // END major TODO!


        
        public void RandomTurfUpdate()
        {
            // Lower chance of random ticks heavily 
            if(TOOLS.Prob(80)) return;

            // Perform a random number of random turf updates
            int repeat = 5;
            while(repeat-- > 0)
            {
                int randx = TOOLS.RandI(width);
                int randy = TOOLS.RandI(height);
                int randz = TOOLS.RandI(depth);
                AbstractTurf turf = GetTurfAtPosition(new GridPos(randx,randy,randz),true);
                turf.RandomTick();
                turf.AtmosphericsCheck();
            }
        }
        
        public bool IsTurfValid(GridPos grid_pos)
        {
            // Assuming the chunk is already loaded is faster then trying to load nothing1
            if(grid_pos.hor < 0 || grid_pos.hor >= turfs.GetLength(0)) return false;
            if(grid_pos.ver < 0 || grid_pos.ver >= turfs.GetLength(1)) return false;
            if(grid_pos.dep < 0 || grid_pos.dep >= turfs.GetLength(2)) return false;
            return true;
        }

        public bool IsChunkValid(ChunkPos grid_pos)
        {
            // Assuming the chunk is already loaded is faster then trying to load nothing1
            if(grid_pos.hor < 0 || grid_pos.hor >= chunk_grid.GetLength(0)) return false;
            if(grid_pos.ver < 0 || grid_pos.ver >= chunk_grid.GetLength(1)) return false;
            if(grid_pos.dep < 0 || grid_pos.dep >= chunk_grid.GetLength(2)) return false;
            return true;
        }
        public bool IsChunkLoaded(ChunkPos grid_pos)
        {
            if(!IsChunkValid(grid_pos)) return false;
            return chunk_grid[grid_pos.hor,grid_pos.ver,grid_pos.dep] != null;
        }
        public NetworkChunk GetChunk(ChunkPos grid_pos)
        {
            NetworkChunk chunk = chunk_grid[grid_pos.hor,grid_pos.ver,grid_pos.dep];
            if(chunk != null) return chunk;
            // Loader...
            NetworkChunk new_chunk = NetworkEntity.CreateEntity(null, map_id, MainController.DataType.Chunk) as NetworkChunk;
            new_chunk.Position = TOOLS.ChunkGridToPos(grid_pos);
            chunk_grid[grid_pos.hor,grid_pos.ver,grid_pos.dep] = new_chunk;
            loaded_chunks.Add(new_chunk);
            return new_chunk;
        }
        public void UnloadChunk(NetworkChunk chunk)
        {
            ChunkPos chunk_pos = new ChunkPos(chunk.Position);
            if(chunk.Unload()) // Safer than just calling Kill() lets chunks decide some stuff if they should unload...
            {
                ChunkController.CleanChunk(chunk);
                chunk_grid[chunk_pos.hor,chunk_pos.ver,chunk_pos.dep] = null;
                loaded_chunks.Remove(chunk);
            }
        }
        public List<NetworkChunk> GetLoadedChunks()
        {
            return loaded_chunks;
        }
        public NetworkChunk[,,] GetLoadedChunkGrid()
        {
            return chunk_grid;
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

        protected virtual void HandleLoop()
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
                        turf_ylist = (Godot.Collections.Array<string[]>)turf_depth[current_x.ToString()]; // array of string[TurfID,CustomData]
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
                AbstractTurf turf = output_map.AddTurf(make_turf_id, new GridPos(current_x,current_y,current_z), areas[make_area_id], false, false);
                if(turf_json.Length > 0) turf.ApplyMapCustomData(TOOLS.ParseJson(turf_json)); // Set this object's flags using an embedded string of json!
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
            int repeats = 200;
            while(repeats-- > 0 && !finished)
            {
                GetTurfAtPosition(current_x, current_y, current_z).Init();
                HandleLoop();
            }
        }

        public AbstractTurf GetTurfAtPosition(int x, int y, int z)
        {
            return output_map.GetTurfAtPosition(new GridPos(x,y,z),false);
        }
    }


    private class MapLateInitilizer : MapOperator
    {
        public MapLateInitilizer(MapContainer input_map)
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
                AbstractTurf turf = GetTurfAtPosition(current_x, current_y, current_z);
                turf.LateInit();
                turf.UpdateIcon();
                HandleLoop();
            }
        }

        public AbstractTurf GetTurfAtPosition(int x, int y, int z)
        {
            return output_map.GetTurfAtPosition(new GridPos(x,y,z),false);
        }
    }



    private class MapEntityCreator : MapOperator
    {
        Godot.Collections.Array<string[]> item_data;
        Godot.Collections.Array<string[]> effect_data;
        Godot.Collections.Array<string[]> structure_data;
        Godot.Collections.Array<string[]> machine_data;

        public new int max_steps
        {
            get {return item_data.Count + effect_data.Count + structure_data.Count + machine_data.Count;}
        } 


        int phase = 0;
        public MapEntityCreator(MapContainer input_map)
        {
            map_id = input_map.MapID;
            output_map = input_map;

            MapData map_data = AssetLoader.loaded_maps[map_id];
            Godot.Collections.Dictionary map_list = TOOLS.ParseJsonFile(map_data.GetFilePath);
            Godot.Collections.Dictionary map_json = (Godot.Collections.Dictionary)map_list[map_data.GetUniqueID];
            item_data       = (Godot.Collections.Array<string[]>)map_json["items"]; // array of string[EntityID,X,Y,Z,CustomData]
            effect_data     = (Godot.Collections.Array<string[]>)map_json["effects"]; // array of string[EntityID,X,Y,Z,CustomData]
            structure_data  = (Godot.Collections.Array<string[]>)map_json["structures"]; // array of string[EntityID,X,Y,Z,CustomData]
            machine_data    = (Godot.Collections.Array<string[]>)map_json["machines"]; // array of string[EntityID,X,Y,Z,CustomData]
            GD.Print("CREATING ENTITIES " + map_id + " =========================");
        }

        public override void Process()
        {
            int repeats = 10;
            while(repeats-- > 0 && !finished)
            {
                // Get entity data!
                string[] entity_pack = null;
                AbstractEntity ent = null;
                switch(phase)
                {
                    case 0: // Item
                        if(item_data.Count > 0)
                        {
                            entity_pack = item_data[current_x];
                            ent = AbstractEntity.CreateEntity(map_id,entity_pack[0],MainController.DataType.Item);
                            if(entity_pack[4].Length > 0) ent.ApplyMapCustomData(TOOLS.ParseJson(entity_pack[4])); // Set this object's flags using an embedded string of json!
                        }
                    break;
                    case 1: // Effect
                        if(effect_data.Count > 0)
                        {
                            entity_pack = effect_data[current_x];
                            ent = AbstractEntity.CreateEntity(map_id,entity_pack[0],MainController.DataType.Effect);
                            if(entity_pack[4].Length > 0) ent.ApplyMapCustomData(TOOLS.ParseJson(entity_pack[4])); // Set this object's flags using an embedded string of json!
                        }
                    break;
                    case 2: // Structure
                        if(structure_data.Count > 0)
                        {
                            entity_pack = structure_data[current_x];
                            ent = AbstractEntity.CreateEntity(map_id,entity_pack[0],MainController.DataType.Structure);
                            if(entity_pack[4].Length > 0) ent.ApplyMapCustomData(TOOLS.ParseJson(entity_pack[4])); // Set this object's flags using an embedded string of json!
                        }
                    break;
                    case 3: // Machine
                        if(machine_data.Count > 0)
                        {
                            entity_pack = machine_data[current_x];
                            ent = AbstractEntity.CreateEntity(map_id,entity_pack[0],MainController.DataType.Machine);
                            if(entity_pack[4].Length > 0) ent.ApplyMapCustomData(TOOLS.ParseJson(entity_pack[4])); // Set this object's flags using an embedded string of json!
                        }
                    break;
                }
                // Set location
                if(ent != null) 
                {
                    ent.Move( ent.map_id_string, new GridPos(float.Parse(entity_pack[1]),float.Parse(entity_pack[2]),float.Parse(entity_pack[3])), false);
                }
                // LOOP!
                HandleLoop();
            }
        }
        protected override void HandleLoop()
        {
            // Next loop!
            steps += 1;
            current_x += 1;
            switch(phase)
            {
                case 0: // Item
                    if(current_x >= item_data.Count)
                    {
                        current_x = 0;
                        phase += 1;
                    }
                break;

                case 1: // Effect
                    if(current_x >= effect_data.Count)
                    {
                        current_x = 0;
                        phase += 1;
                    }
                break;

                case 2: // Structure
                    if(current_x >= structure_data.Count)
                    {
                        current_x = 0;
                        phase += 1;
                    }
                break;

                case 3: // Machine
                    if(current_x >= machine_data.Count)
                    {
                        current_x = 0;
                        phase += 1;
                        finished = true;
                    }
                break;
            }
            TOOLS.PrintProgress(steps,max_steps);
        }
    }
}