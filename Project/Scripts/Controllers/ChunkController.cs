using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class ChunkController : DeligateController
{
    public static ChunkController controller;    // Singleton reference for each controller, mostly used during setup to check if controller has init.
    public ChunkController()
    {
        controller = this;
    }

    public static int chunk_load_range = 3;

    public static int chunk_size = 5; // Size in turfs that chunks are


    public override bool CanInit()
    {
        return IsSubControllerInit(MapController.controller); // waiting on the map controller first
    }

    public override bool Init()
    {
        display_name = "Chunk";
        tick_rate = 10;
        return true;
    }

    public override void SetupTick()
    {
        FinishInit();
    }

    public override void Fire()
    {
        //GD.Print(Name + " Fired");

        // Emergency respawns
        List<NetworkClient> client_list = MainController.ClientList;
        for(int i = 0; i < client_list.Count; i++) 
		{
			NetworkClient client = client_list[i];
            if(!client.has_logged_in) continue; // Skip!
            
            // Run thread
            if(!MapController.IsMapLoaded(client.focused_map_id))
            {
                GD.PrintErr("CLIENT " + client.Name + " ON UNLOADED MAP " + client.focused_map_id);
                client.Spawn(); // EMERGENCY RESPAWN
            }
        }

        // Handle loading!
        for(int i = 0; i < client_list.Count; i++) 
		{
			NetworkClient client = client_list[i];
            if(!client.has_logged_in) continue; // Skip!
            int id = int.Parse(client.Name);

            // hor/ver distance
            int max_chunk_loads = 6;
            float loadborder_w = chunk_load_range;
            float loadborder_h = chunk_load_range;
            for(int u = 0; u < loadborder_w * 2; u++) 
            {
                for(int v = 0; v < loadborder_h * 2; v++) 
                {
                    // Load our chunks
                    ChunkPos pos = new ChunkPos(client.focused_position - new Vector3((loadborder_w/4) * MapController.tile_size * ChunkController.chunk_size,0,(loadborder_h/4) * MapController.tile_size * ChunkController.chunk_size));
                    pos.hor -= Mathf.FloorToInt(loadborder_w/2);
                    pos.ver -= Mathf.FloorToInt(loadborder_h/2);
                    pos.hor += u;
                    pos.ver += v;
                    if(IsChunkValid(client.focused_map_id,pos)) 
                    {
                        bool is_loaded = IsChunkLoaded(client.focused_map_id,pos);
                        if(is_loaded) 
                        {
                            NetworkChunk chunk = GetChunk(client.focused_map_id,pos);
                            chunk.visible_to_peers.Add(id);
                        }
                        else if(max_chunk_loads > 0) // Limit chunk loads per client to avoid hitching from getting 12+ chunks at the same time.
                        {
                            NetworkChunk chunk = GetChunk(client.focused_map_id,pos);
                            ChunkController.SetupChunk(chunk);
                            chunk.visible_to_peers.Add(id);
                            max_chunk_loads -= 1;
                        }
                    }
                }
            }
		}
        
        // Unload chunks out of range, randomly pick candidates every frame and unload them
        List<NetworkChunk> loaded_chunks = GetAllLoadedChunks();
        int unload_count = Math.Min(loaded_chunks.Count, 20);
        while(unload_count-- > 0 && loaded_chunks.Count > 0)
        {
            NetworkChunk chunk = TOOLS.Pick(loaded_chunks);
            if(chunk.visible_to_peers.Count <= 0) ChunkUnload(chunk);
        }
        
        // Reset chunk visibility while ticking all active chunks!
        Dictionary<string,List<NetworkChunk>> map_chunks = GetAllMapChunks();
        foreach(List<NetworkChunk> chunks in map_chunks.Values)
        {
            foreach(NetworkChunk chunk in chunks)
            {
                chunk.Tick();
                chunk.multi_syncronizer.UpdateVisibility();
                chunk.visible_to_peers.Clear();
            }
        }
    }

    public static Vector3 GetAlignedPos(Vector3 world_pos)
    {
        float alignsize = ChunkController.chunk_size * MapController.tile_size;
        world_pos = new Vector3(Mathf.FloorToInt(world_pos.X / alignsize) * alignsize, Mathf.FloorToInt(world_pos.Y), Mathf.FloorToInt(world_pos.Z / alignsize) * alignsize);
        return world_pos;
    }

    public override void Shutdown()
    {
        
    }

    public static void NewClient(NetworkClient client)
    {
        List<NetworkChunk> loaded_chunks = GetAllLoadedChunks();
        foreach(NetworkChunk chunk in loaded_chunks)
        {
            SetupChunk(chunk);
        }
        foreach(NetworkEntity ent in MainController.controller.entity_container.GetChildren())
        {
            if(ent is not NetworkChunk) ent.SetUpdatedPosition();
        }
    }


    /*****************************************************************
     * CHUNK MANAGEMENT
     ****************************************************************/
    public static void SetupChunk(NetworkChunk chunk)
    {
        GridPos pos = new GridPos(chunk.map_id_string,GetAlignedPos(chunk.Position));
        for(int u = 0; u < ChunkController.chunk_size; u++) 
        {
            for(int v = 0; v < ChunkController.chunk_size; v++) 
            {
                float hor = pos.hor + u;
                float ver = pos.ver + v;
                AbstractTurf turf = MapController.GetMap(chunk.map_id_string).GetTurfAtPosition(new GridPos(chunk.map_id_string,hor,ver,pos.dep),true);
                turf.UpdateIcon();  // Build mesh!
                foreach(AbstractEntity ent in turf.Contents)
                {
                    ent.UpdateIcon();
                }
            }
        }
        // Force initial graphical state
        chunk.MeshUpdate();
        chunk.Tick();
    }
    public static void CleanChunk(NetworkChunk chunk)
    {
        GridPos pos = new GridPos(chunk.map_id_string,GetAlignedPos(chunk.Position));
        for(int u = 0; u < ChunkController.chunk_size; u++) 
        {
            for(int v = 0; v < ChunkController.chunk_size; v++) 
            {
                float hor = pos.hor + u;
                float ver = pos.ver + v;
                AbstractTurf turf = MapController.GetMap(chunk.map_id_string).GetTurfAtPosition(new GridPos(chunk.map_id_string,hor,ver,pos.dep),true);
                foreach(AbstractEntity ent in turf.Contents)
                {
                    ent.UnloadNetworkEntity();
                }
            }
        }
    }
    public static Dictionary<string,List<NetworkChunk>> GetAllMapChunks()
    {
        Dictionary<string,List<NetworkChunk>> ret = new Dictionary<string,List<NetworkChunk>>();
        foreach(KeyValuePair<string,MapContainer> entry in MapController.GetLoadedMaps())
        {
            ret[entry.Key] = entry.Value.GetLoadedChunks();
        }
        return ret;
    }
    public static NetworkChunk[,,] GetLoadedChunkGrid(string mapID)
    {
        return MapController.GetMap(mapID).GetLoadedChunkGrid();
    }
    public static List<NetworkChunk> GetAllLoadedChunks()
    {
        List<NetworkChunk> all_loaded = new List<NetworkChunk>();
        foreach(MapContainer map in MapController.GetLoadedMaps().Values)
        {
            all_loaded.AddRange(map.GetLoadedChunks());
        }
        return all_loaded;
    }
    public static List<NetworkChunk> GetMapLoadedChunks(string mapID)
    {
        return MapController.GetMap(mapID).GetLoadedChunks();
    }
    public static bool IsChunkValid(string mapID, ChunkPos chunk_pos)
    {
        return MapController.GetMap(mapID).IsChunkValid(chunk_pos);
    }
    public static bool IsChunkLoaded(string mapID, ChunkPos chunk_pos)
    {
        return MapController.GetMap(mapID).IsChunkLoaded(chunk_pos);
    }
    public static NetworkChunk GetChunk(string mapID, ChunkPos chunk_pos)
    {
        return MapController.GetMap(mapID).GetChunk(chunk_pos);
    }
    public static void ChunkUnload(NetworkChunk chunk)
    {
        MapController.GetMap(chunk.map_id_string).UnloadChunk(chunk);
    }
}
