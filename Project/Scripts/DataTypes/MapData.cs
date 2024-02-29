using Godot;
using System;

[GlobalClass] 
public partial class MapData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        display_name    = TOOLS.ApplyExistingTag(data,"name",display_name);
        width           = TOOLS.ApplyExistingTag(data,"width",width);
        height          = TOOLS.ApplyExistingTag(data,"height",height);
        depth           = TOOLS.ApplyExistingTag(data,"depth",depth);
    }
    
    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " width: "  + width + " height: " + height + " depth: " + depth;
    }
    
    // Unique data
    public int width;
    public int height;
    public int depth;
}
