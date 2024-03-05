using Godot;
using System.Collections.Generic;
using System;
using System.ComponentModel;

[GlobalClass]
public partial class MeshUpdater : MeshInstance3D
{
    public const string error_path = "res://Library/Textures/Error.png";

    public static Dictionary<string,Resource> texture_cache = new Dictionary<string,Resource>();

    public void MeshUpdated(string json)
    {
        Godot.Collections.Dictionary turf_data = TOOLS.ParseJson(json);
        string model = turf_data["model"].AsString();
        string texture = GetPath(turf_data["texture"].AsString());
        double anim_speed = turf_data["anim_speed"].AsDouble();

        // Assign model,tex, and animation speed to turf's model!
        MaterialOverride ??= GD.Load("res://Materials/Main.tres").Duplicate(true) as ShaderMaterial;
        PreprocessTextures(texture);
        (MaterialOverride as ShaderMaterial).SetShaderParameter( "_MainTexture", texture_cache[texture]);
    }

    public static string GetPath(string texture_path)
    {
        return "res://Library/Textures/" + texture_path;
    }

    public static void PreprocessTextures(string path)
    {
        if(!texture_cache.ContainsKey(path))
        {
            string error_path = path;
            if(!Godot.FileAccess.FileExists(path)) path = error_path;
            texture_cache[error_path] = GD.Load( path);
            texture_cache[path] = GD.Load( path);
        }
    }
}
