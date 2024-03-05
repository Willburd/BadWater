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
        anim_speed      = TOOLS.ApplyExistingTag(data,"anim_speed",(double)0);
        intangible      = TOOLS.ApplyExistingTag(data,"intangible",intangible);
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
        intangible          = source.intangible;
    }

    protected virtual string GetVarString()
    {
        return "";
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
    public string display_name = "Pack";
    public string description = "";
    public string behaviorID = "";
    public string tag = "";
    public string model = "Plane";
    public string texture = "";
    public double anim_speed = 0;
    public bool intangible;
}
