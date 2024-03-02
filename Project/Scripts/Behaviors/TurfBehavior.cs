using Godot;
using System;

// Handles construction clicks
public partial class TurfBasic : Behavior
{
    public TurfBasic(int set_build_level)
    {
        build_level = set_build_level;
    }

    int build_level = 0;

    public override void MapLoadVars(Godot.Collections.Dictionary data)
    {
        // can_build_on = TOOLS.ApplyExistingTag(data,"can_build_on",can_build_on);
    }
}
