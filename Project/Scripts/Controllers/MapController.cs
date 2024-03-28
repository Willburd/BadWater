using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection.Metadata;

public partial class MapController : DeligateController
{
    public static MapController controller;    // Singleton reference for each controller, mostly used during setup to check if controller has init.
	public MapController()
    {
        controller = this;
    }

    public const float screen_visible_range = 9; // max range for chat and entity vision checks

    public static int tile_size = 1; // size in 3D units that world tiles are

    /*****************************************************************
     * MAP LOADING PHASES
     ****************************************************************/

    private List<MapHelpers.MapLoader> loading = new List<MapHelpers.MapLoader>();
    private List<MapHelpers.MapInitilizer> initing = new List<MapHelpers.MapInitilizer>();
    private List<MapHelpers.MapLateInitilizer> iconupdating = new List<MapHelpers.MapLateInitilizer>();
    private List<MapHelpers.MapEntityCreator> entitycreating = new List<MapHelpers.MapEntityCreator>();


    /*****************************************************************
     * CONTROLLER SETUP
     ****************************************************************/
    public Dictionary<string,AbstractArea> areas = new Dictionary<string,AbstractArea>();
    public List<AbstractEffect> effects = new List<AbstractEffect>();
    public Dictionary<string,List<AbstractEffect>> spawners = new Dictionary<string,List<AbstractEffect>>();

    private static Dictionary<string,MapContainer> active_maps = new Dictionary<string,MapContainer>();

    public override bool CanInit()
    {
        return IsSubControllerInit(ChemController.controller);
    }

    public override bool Init()
    {
        display_name = "Map";
        tick_rate = 3;
        InitAreas();
        // For each map loaded, init them!
        string[] loaded = MainController.controller.config.loaded_maps;
        for(int i = 0; i < loaded.Length; i++) 
        {
            string map_id = loaded[i];
            if(!AssetLoader.loaded_maps.ContainsKey(map_id)) continue;
            MapData map_data = AssetLoader.loaded_maps[map_id];
            ChatController.DebugLog("-Loading map: " + map_data.display_name);
            loading.Add(new MapHelpers.MapLoader(this,map_id,map_data.width,map_data.height,map_data.depth));
        }
        return true;
    }

    public override void SetupTick()
    {
        // Process loading maps
        bool finished = true;
        if(loading.Count > 0)
        {
            foreach(MapHelpers.MapOperator loader in loading)
            {
                if(!loader.Finished())
                {
                    finished = false;
                    loader.Process();
                }
            }
            if(!finished) return;
            foreach(MapHelpers.MapOperator loader in loading)
            {
                active_maps[loader.GetMapID()] = loader.GetMap();
                initing.Add(new MapHelpers.MapInitilizer(this,active_maps[loader.GetMapID()]));
            }
            loading.Clear();
            return;
        }
        // Time to initilize!
        if(initing.Count > 0)
        {
            foreach(MapHelpers.MapInitilizer init in initing)
            {
                if(!init.Finished())
                {
                    finished = false;
                    init.Process();
                }
            }
            if(!finished) return;
            foreach(MapHelpers.MapOperator loader in initing)
            {
                iconupdating.Add(new MapHelpers.MapLateInitilizer(this,active_maps[loader.GetMapID()]));
            }
            initing.Clear();
            return;
        }
        // Late init, and UpdateIcons();
        if(iconupdating.Count > 0)
        {
            foreach(MapHelpers.MapLateInitilizer iconing in iconupdating)
            {
                if(!iconing.Finished())
                {
                    finished = false;
                    iconing.Process();
                }
            }
            if(!finished) return;
            foreach(MapHelpers.MapOperator loader in iconupdating)
            {
                entitycreating.Add(new MapHelpers.MapEntityCreator(this,active_maps[loader.GetMapID()]));
            }
            iconupdating.Clear();
            return;
        }
        // Create entities!
        if(entitycreating.Count > 0)
        {
            foreach(MapHelpers.MapEntityCreator creator in entitycreating)
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
        ChatController.DebugLog("INIT AREAS " + AssetLoader.loaded_areas.Count + " ------------------------------------------------");
        // Create all areas from resources
        foreach(KeyValuePair<string, AreaData> entry in AssetLoader.loaded_areas)
        {
            AbstractArea area = AbstractEntity.CreateEntity(MainController.DataType.Area,entry.Value.GetUniqueModID,null, true) as AbstractArea;
            areas[entry.Value.GetUniqueModID] = area;
            area.Init();
        }
    }
    private void InitEffects()
    {
        ChatController.DebugLog("INIT EFFECTS " + effects.Count + " ------------------------------------------------");
        for(int i = 0; i < effects.Count; i++) 
        {
            AbstractTurf turf = effects[i].GetTurf();
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
                ChatController.DebugLog("-Added spawner, tag: " + spawn_tag);
                spawners[spawn_tag].Add(effects[i]);
            }
        }
    }
    private void InitEntities()
    {
        // Map controller handles the other controllers entity lists for this too, instead of spagetti. So those controllers can assume the Init() work has been done!
        List<AbstractEntity> all_entities = new List<AbstractEntity>();
        all_entities.AddRange(MapController.controller.entities);
        all_entities.AddRange(MachineController.controller.entities);
        all_entities.AddRange(MobController.controller.entities);
        ChatController.DebugLog("INIT ENTITIES " + all_entities.Count + " ------------------------------------------------");
        for(int i = 0; i < all_entities.Count; i++) 
        {
            // Directly add to turf's contents, we're still initting, no need to call Crossed() or Entered()
            AbstractTurf turf = all_entities[i].GetTurf();
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
    public static Dictionary<string,MapContainer> GetLoadedMaps()
    {
        return active_maps;
    }
    public static MapContainer GetMap(string mapID)
    {
        return active_maps[mapID];
    }
    public static void SetMap(string mapID, MapContainer new_map)
    {
        active_maps[mapID] = new_map;
    }


    /*****************************************************************
     * AREA MANAGEMENT
     ****************************************************************/
    public static AbstractArea GetAreaAtPosition(GridPos grid_pos, bool submaps)
    {
        return active_maps[grid_pos.GetMapID()].GetAreaAtPosition(grid_pos,submaps);
    }
    public static AbstractTurf GetTurfAtPosition(string mapID, Vector3 pos, bool submaps)
    {
        return active_maps[mapID].GetTurfAtPosition(new GridPos(mapID,pos),submaps);
    }
    public static AbstractArea GetAreaAtPosition(string mapID, Vector3 pos, bool submaps)
    {
        return active_maps[mapID].GetAreaAtPosition(new GridPos(mapID,pos),submaps);
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
            entry.Value.Tick();
        }
    }
    public override void Shutdown()
    {
        
    }
}