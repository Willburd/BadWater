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
    public bool face_camera = false;
    [Export]
    public bool is_sprite = false;
    [Export]
    public bool is_directional = false;
    [Export]
    public bool render_above;


    /*****************************************************************
     * Rendering
     ****************************************************************/
    private Godot.Collections.Dictionary current_data;
    private float animator_value = 0f;
    public NetworkEntity Entity
    {
        get {return GetParent() as NetworkEntity;}
    }
    public string GetDisplayText
    {
        get {if(Entity is NetworkEffect net_effect) return net_effect.synced_text; else return "";}
    }
    public string GetShaderMaterial
    {
        get {return render_above ? ShaderConfig.above_all : ShaderConfig.main;}
    }
    public void TextureUpdated(string json)
    {
        TextureUpdated(TOOLS.ParseJson(json));
    }
    

    public static MeshUpdater GetModelScene(Godot.Collections.Dictionary data)
    {
        string path = "res://Library/Models/" + data["model"].AsString();
        if(!AssetLoader.loaded_models.ContainsKey(path)) return null;
        return (MeshUpdater)AssetLoader.loaded_models[path].Instantiate();
    }
    public void TextureUpdated(Godot.Collections.Dictionary data)
    {
        if(mesh == null || mesh.GetSurfaceOverrideMaterialCount() == 0) return;
        // Check for new animations
        string old_tex = "";
        if(current_data != null) old_tex = current_data["texture"].AsString();

        // Update with new data
        current_data = data;
        string texture      = current_data["texture"].AsString();
        double anim_speed   = current_data["anim_speed"].AsDouble();
        string state        = "Idle";
        if(is_directional) state = current_data["state"].AsString();

        // new animations reset to 0!
        if(old_tex != texture) animator_value = 0;
        // Assign model,tex, and animation speed to the entity!
        TextureDataUpdate(texture,state,anim_speed > 0);
    }
    public void TextureDataUpdate(string texture_path, string icon_state, bool animating)
    {
        if(!is_directional)
        {
            // Solve animation
            if(animating) 
            {
                // Get animation index 
                string animation_suffix = "_" + Mathf.Floor(animator_value);;
                string new_path = "res://Library/Textures/" + texture_path.Replace(".png", "") + animation_suffix + ".png";
                if(!AssetLoader.loaded_textures.ContainsKey(new_path)) 
                {
                    // end of animation attempt a loop
                    animator_value -= Mathf.Floor(animator_value);
                    animation_suffix = "_" + Mathf.Floor(animator_value);
                    new_path = "res://Library/Textures/" + texture_path.Replace(".png", "") + animation_suffix + ".png";
                }
                if(AssetLoader.loaded_textures.ContainsKey(new_path)) texture_path = new_path;
            }
            else
            {
                texture_path = "res://Library/Textures/" + texture_path;
            }
            if(!AssetLoader.loaded_textures.ContainsKey(texture_path)) texture_path = "res://Library/Textures/Error.png";
            cached_texpath = texture_path;
            cached_current_texdata = AssetLoader.loaded_textures[cached_texpath];
            // Get shader to use
            // Load from assetloader's material cache. Get the page the texture is on, and set it's offset from the atlas we built on launch!
            mesh.SetSurfaceOverrideMaterial(0,AssetLoader.material_cache[GetShaderMaterial][cached_current_texdata.tex_page]);
            mesh.SetInstanceShaderParameter( "_XY", new Vector2((float)cached_current_texdata.u / AssetLoader.tex_page_size,(float)cached_current_texdata.v / AssetLoader.tex_page_size) );
            mesh.SetInstanceShaderParameter( "_WH", new Vector2((float)cached_current_texdata.width / AssetLoader.tex_page_size,(float)cached_current_texdata.height / AssetLoader.tex_page_size) );
            mesh.SetInstanceShaderParameter( "_AA", draw_alpha);
        }
        else
        {
            // Decode direction sprites from state of mob
            string animation_suffix = "";
            if(animating) animation_suffix = "_" + animator_value;
            // Solve animation and direction!
            cached_texpath = texture_path;
            cached_icon_state = icon_state;
            cached_animation_suffix = animation_suffix;
            RotateDirectionInRelationToCamera();
        }
    }

    /*****************************************************************
     * These are used for internal rotations, has to be done regularly...
     ****************************************************************/
    private Vector3 camera_relational_vector;
    private string cached_texpath;
    public string CachedTexturePath
    {
        get {return cached_texpath;}
    }
    private AssetLoader.LoadedTexture cached_current_texdata;
    private string cached_icon_state;
    private string cached_animation_suffix;
    public void RotateDirectionInRelationToCamera()
    {
        // Solve rotation steps from camera rotation
        if(mesh == null || mesh.GetSurfaceOverrideMaterialCount() == 0) return;
        float solve_step = Mathf.Round(new Vector2(camera_relational_vector.X,camera_relational_vector.Z).Angle() / (Mathf.Pi * 2) * 100);
        int dir_steps;
        if(Mathf.Abs(solve_step) < 4.5) dir_steps = 0;
        else if(Mathf.Abs(solve_step) < 48.5) dir_steps = 1;
        else dir_steps = 2;
        dir_steps *= Mathf.Sign(solve_step);
        string direction_tex = "res://Library/Textures/" + cached_texpath + "/" + cached_icon_state + "/" + DAT.RotateCardinal(Entity.direction, Mathf.RoundToInt(dir_steps)) + cached_animation_suffix + ".png";
        // Check if asset exists as directional, and fallback otherwise
        if(!AssetLoader.loaded_textures.ContainsKey(direction_tex)) direction_tex = "res://Library/Textures/" + cached_texpath + "/" + cached_icon_state + cached_animation_suffix + "/Base.png";
        if(!AssetLoader.loaded_textures.ContainsKey(direction_tex)) direction_tex = "res://Library/Textures/" + cached_texpath + "/" + cached_icon_state + "/Base.png";
        if(!AssetLoader.loaded_textures.ContainsKey(direction_tex)) direction_tex = "res://Library/Textures/Error.png";
        cached_current_texdata = AssetLoader.loaded_textures[direction_tex];
        // Load from assetloader's material cache. Get the page the texture is on, and set it's offset from the atlas we built on launch!
        mesh.SetSurfaceOverrideMaterial(0,AssetLoader.material_cache[GetShaderMaterial][cached_current_texdata.tex_page]);
        mesh.SetInstanceShaderParameter( "_XY", new Vector2((float)cached_current_texdata.u / AssetLoader.tex_page_size,(float)cached_current_texdata.v / AssetLoader.tex_page_size) );
        mesh.SetInstanceShaderParameter( "_WH", new Vector2((float)cached_current_texdata.width / AssetLoader.tex_page_size,(float)cached_current_texdata.height / AssetLoader.tex_page_size) );
        mesh.SetInstanceShaderParameter( "_AA", draw_alpha);
    }

    public override void _Ready()
    {
        BillboardFaceCamera();
    }

    public override void _PhysicsProcess(double delta)
    {
        BillboardFaceCamera();
        // New animation frame!
        if(current_data != null)
        {
            int old_anim_frame = Mathf.FloorToInt(animator_value);
            animator_value += (float)(current_data["anim_speed"].AsDouble() * delta);
            if(old_anim_frame != Mathf.FloorToInt(animator_value)) TextureUpdated(current_data);
        }
    }

    public void BillboardFaceCamera()
    {
        if(!face_camera) return;
        // Use constraint to look at camera.
        if(GetViewport().GetCamera3D() != null)
        {
            Quaternion quat = GetViewport().GetCamera3D().Quaternion;
            Vector3 solve_vec = quat * Vector3.Forward;
            LookAt(GlobalPosition + solve_vec);
            camera_relational_vector = Vector3.Right * 50f * quat;
            camera_relational_vector.Y = 0;
            camera_relational_vector = camera_relational_vector.Normalized();
            if(is_directional) RotateDirectionInRelationToCamera();
        }
    }


    /*****************************************************************
     * Texture clicking
     ****************************************************************/
    public bool ClickInput(Camera3D camera, InputEvent evnt, Vector3 position, StaticBody3D collider)
    {
        if(evnt is InputEventMouseButton button)
        {
            if(button.ButtonIndex == MouseButton.Left || button.ButtonIndex == MouseButton.Middle)
            {
                Vector2 texspace = ColliderUVSpace(position,collider);
                if(CheckTexturePressed(texspace.X,texspace.Y)) 
                {
                    if(button.Pressed)
                    {
                        (GetParent() as NetworkEntity).ClickPressed(position,button.ButtonIndex);
                    }
                    else
                    {
                        (GetParent() as NetworkEntity).ClickReleased(position,button.ButtonIndex);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public Vector2 ColliderUVSpace(Vector3 position,StaticBody3D collider)
    {
        // Godot, please explain to me why you don't have the ability to get texcoord from a click off a mesh...
        // Massively crippling for anyone doing pixel detection or mesh painting..
        position = collider.ToLocal(position);
        position *= collider.Quaternion.Inverse();
        float meshu = Mathf.InverseLerp(-1f,1f,position.X);
        float meshv = Mathf.InverseLerp(-1f,1f,position.Z);
        return new Vector2(meshu,meshv);
    }

    public bool CheckTexturePressed(float meshu, float meshv)
    {
        // Check texture information
        AssetLoader.LoadedTexture tex_data = cached_current_texdata;
        float ux = tex_data.u + (tex_data.width * meshu);
        float vy = tex_data.v + (tex_data.height * meshv);
        Color col = AssetLoader.texture_pages[tex_data.tex_page].GetPixel(Mathf.FloorToInt(ux),Mathf.FloorToInt(vy));
        return col.A > 0.01f;
    }

    /*****************************************************************
     * Offset animation handler
     ****************************************************************/
    float draw_alpha = 1f;
    public void SetAnimationVars( float alpha)
    {
        draw_alpha = alpha;
    }
}
