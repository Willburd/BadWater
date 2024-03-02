using Godot;
using System;

[GlobalClass] 
public partial class EffectData : PackData
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
        is_spawner      = TOOLS.ApplyExistingTag(data,"is_spawner",is_spawner);
        cleanable       = TOOLS.ApplyExistingTag(data,"cleanable",cleanable);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " tag: " + tag;
    }

    // Unique data
    public bool is_spawner;
    public bool cleanable;
}
