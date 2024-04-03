using Godot;
using System;

[GlobalClass] 
public partial class StructureData : PackData
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
        return " name: " + display_name + " description: " + description + " tag: " + tag +  " density: "  + density + " model: "  + model + " texture: "  + texture + " opaque: " + opaque;
    }
    
    public override void Clone(PackData source)
    {
        TurfData temp = source as TurfData;
        base.Clone(temp);
        temp.density = density;
        temp.opaque = opaque;
    }

    // Unique data
    public bool density = false;
    public bool opaque = false;
}
