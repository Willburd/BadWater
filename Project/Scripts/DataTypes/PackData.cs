using Godot;
using System;


[GlobalClass] 
public partial class PackData : Resource
{
    protected string source_file_path = "";
    protected void SetIdentity(string set_prefix, string set_ID, string set_path, string set_parent)
    {
        mod_prefix = set_prefix;
        unique_ID = set_ID;
        source_file_path = set_path;
        data_parent = set_parent;   // INHERETANCE IS FUN
    }

    public void Init(string file_path, string set_prefix, string set_ID, Godot.Collections.Dictionary data)
    {
        SetIdentity( set_prefix, set_ID, file_path, TOOLS.ApplyExistingTag(data,"parent",""));
        temp_file_data = data;
        ParentFlag = data_parent == ""; // used by the inheretance setup code. If this is false, we aren't ready to be read... So we defer the parent data loading until we are! 
    }

    public virtual void SetVars(Godot.Collections.Dictionary data_override = null) // Uses temp_file_data to setup the object...
    {

    }

    public Godot.Collections.Dictionary GetTempData()
    {
        return temp_file_data;
    }
    public void ClearTempData()
    {
        temp_file_data = null;
    }

    protected virtual string GetVarString()
    {
        return "";
    }

    public void ShowVars()
    {
        // Print variables of loaded data for debugging
        GD.Print("-" + GetType().ToString() + ":" + GetUniqueModID + GetVarString());
    }
    
    public PackData Clone()
    {
        return MemberwiseClone() as PackData;
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
    public string display_name = "Pack";
    public string description = "";
    public string behaviorID = "";
    public string model = "Plane";
    public string texture = "";
}
