using Godot;
using System;

[GlobalClass] 
public partial class TurfData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data)
    {
        display_name    = TOOLS.ApplyExistingTag(data,"name",display_name);
        density          = TOOLS.ApplyExistingTag(data,"density",density);
        opaque           = TOOLS.ApplyExistingTag(data,"opaque",opaque);
    }
    public override void ShowVars()
    {
        // Print variables of loaded data for debugging
        GD.Print("-" + GetUniqueModID + " name: " + display_name + " density: "  + density + " opaque: " + opaque);
    }
    
    // Unique data
    public bool density = false;
    public bool opaque = false;
}
