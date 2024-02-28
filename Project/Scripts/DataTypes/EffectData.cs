using Godot;
using System;

[GlobalClass] 
public partial class EffectData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data)
    {
        display_name    = TOOLS.ApplyExistingTag(data,"name",display_name);
        spawner_id      = TOOLS.ApplyExistingTag(data,"spawner_id",spawner_id);
        cleanable       = TOOLS.ApplyExistingTag(data,"cleanable",cleanable);
    }
    public override void ShowVars()
    {
        // Print variables of loaded data for debugging
        GD.Print("-" + GetUniqueModID + " name: " + display_name );
    }

    // Unique data
    public string spawner_id;
    public bool cleanable;
}
