using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class MapController : DeligateController
{
    public static int TileSize = 2; // size in 3D units that world tiles are

    protected static List<NetworkTurf> all_turfs = new List<NetworkTurf>();
    public static Dictionary<string,NetworkTurf> turf_at_location = new Dictionary<string,NetworkTurf>();
    public static NetworkArea base_area = new NetworkArea();
    public static List<NetworkArea> areas = new List<NetworkArea>();

    public override bool CanInit()
    {
        return IsSubControllerInit(MachineController.controller);
    }

    public override bool Init()
    {
        tick_rate = 3;
        controller = this;
        InitMap("MAP",100,100,1); // TODO - Decode map data from mapper, when it exists
        InitAreas();
        InitTurfs();
        InitEntities();
        FinishInit();
        return true;
    }

    private void InitMap(string mapID,int width, int height,int depth)
    {
        // TODO - Load map tiles, and init them as NetworkTurfs in the proper locations!
        for(int h = 0; h < depth; h++) 
        {
            for(int i = 0; i < width; i++) 
            {
                for(int t = 0; t < height; t++) 
                {
                    NetworkArea area_inside = base_area;
                    // TODO - Solve for area that should be in this tile...
                    AddTurf(mapID, new Vector3(i,t,h), area_inside, false);
                }
            }
        }
    }

    private void InitAreas()
    {
        // Init the areas
        for(int i = 0; i < areas.Count; i++) 
        {
            areas[i].Init();
        }
        base_area.Init(); // base area too!
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
        for(int i = 0; i < areas.Count; i++) 
        {
            NetworkArea area = areas[i];
            area.Tick();
        }
        // Handle unsorted too
        base_area.Tick();
    }

    public override void Shutdown()
    {
        
    }

    public static NetworkTurf AddTurf(string mapID, Vector3 grid_pos, NetworkArea area, bool replace = true)
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
        NetworkTurf turf = NetworkEntity.CreateEntity(mapID,NetworkEntity.EntityType.Turf) as NetworkTurf;
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
            AddTurf(mapID,grid_pos,get_area,false);
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