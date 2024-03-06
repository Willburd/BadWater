using Godot;
using System;

[GlobalClass] 
public partial class MobData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        base.SetVars(data_override);
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        max_health       = TOOLS.ApplyExistingTag(data,"max_health",max_health);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " tag: " + tag + " maxhealth: "  + max_health;
    }
    
    public override void Clone(PackData source)
    {
        MobData temp = source as MobData;
        base.Clone(temp);
        max_health = temp.max_health;
    }

    // Unique data
    public float max_health;
}
