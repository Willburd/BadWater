using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkController : DeligateController
{
    public static int chunk_load_range = 3;
    public static int chunk_unload_range = 5;


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

            // Load our chunks
            MapController.ChunkPos pos = new MapController.ChunkPos(client.focused_position);
            if(!MapController.IsChunkLoaded(client.focused_map_id,pos))
            {
                // Init chunk!
                NetworkChunk map_chunk = MapController.GetChunk(client.focused_map_id,pos);
                map_chunk.Position = GetAlignedPos(client.focused_position);
            }

            // Unload others
            List<NetworkChunk> chunks = MapController.GetLoadedChunks(client.focused_map_id);
            foreach(NetworkChunk chunk in chunks)
            {
                // hor/ver distance
                Vector2 simple_pos = new Vector2(client.focused_position.X,client.focused_position.Y);
                Vector2 simple_chunk = new Vector2(chunk.Position.X,chunk.Position.Y);
                // dep distance
                float dep_dist = Mathf.Abs(client.focused_position.Z - chunk.Position.Z);

                // chunk loaded, handle if it should unload
                if(chunk.timer % 10 == 0 && (simple_chunk.DistanceSquaredTo(simple_pos) > chunk_size * chunk_unload_range || dep_dist > 2))
                {
                    MapController.ChunkUnload(chunk);
                    break; // limit unloads...
                }
            }
		}
    }

    public Vector3 GetAlignedPos(Vector3 world_pos)
    {
        world_pos = new Vector3(Mathf.FloorToInt(world_pos.X / chunk_size) * chunk_size, Mathf.FloorToInt(world_pos.Y), Mathf.FloorToInt(world_pos.Z / chunk_size) * chunk_size);
        return world_pos;
    }

    public override void Shutdown()
    {
        
    }
}
