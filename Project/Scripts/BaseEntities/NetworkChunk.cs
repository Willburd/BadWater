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
    [Export]
    public TurfMeshUpdater mesh_updater;

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
        for(int u = 0; u < ChunkController.chunk_size; u++) 
        {
            for(int v = 0; v < ChunkController.chunk_size; v++) 
            {
                float hor = pos.hor + u;
                float ver = pos.ver + v;
                Godot.Collections.Dictionary turf_data;
                AbstractTurf turf = MapController.GetTurfAtPosition(map_id_string,new MapController.GridPos(hor,ver,pos.dep));
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = debug_visual)]
    private void ClientMeshUpdate(Vector3 pos, string mesh_json)
    {
        Position = pos;
        mesh_updater.MeshUpdated(mesh_json);
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
