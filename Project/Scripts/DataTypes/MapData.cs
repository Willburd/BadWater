using Godot;
using System;

[GlobalClass] 
public partial class MapData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data)
    {
        display_name    = TOOLS.ApplyExistingTag(data,"name",display_name);
        width           = TOOLS.ApplyExistingTag(data,"width",width);
        height          = TOOLS.ApplyExistingTag(data,"height",height);
        depth           = TOOLS.ApplyExistingTag(data,"depth",depth);
    }
    public override void ShowVars()
    {
        // Print variables of loaded data for debugging
        GD.Print("-" + GetUniqueModID + " name: " + display_name + " width: "  + width + " height: " + height + " depth: " + depth);
    }
    
    // Unique data
    public int width;
    public int height;
    public int depth;
}
