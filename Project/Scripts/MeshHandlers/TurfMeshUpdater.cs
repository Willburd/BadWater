using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class TurfMeshUpdater : Node
{
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
        Godot.Collections.Dictionary chunk_data = TOOLS.ParseJson(json);
        for(int i = 0; i < mesh_array.Length; i++) 
        {
            Godot.Collections.Dictionary turf_data = (Godot.Collections.Dictionary)chunk_data["turf_" + i];
            string model = turf_data["model"].AsString();
            string texture = MeshUpdater.GetPath(turf_data["texture"].AsString());
            double anim_speed = turf_data["anim_speed"].AsDouble();

            // Assign model,tex, and animation speed to turf!
            MeshUpdater.TextureDataUpdate(mesh_array[i],texture);
        }
    }
}
