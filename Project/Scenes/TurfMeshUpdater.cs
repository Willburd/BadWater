using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class TurfMeshUpdater : Node3D
{

    public static Dictionary<string,Resource> texture_cache = new Dictionary<string,Resource>();

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
    public void MeshUpdated(string json)
    {
        GD.Print("MESH UPDATE   " + json);
        Godot.Collections.Dictionary chunk_data = TOOLS.ParseJson(json);
        for(int i = 0; i < mesh_array.Length; i++) 
        {
            Godot.Collections.Dictionary turf_data = (Godot.Collections.Dictionary)chunk_data["turf_" + i];
            string model = turf_data["model"].AsString();
            string texture = turf_data["texture"].AsString();
            double anim_speed = turf_data["anim_speed"].AsDouble();
            MeshInstance3D mesh = mesh_array[i];

            // Assign model,tex, and animation speed to turf's model!
            if(mesh.MaterialOverride == null) mesh.MaterialOverride = GD.Load("res://Materials/Main.tres").Duplicate(true) as ShaderMaterial;
            string path = "res://Library/Textures/" + texture;
            if(!texture_cache.ContainsKey(path))
            {
                if(!Godot.FileAccess.FileExists(path)) path = "res://Library/Textures/Error.png";
                texture_cache[path] = GD.Load( path);
            }
            (mesh.MaterialOverride as ShaderMaterial).SetShaderParameter( "_MainTexture", texture_cache[path]);
        }
    }
}
