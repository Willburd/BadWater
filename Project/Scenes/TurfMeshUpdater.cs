using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class TurfMeshUpdater : Node3D
{

    public static Dictionary<string,Resource> texture_cache = new Dictionary<string,Resource>();

    private string last_tex = "";

    private MeshInstance3D[] mesh_array;

    public override void _Ready()
    {
        mesh_array = new MeshInstance3D[ChunkController.chunk_size * ChunkController.chunk_size];
        int i = 0;
        foreach(MeshInstance3D mesh in GetChildren())
        {
            mesh_array[i] = mesh;
            i++;
        }
    }

    public void MeshUpdated(NetworkChunk turf)
    {
        /*
        bool force_update = false;
        if(MaterialOverride == null)
        {
            MaterialOverride = GD.Load("res://Materials/Main.tres").Duplicate(true) as ShaderMaterial;
            force_update = true;
        }

        if(force_update || last_tex != turf.texture)
        {
            string path = "res://Library/Textures/" + turf.texture;
            if(!texture_cache.ContainsKey(path))
            {
                if(!Godot.FileAccess.FileExists(path)) path = "res://Library/Textures/Error.png";
                texture_cache[path] = GD.Load( path);
            }
            (MaterialOverride as ShaderMaterial).SetShaderParameter( "_MainTexture", texture_cache[path]);
            last_tex = turf.texture;
        }
        */
    }
}
