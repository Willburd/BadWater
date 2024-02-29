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
        description    = TOOLS.ApplyExistingTag(data,"desc",description);
        spawner_id      = TOOLS.ApplyExistingTag(data,"spawner_id",spawner_id);
        cleanable       = TOOLS.ApplyExistingTag(data,"cleanable",cleanable);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description;
    }

    // Unique data
    public string spawner_id;
    public bool cleanable;
}
