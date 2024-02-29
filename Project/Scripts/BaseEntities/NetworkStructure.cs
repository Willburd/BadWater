using Godot;
using System;
using System.Collections.Generic;

// Structure entites are simple map objects that do not require any kind of automated update, but are not turfs. Things such as signs.
[GlobalClass] 
public partial class NetworkStructure : NetworkEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        template_data = data;
        //density = template_data.density;
        //opaque = template_data.opaque;
    }
    [Export]
    public bool density;                // blocks movement
    [Export]
    public bool opaque;               // blocks vision
    // End of template data
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
