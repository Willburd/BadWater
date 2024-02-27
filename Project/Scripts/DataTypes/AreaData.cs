using Godot;
using System;

[GlobalClass] 
public partial class AreaData : PackData
{
    public void Init(string set_prefix, string set_ID, string set_name, string set_base_turf_ID, bool set_is_space, bool set_is_always_powered)
    {
        SetIdentity( set_prefix, set_ID);
        base_turf_ID = set_base_turf_ID;
        display_name = set_name;
        is_space = set_is_space;
        always_powered = set_is_always_powered;
        GD.Print("-" + GetUniqueID + " name: " + display_name + " is_space: "  + is_space + " always_powered: " + always_powered);
    }
    
    // Unique data
    public string base_turf_ID;
    public bool always_powered;
    public bool is_space;
}
