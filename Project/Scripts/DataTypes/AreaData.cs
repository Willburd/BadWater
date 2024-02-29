using Godot;
using System;

[GlobalClass] 
public partial class AreaData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        display_name    = TOOLS.ApplyExistingTag(data,"name",display_name);
        is_space        = TOOLS.ApplyExistingTag(data,"is_space",is_space);
        always_powered  = TOOLS.ApplyExistingTag(data,"always_powered",always_powered);
    }
    public override void ShowVars()
    {
        // Print variables of loaded data for debugging
        GD.Print("-" + GetType().ToString() + ":" + GetUniqueModID + " name: " + display_name + " : "  + is_space + " : " + always_powered);
    }
    
    // Unique data
    public string base_turf_ID;
    public bool always_powered;
    public bool is_space;
}
