using Godot;
using System;

[GlobalClass]
public partial class TurfMeshUpdater : MeshInstance3D
{

    private string last_tex = "";

    public void MeshUpdated(NetworkTurf turf)
    {
        bool force_update = false;
        if(MaterialOverride == null)
        {
            MaterialOverride = GD.Load("res://Materials/Main.tres").Duplicate(true) as ShaderMaterial;
            force_update = true;
        }

        if(force_update || last_tex != turf.texture)
        {
            string path = "res://Library/Textures/" + turf.texture;
            if(!Godot.FileAccess.FileExists(path)) path = "res://Library/Textures/Error.png";
            (MaterialOverride as ShaderMaterial).SetShaderParameter( "_MainTexture", GD.Load( path));
            last_tex = turf.texture;
        }
    }
}
