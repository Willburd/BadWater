using Godot;
using System;
using System.Collections.Generic;

public class MapContainer
{
    private AbstractTurf[,,] turfs;
    private string map_id;
    private int width;
    private int height;
    private int depth;

    // XY location map is at, when submapped into another map!
    private List<string> loaded_submaps = new List<string>();
    public GridPos submap_pos = new GridPos("NULL",0,0,0);

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
            if(check_turf != null) RemoveTurf(check_turf, false, submaps);
        }
        // Spawn new turf
        AbstractTurf turf = AbstractTools.CreateEntity(MainController.DataType.Turf,turfID,null, true) as AbstractTurf;
        SetTurfPosition(turf,grid_pos,submaps);
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
        SetTurfPosition(turf,grid_pos,submaps);
        return check_turf;
    }

    private void SetTurfPosition(AbstractTurf turf, GridPos grid_pos, bool submaps)
    {
        // Very dangerous function... Lets keep this internal, and only accessed by safe public calls!
        AbstractTools.Move(turf, grid_pos, false);
        Internal_SetTurf(grid_pos, turf, submaps);
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
            AbstractTools.DeleteEntity( Internal_GetTurf(grid_pos, submaps) );
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



    private AbstractTurf Internal_GetTurf(GridPos grid_pos, bool submaps)
    {
        if(!IsTurfValid(grid_pos)) return null;
        if(submaps)
        {
            foreach(string map_id in loaded_submaps)
            {
                MapContainer map = MapController.GetMap(map_id);
                if(grid_pos.hor >= map.submap_pos.hor && grid_pos.hor < map.submap_pos.hor + map.Width 
                && grid_pos.ver >= map.submap_pos.ver && grid_pos.ver < map.submap_pos.ver + map.Height
                && grid_pos.dep >= map.submap_pos.dep && grid_pos.dep < map.submap_pos.dep + map.Depth)
                {
                    return map.Internal_GetTurf(new GridPos(map_id,grid_pos.hor-map.submap_pos.hor,grid_pos.ver-map.submap_pos.ver,grid_pos.dep-map.submap_pos.dep),true);
                }
            }
        }
        int hor = Mathf.FloorToInt(grid_pos.hor);
        int ver = Mathf.FloorToInt(grid_pos.ver);
        int dep = Mathf.FloorToInt(grid_pos.dep);
        return turfs[hor,ver,dep];
    }

    private void Internal_SetTurf(GridPos grid_pos, AbstractTurf set, bool submaps)
    {
        if(!IsTurfValid(grid_pos)) return;
        if(submaps)
        {
            foreach(string map_id in loaded_submaps)
            {
                MapContainer map = MapController.GetMap(map_id);
                if(grid_pos.hor >= map.submap_pos.hor && grid_pos.hor < map.submap_pos.hor + map.Width 
                && grid_pos.ver >= map.submap_pos.ver && grid_pos.ver < map.submap_pos.ver + map.Height
                && grid_pos.dep >= map.submap_pos.dep && grid_pos.dep < map.submap_pos.dep + map.Depth)
                {
                    map.Internal_SetTurf(new GridPos(map_id,grid_pos.hor-map.submap_pos.hor,grid_pos.ver-map.submap_pos.ver,grid_pos.dep-map.submap_pos.dep),set,true);
                    return;
                }
            }
        }
        int hor = Mathf.FloorToInt(grid_pos.hor);
        int ver = Mathf.FloorToInt(grid_pos.ver);
        int dep = Mathf.FloorToInt(grid_pos.dep);
        turfs[hor,ver,dep] = set;
    }


    
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
            AbstractTurf turf = GetTurfAtPosition(new GridPos(map_id,randx,randy,randz),false);
            turf.RandomTick();
            turf.AtmosphericsCheck();
        }
    }
    
    public bool IsTurfValid(GridPos grid_pos)
    {
        // Assuming the chunk is already loaded is faster then trying to load nothing!
        if(grid_pos.GetMapID() == "NULL" || grid_pos.GetMapID() == "BAG") return false;
        if(grid_pos.hor < 0 || grid_pos.hor >= turfs.GetLength(0)) return false;
        if(grid_pos.ver < 0 || grid_pos.ver >= turfs.GetLength(1)) return false;
        if(grid_pos.dep < 0 || grid_pos.dep >= turfs.GetLength(2)) return false;
        return true;
    }

    public bool IsChunkValid(ChunkPos grid_pos)
    {
        // Assuming the chunk is already loaded is faster then trying to load nothing!
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
        NetworkChunk new_chunk = NetworkEntity.CreateEntity(null, MainController.DataType.Chunk, map_id) as NetworkChunk;
        new_chunk.Position = TOOLS.ChunkGridToPos(grid_pos);
        chunk_grid[grid_pos.hor,grid_pos.ver,grid_pos.dep] = new_chunk;
        loaded_chunks.Add(new_chunk);
        return new_chunk;
    }
    public void UnloadChunk(NetworkChunk chunk)
    {
        ChunkPos chunk_pos = new ChunkPos(chunk.Position);
        if(chunk.CanUnload()) // Safer than just calling DeleteEntity() lets chunks decide some stuff if they should unload...
        {
            ChunkController.CleanChunk(chunk);
            chunk_grid[chunk_pos.hor,chunk_pos.ver,chunk_pos.dep] = null;
            loaded_chunks.Remove(chunk);
            chunk.DeleteEntity();
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

    public bool HasSubmap(string checkmap)
    {
        return loaded_submaps.Contains(checkmap);
    }

    public List<string> GetSubmapList()
    {
        return loaded_submaps;
    }
}