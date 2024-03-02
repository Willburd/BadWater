using Godot;
using System;
using System.Collections.Generic;

// Machine entities are objects on a map that perform a regular update, are not living things, and often interact directly with the map. Rarely some objects that are not machines may use this type.
[GlobalClass] 
public partial class NetworkMachine : NetworkEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        PackRef = new PackRef(data);
        //MachineData temp = AssetLoader.GetPackFromModID(PackRef) as MachineData;
        //SetTag(temp.tag);
        //model = temp.model;
        //texture = temp.texture;
        //density = template_data.density;
        //opaque = template_data.opaque;
        //SetBehavior(Behavior.CreateBehavior(temp.behaviorID));
    }
    public override PackData TemplateWrite()
    {
        ItemData data = new ItemData();


        return data;
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
