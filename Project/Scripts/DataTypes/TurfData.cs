using Godot;
using System;

[GlobalClass] 
public partial class TurfData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        display_name    = TOOLS.ApplyExistingTag(data,"name",display_name);
        density          = TOOLS.ApplyExistingTag(data,"density",density);
        opaque           = TOOLS.ApplyExistingTag(data,"opaque",opaque);
    }
    public override void ShowVars()
    {
        // Print variables of loaded data for debugging
        GD.Print("-" + GetType().ToString() + ":" + GetUniqueModID + " name: " + display_name + " density: "  + density + " opaque: " + opaque);
    }
    
    // Unique data
    public bool density = false;
    public bool opaque = false;
}
