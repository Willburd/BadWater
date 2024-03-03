using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkController : DeligateController
{
    public static int chunk_load_range = 5;
    public static int chunk_unload_range = 7;


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
        List<NetworkClient> client_list = MainController.ClientList;
        for(int i = 0; i <client_list.Count; i++) 
		{
			NetworkClient client = client_list[i];
            if(!MapController.IsMapLoaded(client.focused_map_id))
            {
                GD.PrintErr("CLIENT " + client.Name + " ON UNLOADED MAP " + client.focused_map_id);
                client.Spawn(); // EMERGENCY RESPAWN
            }

            // hor/ver distance
            float loadborder_w = chunk_load_range;
            float loadborder_h = chunk_load_range * (float)0.8;
            for(int u = 0; u < loadborder_w; u++) 
            {
                for(int v = 0; v < loadborder_h; v++) 
                {
                    // Load our chunks
                    MapController.ChunkPos pos = new MapController.ChunkPos(client.focused_position);
                    pos.hor -= Mathf.FloorToInt(loadborder_w/2);
                    pos.ver -= Mathf.FloorToInt(loadborder_h/2);
                    pos.hor += u;
                    pos.ver += v;
                    if(!MapController.IsChunkLoaded(client.focused_map_id,pos)) MapController.GetChunk(client.focused_map_id,pos);
                }
            }

            // Unload others
            List<NetworkChunk> chunks = MapController.GetLoadedChunks(client.focused_map_id);
            foreach(NetworkChunk chunk in chunks)
            {
                // hor/ver distance
                float unloadborder_w = chunk_unload_range;
                float unloadborder_h = chunk_unload_range * (float)0.8;
                float gridsize = ChunkController.chunk_size * MapController.tile_size;
                Rect2 rect = new Rect2(client.focused_position.X - ((unloadborder_w/2) * gridsize),client.focused_position.Z - ((unloadborder_h/2) * gridsize),unloadborder_w * gridsize,unloadborder_h * gridsize);
                Vector2 simple_chunk = new Vector2(chunk.Position.X,chunk.Position.Z);
                
                // dep distance
                float dep_dist = Mathf.Abs(client.focused_position.Z - chunk.Position.Z);

                // chunk loaded, handle if it should unload
                if(chunk.timer % 10 == 0 && (!rect.HasPoint(simple_chunk) || dep_dist > 2))
                {
                    MapController.ChunkUnload(chunk);
                    break; // limit unloads...
                }
            }
		}
    }

    public Vector3 GetAlignedPos(Vector3 world_pos)
    {
        float alignsize = ChunkController.chunk_size * MapController.tile_size;
        world_pos = new Vector3(Mathf.FloorToInt(world_pos.X / alignsize) * alignsize, Mathf.FloorToInt(world_pos.Y), Mathf.FloorToInt(world_pos.Z / alignsize) * alignsize);
        return world_pos;
    }

    public override void Shutdown()
    {
        
    }
}
