using Godot;
using System;

[GlobalClass] 
public partial class PackData : Resource
{
    protected void SetIdentity(string set_prefix, string set_ID)
    {
        mod_prefix = set_prefix;
        unique_ID = set_ID;
    }

    protected string mod_prefix = "";
    protected string unique_ID = "Turf";
    // mod_prefix is used to avoid collisions with turfs, by having namespaces for the data
    public string GetUniqueID
    {
        get { return mod_prefix + ":" + unique_ID; }
    }
    public string display_name = "Pack";
}
