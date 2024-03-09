using Godot;

[GlobalClass]
public partial class MeshUpdater : MeshInstance3D
{
    public const string error_path = "res://Library/Textures/Error.png";
    public static string GetPath(string texture_path)
    {
        return "res://Library/Textures/" + texture_path;
    }

    public void TextureUpdated(string json)
    {
        TextureUpdated(TOOLS.ParseJson(json));
    }

    public static MeshUpdater GetModelScene(Godot.Collections.Dictionary turf_data)
    {
        string path = "res://Library/Models/" + turf_data["model"].AsString();
        return (MeshUpdater)AssetLoader.loaded_models[path].Instantiate();
    }
    
    public void TextureUpdated(Godot.Collections.Dictionary turf_data)
    {
        string texture = GetPath(turf_data["texture"].AsString());
        double anim_speed = turf_data["anim_speed"].AsDouble();
        // Assign model,tex, and animation speed to the entity!
        TextureDataUpdate(this,texture);
    }

    public static void TextureDataUpdate(MeshInstance3D mesh,string texture_path)
    {
        if(!AssetLoader.loaded_textures.ContainsKey(texture_path)) texture_path = "res://Library/Textures/Error.png";
        AssetLoader.LoadedTexture tex_data = AssetLoader.loaded_textures[texture_path];
        // Load from assetloader's material cache. Get the page the texture is on, and set it's offset from the atlas we built on launch!
        mesh.SetSurfaceOverrideMaterial(0,AssetLoader.material_cache[tex_data.tex_page]);
        mesh.SetInstanceShaderParameter( "_XY", new Vector2((float)tex_data.u / AssetLoader.tex_page_size,(float)tex_data.v / AssetLoader.tex_page_size) );
        mesh.SetInstanceShaderParameter( "_WH", new Vector2((float)tex_data.width / AssetLoader.tex_page_size,(float)tex_data.height / AssetLoader.tex_page_size) );
    }
}
