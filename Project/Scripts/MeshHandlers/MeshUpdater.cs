using System.Data;
using System.Data.Common;
using Godot;

[GlobalClass]
public partial class MeshUpdater : Node3D
{
    public const string error_path = "res://Library/Textures/Error.png";

    [Export]
    public MeshInstance3D mesh;
    [Export]
    public bool is_sprite = false;
    [Export]
    public bool is_directional = false;

    private Vector3 camera_relational_vector;

    public void TextureUpdated(string json)
    {
        TextureUpdated(TOOLS.ParseJson(json));
    }

    public static MeshUpdater GetModelScene(Godot.Collections.Dictionary turf_data)
    {
        string path = "res://Library/Models/" + turf_data["model"].AsString();
        if(!AssetLoader.loaded_models.ContainsKey(path)) return null;
        return (MeshUpdater)AssetLoader.loaded_models[path].Instantiate();
    }

    public void TextureUpdated(Godot.Collections.Dictionary turf_data)
    {
        string texture      = turf_data["texture"].AsString();
        double anim_speed   = turf_data["anim_speed"].AsDouble();
        string state        = "Idle";
        DAT.Dir direction   = DAT.Dir.None;
        if(is_directional)
        {
            state        = turf_data["state"].AsString();
            direction    = (DAT.Dir)turf_data["direction"].AsInt32();
        }
        // Assign model,tex, and animation speed to the entity!
        TextureDataUpdate(texture,state,direction,anim_speed > 0,0);
    }

    public void TextureDataUpdate(string texture_path, string icon_state, DAT.Dir direction, bool animating, float animation_index)
    {
        if(!is_directional)
        {
            // Solve animation
            if(animating) 
            {
                // Get animation index 
                string animation_suffix = "";
                animation_suffix = "_" + animation_index;
                string new_path = "res://Library/Textures/" + texture_path.Replace(".png", "") + animation_suffix + ".png";
                if(AssetLoader.loaded_textures.ContainsKey(new_path)) texture_path = new_path;
            }
            else
            {
                texture_path = "res://Library/Textures/" + texture_path;
            }
            if(!AssetLoader.loaded_textures.ContainsKey(texture_path)) texture_path = "res://Library/Textures/Error.png";
            AssetLoader.LoadedTexture tex_data = AssetLoader.loaded_textures[texture_path];
            // Load from assetloader's material cache. Get the page the texture is on, and set it's offset from the atlas we built on launch!
            mesh.SetSurfaceOverrideMaterial(0,AssetLoader.material_cache[tex_data.tex_page]);
            mesh.SetInstanceShaderParameter( "_XY", new Vector2((float)tex_data.u / AssetLoader.tex_page_size,(float)tex_data.v / AssetLoader.tex_page_size) );
            mesh.SetInstanceShaderParameter( "_WH", new Vector2((float)tex_data.width / AssetLoader.tex_page_size,(float)tex_data.height / AssetLoader.tex_page_size) );
        }
        else
        {
            // Decode direction sprites from state of mob
            string animation_suffix = "";
            if(animating) animation_suffix = "_" + animation_index;
            // Solve animation and direction!
            cached_texpath = texture_path;
            cached_icon_state = icon_state;
            cached_animation_suffix = animation_suffix;
            cached_direction = direction;
            RotateDirectionInRelationToCamera();
        }
    }

    
    // These are used for internal rotations, has to be done regularly...
    private string cached_texpath;
    private string cached_icon_state;
    private DAT.Dir cached_direction;
    private string cached_animation_suffix;
    public void RotateDirectionInRelationToCamera()
    {
        // Solve rotation steps from camera rotation
        int dir_steps = Mathf.RoundToInt(Mathf.RadToDeg(new Vector2(camera_relational_vector.X,camera_relational_vector.Z).Angle() + 360) / 90) % 4;
        string direction_tex = "res://Library/Textures/" + cached_texpath + "/" + cached_icon_state + "/" + DAT.RotateCardinal(cached_direction, dir_steps) + cached_animation_suffix + ".png";
        // Check if asset exists as directional, and fallback otherwise
        if(!AssetLoader.loaded_textures.ContainsKey(direction_tex)) direction_tex = "res://Library/Textures/" + cached_texpath + "/" + cached_icon_state + cached_animation_suffix + "/Base.png";
        if(!AssetLoader.loaded_textures.ContainsKey(direction_tex)) direction_tex = "res://Library/Textures/" + cached_texpath + "/" + cached_icon_state + "/Base.png";
        if(!AssetLoader.loaded_textures.ContainsKey(direction_tex)) direction_tex = "res://Library/Textures/Error.png";
        AssetLoader.LoadedTexture tex_data = AssetLoader.loaded_textures[direction_tex];
        // Load from assetloader's material cache. Get the page the texture is on, and set it's offset from the atlas we built on launch!
        mesh.SetSurfaceOverrideMaterial(0,AssetLoader.material_cache[tex_data.tex_page]);
        mesh.SetInstanceShaderParameter( "_XY", new Vector2((float)tex_data.u / AssetLoader.tex_page_size,(float)tex_data.v / AssetLoader.tex_page_size) );
        mesh.SetInstanceShaderParameter( "_WH", new Vector2((float)tex_data.width / AssetLoader.tex_page_size,(float)tex_data.height / AssetLoader.tex_page_size) );
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if(!is_directional) return;
        // Use constraint to look at camera.
        if(GetViewport().GetCamera3D() != null)
        {
            LookAt(GetViewport().GetCamera3D().Position,Vector3.Up,true);
            camera_relational_vector = Vector3.Forward * GetViewport().GetCamera3D().Quaternion;
            camera_relational_vector.Y = 0;
            camera_relational_vector = camera_relational_vector.Normalized();
            RotateDirectionInRelationToCamera();
        }
    }
}
