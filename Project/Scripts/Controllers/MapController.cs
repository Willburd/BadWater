using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class MapController : DeligateController
{
    public static int TileSize = 2; // size in 3D units that world tiles are

    protected static List<NetworkTurf> all_turfs = new List<NetworkTurf>();
    public static Dictionary<string,NetworkTurf> turf_at_location = new Dictionary<string,NetworkTurf>();
    public static AreaNetworkData base_area = new AreaNetworkData();
    public static List<AreaNetworkData> areas = new List<AreaNetworkData>();

    public override bool CanInit()
    {
        return true;
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
                    AreaNetworkData area_inside = base_area;
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
        for(int i = 0; i < all_turfs.Count; i++) 
        {
            all_turfs[i].Init();
        }
    }

    private void InitEntities()
    {
        // Directly add to turf's contents, we're still initting, no need to call Crossed() or Entered()
        // Map controller handles the other controllers setup for this too, instead of spagetti. So those controllers can assume the work is done!
        for(int i = 0; i < entities.Count; i++) 
        {
            NetworkTurf turf = entities[i].GetTurf();
            turf.EntityEntered(entities[i],false);
        }
        for(int i = 0; i < MachineController.entities.Count; i++) 
        {
            NetworkTurf turf = entities[i].GetTurf();
            turf.EntityEntered(entities[i],false);
        }
        for(int i = 0; i < MobController.entities.Count; i++) 
        {
            NetworkTurf turf = entities[i].GetTurf();
            turf.EntityEntered(entities[i],false);
        }
    }


    public override void Fire()
    {
        //GD.Print(Name + " Fired");
        // All areas get their update call
        for(int i = 0; i < areas.Count; i++) 
        {
            AreaNetworkData area = areas[i];
            area.Tick();
        }
        // Handle unsorted too
        base_area.Tick();
    }

    public override void Shutdown()
    {
        
    }

    public static NetworkTurf AddTurf(string mapID, Vector3 grid_pos, AreaNetworkData area, bool replace = true)
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
        AreaNetworkData get_area = turf.Area;
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

    public static AreaNetworkData GetAreaAtPosition(string mapID, Vector3 pos)
    {
        NetworkTurf turf = GetTurfAtPosition(mapID, pos);
        if(turf == null) return null;
        return turf.Area;
    }
}

public class AreaNetworkData
{
    public string name = "None";
    bool always_powered;
    bool is_space;

    public List<NetworkTurf> turfs = new List<NetworkTurf>();

    public void Init()
    {
        // setup area's data from the resource script
    }

    public void Tick()
    {
        RandomTurfUpdate(); // Randomly update some turfs, some types of turfs do things when random ticked, atmo also likes these.
    }

    public void AddTurf(NetworkTurf turf)
    {
        // Remove from other areas
        if(turf.Area != null)
        {
            turf.Area.turfs.Remove(turf);
        }
        // Make ours!
        turf.Area = this;
        turfs.Add(turf);
    }

    public void RandomTurfUpdate()
    {
        // Lower chance of random ticks heavily 
        if(turfs.Count == 0) return;
        if((Mathf.Abs((int)GD.Randi()) % 100) < 80) return;
        // Perform a random number of random turf updates
        int repeat = Mathf.Clamp(Mathf.Abs((int)GD.Randi()) % Mathf.Max((int)(turfs.Count / 50),1), 1, turfs.Count);
        while(repeat-- > 0)
        {
            int check = Mathf.Abs((int)GD.Randi()) % turfs.Count;
            NetworkTurf turf = turfs[check];
            turf.RandomTick();
            turf.AtmosphericsCheck();
        }
    }
}