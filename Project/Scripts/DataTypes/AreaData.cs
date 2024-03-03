using Godot;
using System;

[GlobalClass] 
public partial class AreaData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        base.SetVars(data_override);
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        is_space        = TOOLS.ApplyExistingTag(data,"is_space",is_space);
        always_powered  = TOOLS.ApplyExistingTag(data,"always_powered",always_powered);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " tag: " + tag + " is_space: " + is_space + " always_powered: " + always_powered;
    }
    
    public override void Clone(PackData source)
    {
        AreaData temp = source as AreaData;
        base.Clone(temp);
        base_turf_ID = temp.base_turf_ID;
        always_powered = temp.always_powered;
        is_space = temp.is_space;
    }
    
    // Unique data
    public string base_turf_ID;
    public bool always_powered;
    public bool is_space;
}
