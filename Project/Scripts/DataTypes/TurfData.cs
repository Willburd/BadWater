using Godot;
using System;

[GlobalClass] 
public partial class TurfData : PackData
{
    public void Init(string set_prefix, string set_ID, string set_name, bool set_density, bool set_opaque)
    {
        SetIdentity( set_prefix, set_ID);
        display_name = set_name;
        density = set_density;
        opaque = set_opaque;
        GD.Print("-" + GetUniqueID + " name: " + display_name + " density: "  + density + " opaque: " + opaque);
    }
    // mod_prefix is used to avoid collisions with turfs, by having namespaces for the data
    public bool density = false;
    public bool opaque = false;
}
