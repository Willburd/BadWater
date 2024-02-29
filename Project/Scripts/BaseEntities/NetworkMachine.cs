using Godot;
using System;
using System.Collections.Generic;

// Machine entities are objects on a map that perform a regular update, are not living things, and often interact directly with the map. Rarely some objects that are not machines may use this type.
[GlobalClass] 
public partial class NetworkMachine : NetworkEntity
{
    // Beginning of template data
    public override void TemplateClone(PackData data)
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