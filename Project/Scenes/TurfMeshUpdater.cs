using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class TurfMeshUpdater : MeshInstance3D
{

    public static Dictionary<string,Resource> texture_cache = new Dictionary<string,Resource>();

    private string last_tex = "";

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