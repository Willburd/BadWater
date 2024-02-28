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
        loaded_vars = false; // used by the inheretance setup code. If this is false, we aren't ready to be read... So we defer the parent data loading until we are! 
    }

    public virtual void SetVars(Godot.Collections.Dictionary data)
    {

    }

    public virtual void ShowVars()
    {
        // Print variables of loaded data for debugging
    }

    protected bool loaded_vars;
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
    
    public string GetFilePath
    {
        get { return source_file_path; }
    }
    public string display_name = "Pack";
}
