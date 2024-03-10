using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkController : DeligateController
{
    public static int chunk_load_range = 5;

    public static int chunk_size = 5; // Size in turfs that chunks are


    public override bool CanInit()
    {
        return IsSubControllerInit(MapController.controller); // waiting on the map controller first
    }

    public override bool Init()
    {
        tick_rate = 2;
        controller = this;
        return true;
    }

    public override void SetupTick()
    {
        FinishInit();
    }

    public override void Fire()
    {
        //GD.Print(Name + " Fired");
        Dictionary<string,List<NetworkChunk>> map_chunks = MapController.GetAllMapChunks();
        foreach(List<NetworkChunk> chunks in map_chunks.Values)
        {
            foreach(NetworkChunk chunk in chunks)
            {
                // Handle ticking all loaded chunks!
                chunk.Tick();
            }
        }
        // Handle unloading!
        List<NetworkChunk> in_vis_range = new List<NetworkChunk>();
        List<NetworkClient> client_list = MainController.ClientList;
        for(int i = 0; i <client_list.Count; i++) 
		{
			NetworkClient client = client_list[i];
            if(!client.has_logged_in) continue; // Skip!
            if(!MapController.IsMapLoaded(client.focused_map_id))
            {
                GD.PrintErr("CLIENT " + client.Name + " ON UNLOADED MAP " + client.focused_map_id);
                client.Spawn(); // EMERGENCY RESPAWN
            }

            // hor/ver distance
            float loadborder_w = chunk_load_range;
            float loadborder_h = chunk_load_range;
            for(int u = 0; u < loadborder_w * 2; u++) 
            {
                for(int v = 0; v < loadborder_h * 2; v++) 
                {
                    // Load our chunks
                    MapController.ChunkPos pos = new MapController.ChunkPos(client.focused_position - new Vector3((loadborder_w/4) * MapController.tile_size * ChunkController.chunk_size,0,(loadborder_h/4) * MapController.tile_size * ChunkController.chunk_size));
                    pos.hor -= Mathf.FloorToInt(loadborder_w/2);
                    pos.ver -= Mathf.FloorToInt(loadborder_h/2);
                    pos.hor += u;
                    pos.ver += v;
                    if(MapController.IsChunkValid(client.focused_map_id,pos)) 
                    {
                        bool is_loaded = MapController.IsChunkLoaded(client.focused_map_id,pos);
                        NetworkChunk chunk = MapController.GetChunk(client.focused_map_id,pos);
                        if(!is_loaded) ChunkController.SetupChunk(chunk);
                        in_vis_range.Add(chunk);
                    }
                }
            }
		}

        // Unload others
        List<NetworkChunk> loaded_chunks = MapController.GetAllLoadedChunks();
        foreach(NetworkChunk chunk in loaded_chunks)
        {
            // chunk loaded, handle if it should unload
            if(chunk.timer % 10 == 0 && !in_vis_range.Contains(chunk))
            {
                MapController.ChunkUnload(chunk);
                break; // limit unloads...
            }
        }
    }

    public static void SetupChunk(NetworkChunk chunk)
    {
        MapController.GridPos pos = new MapController.GridPos(GetAlignedPos(chunk.Position));
        for(int u = 0; u < ChunkController.chunk_size; u++) 
        {
            for(int v = 0; v < ChunkController.chunk_size; v++) 
            {
                float hor = pos.hor + u;
                float ver = pos.ver + v;
                AbstractTurf turf = MapController.GetTurfAtPosition(chunk.map_id_string,new MapController.GridPos(hor,ver,pos.dep),true);
                turf.UpdateIcon();  // Build mesh!
                foreach(AbstractEntity ent in turf.Contents)
                {
                    ent.UpdateIcon();
                    ent.SyncNetwork(true);
                }
            }
        }
        // Force initial graphical state
        chunk.MeshUpdate();
        chunk.Tick();
    }

    public static void CleanChunk(NetworkChunk chunk)
    {
        MapController.GridPos pos = new MapController.GridPos(GetAlignedPos(chunk.Position));
        for(int u = 0; u < ChunkController.chunk_size; u++) 
        {
            for(int v = 0; v < ChunkController.chunk_size; v++) 
            {
                float hor = pos.hor + u;
                float ver = pos.ver + v;
                AbstractTurf turf = MapController.GetTurfAtPosition(chunk.map_id_string,new MapController.GridPos(hor,ver,pos.dep),true);
                foreach(AbstractEntity ent in turf.Contents)
                {
                    ent.UnloadNetworkEntity();
                }
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
        List<NetworkChunk> loaded_chunks = MapController.GetAllLoadedChunks();
        foreach(NetworkChunk chunk in loaded_chunks)
        {
            SetupChunk(chunk);
        }
    }
}
