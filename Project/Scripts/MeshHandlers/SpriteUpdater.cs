using Godot;

[GlobalClass]
public partial class SpriteUpdater : Node3D
{
    public const string error_path = "res://Library/Textures/Error.png";

    [Export]
    public Sprite3D mesh;

    public void TextureUpdated(string json)
    {
        TextureUpdated(TOOLS.ParseJson(json));
    }

    public static SpriteUpdater GetModelScene(Godot.Collections.Dictionary turf_data)
    {
        string path = "res://Library/Models/" + turf_data["model"].AsString();
        if(!AssetLoader.loaded_models.ContainsKey(path)) return null;
        return (SpriteUpdater)AssetLoader.loaded_models[path].Instantiate();
    }
    
    public void TextureUpdated(Godot.Collections.Dictionary turf_data)
    {
        string texture = turf_data["texture"].AsString();
        string icon_state = turf_data["state"].AsString();
        DAT.Dir direction = (DAT.Dir)turf_data["direction"].AsUInt32();
        double anim_speed = turf_data["anim_speed"].AsDouble();
        // Assign model,tex, and animation speed to the entity!
        TextureDataUpdate(mesh,texture, icon_state, anim_speed > 0,0,direction);
    }

    public static void TextureDataUpdate(Sprite3D mesh,string texture_path, string icon_state, bool animating, float animation_index, DAT.Dir direction)
    {
        // Decode direction sprites from state of mob
        string animation_suffix = "";
        if(animating) animation_suffix = "_" + animation_index;
        // Solve animation and direction!
        string direction_tex = "res://Library/Textures/" + texture_path + "/" + icon_state + "/" + direction + animation_suffix + ".png";
        if(!AssetLoader.loaded_textures.ContainsKey(direction_tex)) direction_tex = "res://Library/Textures/" + icon_state + "/" + texture_path + animation_suffix + "/Base.png";
        if(!AssetLoader.loaded_textures.ContainsKey(direction_tex)) direction_tex = "res://Library/Textures/" + icon_state + "/" + texture_path + "/Base.png";
        if(!AssetLoader.loaded_textures.ContainsKey(direction_tex)) direction_tex = "res://Library/Textures/Error.png";
        AssetLoader.LoadedTexture tex_data = AssetLoader.loaded_textures[direction_tex];
        // Load from assetloader's material cache. Get the page the texture is on, and set it's offset from the atlas we built on launch!
        //mesh.Texture = AssetLoader.texture_pages[tex_data.tex_page];
        mesh.SetInstanceShaderParameter( "_XY", new Vector2((float)tex_data.u / AssetLoader.tex_page_size,(float)tex_data.v / AssetLoader.tex_page_size) );
        mesh.SetInstanceShaderParameter( "_WH", new Vector2((float)tex_data.width / AssetLoader.tex_page_size,(float)tex_data.height / AssetLoader.tex_page_size) );
    }

    public override void _PhysicsProcess(double delta)
    {
        // Use constraint to look at camera.
        if(GetViewport().GetCamera3D() != null)
        {
            LookAt(GetViewport().GetCamera3D().Position);
        }
    }
}
