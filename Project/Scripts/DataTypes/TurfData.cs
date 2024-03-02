using Godot;
using System;

[GlobalClass] 
public partial class TurfData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        display_name      = TOOLS.ApplyExistingTag(data,"name",display_name);
        description       = TOOLS.ApplyExistingTag(data,"desc",description);
        behaviorID        = TOOLS.ApplyExistingTag(data,"behavior",behaviorID);
        model             = TOOLS.ApplyExistingTag(data,"model",model);
        texture           = TOOLS.ApplyExistingTag(data,"texture",texture);
        density           = TOOLS.ApplyExistingTag(data,"density",density);
        opaque            = TOOLS.ApplyExistingTag(data,"opaque",opaque);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " density: "  + density + " model: "  + model + " texture: "  + texture + " opaque: " + opaque;
    }
    
    // Unique data
    public bool density = false;
    public bool opaque = false;
}
