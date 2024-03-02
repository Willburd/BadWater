using Godot;
using System;
using System.Collections.Generic;

public partial class NetworkArea : NetworkEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        PackRef = new PackRef(data);
        AreaData temp = AssetLoader.GetPackFromModID(PackRef) as AreaData;
        model = temp.model;
        texture = temp.texture;
        base_turf_ID = temp.base_turf_ID;
        is_space = temp.is_space;
        always_powered = temp.always_powered;
        SetBehavior(Behavior.CreateBehavior(temp.behaviorID));
    }
    
    // Unique data
    [Export]
    public string base_turf_ID;
    [Export]
    public bool always_powered;
    [Export]
    public bool is_space;
    // End of template data
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }

    public void AddTurf(AbstractTurf turf)
    {
        // Remove from other areas
        turf.Area = this;
    }
}