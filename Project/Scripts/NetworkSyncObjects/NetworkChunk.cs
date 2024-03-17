using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

// Turfs are map tiles that other entities move on. Turfs have a list of entities they currently contain.
[GlobalClass] 
public partial class NetworkChunk : NetworkEntity
{
    public int timer = 0;
    public bool do_not_unload = false;
    private MeshUpdater[] mesh_array = new MeshUpdater[ChunkController.chunk_size * ChunkController.chunk_size];

    private bool mesh_dirty;
    
    public void Tick()
    {
        timer += 1;
        if(mesh_dirty) 
        {
            Internal_MeshUpdate();
            mesh_dirty = false;
        }
    }

    public override void MeshUpdate()
    {
        mesh_dirty = true; // lets wait till all of the mesh has settled!
    }

    protected override void Internal_MeshUpdate()
    {
        Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
        int steps = 0;
        MapController.GridPos pos = new MapController.GridPos(ChunkController.GetAlignedPos(Position));
        for(int v = 0; v < ChunkController.chunk_size; v++) 
        {
            for(int u = 0; u < ChunkController.chunk_size; u++) 
            {
                float hor = pos.hor + u;
                float ver = pos.ver + v;
                Godot.Collections.Dictionary turf_data;
                AbstractTurf turf = MapController.GetTurfAtPosition(map_id_string,new MapController.GridPos(hor,ver,pos.dep),true);
                if(turf == null) 
                {
                    turf_data = new Godot.Collections.Dictionary 
                    {
                        { "model", ""},
                        { "texture", ""},
                        { "anim_speed", ""}
                    };
                }
                else
                {
                    // Create per-turf data
                    turf_data = new Godot.Collections.Dictionary 
                    {
                        { "model", turf.model },
                        { "texture", turf.texture },
                        { "anim_speed", turf.anim_speed }
                    };
                }
                // append
                data.Add("turf_" + steps,turf_data);
                steps++;
            }
        }
        // Update json on other end.
        Rpc(nameof(ClientMeshUpdate) ,Position, Json.Stringify(data));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = debug_visual, TransferChannel = (int)MainController.RPCTransferChannels.VisualUpdate)]
    private void ClientMeshUpdate(Vector3 pos, string mesh_json)
    {
        Position = pos;
        Godot.Collections.Dictionary chunk_data = TOOLS.ParseJson(mesh_json);
        for(int i = 0; i < mesh_array.Length; i++) 
        {
            Godot.Collections.Dictionary turf_data = (Godot.Collections.Dictionary)chunk_data["turf_" + i];
            // Get new model
            if(mesh_array[i] != null) mesh_array[i].QueueFree();
            mesh_array[i] = MeshUpdater.GetModelScene(turf_data);
            mesh_array[i].Visible = false;
            AddChild(mesh_array[i]);
            // Init model textures
            if(mesh_array[i] == null) GD.Print("No model for " + turf_data["model"]);
            mesh_array[i].Position = new Vector3(Mathf.Floor(i % ChunkController.chunk_size) * MapController.tile_size,0,Mathf.Floor(i / ChunkController.chunk_size) * MapController.tile_size);
            mesh_array[i].TextureUpdated(turf_data);
            mesh_array[i].Visible = true;
        }
    }

    public bool Unload()
    {
        // handles unique situations where a chunk shouldn't unload just yet...
        if(!do_not_unload)
        {
            Kill();
            return true;
        }
        return false;
    }
    
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}