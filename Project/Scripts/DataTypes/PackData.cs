using Godot;
using System;


[GlobalClass] 
public partial class PackData : Resource
{
    protected string source_file_path = "";
    protected void SetIdentity(string set_prefix, string set_ID, MainController.DataType type, string set_path, string set_parent)
    {
        mod_prefix = set_prefix;
        unique_ID = set_ID;
        source_file_path = set_path;
        entity_type = type;
        data_parent = set_parent;   // INHERETANCE IS FUN
    }

    public void Init(string file_path, string set_prefix, string set_ID, MainController.DataType type, Godot.Collections.Dictionary data)
    {
        SetIdentity( set_prefix, set_ID, type, file_path, TOOLS.ApplyExistingTag(data,"parent",""));
        temp_file_data = data;
        ParentFlag = data_parent == ""; // used by the inheretance setup code. If this is false, we aren't ready to be read... So we defer the parent data loading until we are! 
    }

    public virtual void SetVars(Godot.Collections.Dictionary data_override = null) // Uses temp_file_data to setup the object...
    {
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        display_name    = TOOLS.ApplyExistingTag(data,"name",display_name);
        description     = TOOLS.ApplyExistingTag(data,"desc",description);
        behaviorID      = TOOLS.ApplyExistingTag(data,"behavior",behaviorID);
        tag             = TOOLS.ApplyExistingTag(data,"tag",tag);
        model           = TOOLS.ApplyExistingTag(data,"model",model);
        texture         = TOOLS.ApplyExistingTag(data,"texture",texture);
        anim_speed      = TOOLS.ApplyExistingTag(data,"anim_speed",anim_speed);
        // Sounds
        hit_sound       = TOOLS.ApplyExistingTag(data,"hit_sound",hit_sound);
        // When used as a weapon
        damtype         = StringToDamageType(TOOLS.ApplyExistingTag(data,"damage_type",damtype.ToString()));
        attack_range    = TOOLS.ApplyExistingTag(data,"attack_range",attack_range);
        attack_force    = TOOLS.ApplyExistingTag(data,"attack_force",attack_force);
        embed_chance    = TOOLS.ApplyExistingTag(data,"embed_chance",embed_chance);
        // Movement
        intangible      = TOOLS.ApplyExistingTag(data,"intangible",intangible);
        unstoppable     = TOOLS.ApplyExistingTag(data,"unstoppable",unstoppable);
    }

    public Godot.Collections.Dictionary GetTempData()
    {
        return temp_file_data;
    }
    public void ClearTempData()
    {
        temp_file_data = null;
    }


    public virtual void Clone(PackData source)
    {
        // needs to CLONE ALL VARS
        mod_prefix          = source.mod_prefix;
        unique_ID           = source.unique_ID;
        source_file_path    = source.source_file_path;
        entity_type         = source.entity_type;
        data_parent         = source.data_parent;
        temp_file_data      = source.temp_file_data;
        data_parent         = source.data_parent;
        mod_prefix          = source.mod_prefix;
        unique_ID           = source.unique_ID;
        display_name        = source.display_name;
        description         = source.description;
        behaviorID          = source.behaviorID;
        tag                 = source.tag;
        model               = source.model;
        texture             = source.texture;
        anim_speed          = source.anim_speed;
        hit_sound           = source.hit_sound;
        damtype             = source.damtype;
        attack_range        = source.attack_range;
        attack_force        = source.attack_force;
        embed_chance        = source.embed_chance;
        intangible          = source.intangible;
        unstoppable         = source.unstoppable;
    }

    protected virtual string GetVarString()
    {
        return "";
    }

    private static DAT.DamageType StringToDamageType(string parse)
    {
        if(Enum.TryParse(parse, out DAT.DamageType ret)) return ret;
        return DAT.DamageType.BRUTE;
    }

    public void ShowVars()
    {
        // Print variables of loaded data for debugging
        GD.Print("-" + GetUniqueModID + GetVarString());
    }


    protected Godot.Collections.Dictionary temp_file_data;
    protected bool loaded_parent;
    protected string data_parent = "";
    protected string mod_prefix = "";
    protected string unique_ID = "Turf";
    // mod_prefix is used to avoid collisions with turfs, by having namespaces for the data
    public string GetModID
    {
        get { return mod_prefix; }
    }
    public string GetUniqueID
    {
        get { return unique_ID; }
    }
    public string GetUniqueModID
    {
        get { return GetModID + ":" + GetUniqueID; }
    }
    public string GetDataParent
    {
        get {return data_parent;}
    }
    public string GetFilePath
    {
        get { return source_file_path; }
    }
    public bool ParentFlag
    {
        get { return loaded_parent; }
        set { loaded_parent = value; }
    }
    public MainController.DataType entity_type;
    public string display_name = "";
    public string description = "";
    public string behaviorID = "";
    public string tag = "";
    public string model = "BASE/Turfs/Plane.tscn";
    public string texture = "";
    public double anim_speed = 0;
    // Sounds
    public string hit_sound = "BASE/Attack/Generic";
    // When used as a weapon
    public DAT.DamageType damtype = DAT.DamageType.BRUTE;
    public int attack_range = 1;
    public float attack_force = 1f;
    public int embed_chance = 0;
    // Movement 
    public bool intangible = false;
    public bool unstoppable = false; // Can not be stopped from moving from Cross(), CanPass(), or Uncross() failing. Still bumps everything it passes through, though.
}
