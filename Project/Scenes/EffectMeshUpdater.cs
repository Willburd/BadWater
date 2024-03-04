using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class EffectMeshUpdater : MeshInstance3D
{
    public static Dictionary<string,Resource> texture_cache = new Dictionary<string,Resource>();

    public void MeshUpdated(string json)
    {
        GD.Print("Efffect mesh print ");
        Godot.Collections.Dictionary turf_data = TOOLS.ParseJson(json);
        string model = turf_data["model"].AsString();
        string texture = turf_data["texture"].AsString();
        double anim_speed = turf_data["anim_speed"].AsDouble();

        // Assign model,tex, and animation speed to turf's model!
        if(MaterialOverride == null) MaterialOverride = GD.Load("res://Materials/Main.tres").Duplicate(true) as ShaderMaterial;
        string path = "res://Library/Textures/" + texture;
        if(!texture_cache.ContainsKey(path))
        {
            if(!Godot.FileAccess.FileExists(path)) path = "res://Library/Textures/Error.png";
            texture_cache[path] = GD.Load( path);
        }
        (MaterialOverride as ShaderMaterial).SetShaderParameter( "_MainTexture", texture_cache[path]);
    }
}
