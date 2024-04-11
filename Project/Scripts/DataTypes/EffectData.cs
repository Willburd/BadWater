using Godot;
using System;

[GlobalClass] 
public partial class EffectData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        base.SetVars(data_override);
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " tag: " + tag;
    }
    
    public override void Clone(PackData source)
    {
        EffectData temp = source as EffectData;
        base.Clone(temp);
    }

    // Unique data
}
