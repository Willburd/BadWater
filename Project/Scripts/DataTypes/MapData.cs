using Godot;
using System;

[GlobalClass] 
public partial class MapData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        base.SetVars(data_override);
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        width           = JsonHandler.ApplyExistingTag(data,"width",width);
        height          = JsonHandler.ApplyExistingTag(data,"height",height);
        depth           = JsonHandler.ApplyExistingTag(data,"depth",depth);
    }
    
    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " tag: " + tag + " width: " + width + " height: " + height + " depth: " + depth;
    }
    
    public override void Clone(PackData source)
    {
        MapData temp = source as MapData;
        base.Clone(temp);
        width = temp.width;
        height = temp.height;
        depth = temp.depth;
    }
    
    // Unique data
    public int width;
    public int height;
    public int depth;
}
