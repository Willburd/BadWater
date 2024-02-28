using Godot;
using System;

[GlobalClass] 
public partial class PackData : Resource
{
    protected string source_file_path = "";
    protected void SetIdentity(string set_prefix, string set_ID, string path)
    {
        mod_prefix = set_prefix;
        unique_ID = set_ID;
        source_file_path = path;
    }

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

    // Unique data
    public bool density = false;
    public bool opaque = false;
}
