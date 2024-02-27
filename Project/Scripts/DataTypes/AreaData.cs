using Godot;
using System;

[GlobalClass] 
public partial class AreaData : PackData
{
    public void Init(string set_prefix, string set_ID, string set_name, bool set_is_space, bool set_is_always_powered)
    {
        SetIdentity( set_prefix, set_ID);
        display_name = set_name;
        is_space = set_is_space;
        always_powered = set_is_always_powered;
        GD.Print("-" + GetUniqueID + " name: " + display_name + " is_space: "  + is_space + " always_powered: " + always_powered);
    }
    bool always_powered;
    bool is_space;
}
