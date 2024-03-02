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
        description     = TOOLS.ApplyExistingTag(data,"desc",description);
        behaviorID      = TOOLS.ApplyExistingTag(data,"behavior",behaviorID);
        tag             = TOOLS.ApplyExistingTag(data,"tag",tag);
        model           = TOOLS.ApplyExistingTag(data,"model",model);
        texture         = TOOLS.ApplyExistingTag(data,"texture",texture);
        is_space        = TOOLS.ApplyExistingTag(data,"is_space",is_space);
        always_powered  = TOOLS.ApplyExistingTag(data,"always_powered",always_powered);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " tag: " + tag + " is_space: " + is_space + " always_powered: " + always_powered;
    }
    
    // Unique data
    public string base_turf_ID;
    public bool always_powered;
    public bool is_space;
}
