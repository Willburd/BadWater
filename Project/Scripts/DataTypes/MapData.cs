using Godot;
using System;

[GlobalClass] 
public partial class MapData : PackData
{
    public void Init(string file_path,string set_prefix, string set_ID, string set_name, int set_width, int set_height, int set_depth)
    {
        SetIdentity( set_prefix, set_ID, file_path);
        display_name = set_name;
        width = set_width;
        height = set_height;
        depth = set_depth;
        GD.Print("-" + GetUniqueModID + " name: " + display_name + " width: "  + width + " height: " + height + " depth: " + depth);
    }
    
    // Unique data
    public int width;
    public int height;
    public int depth;
}
