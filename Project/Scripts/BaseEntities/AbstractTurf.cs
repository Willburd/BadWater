using Godot;
using System;

public partial class AbstractTurf : AbstractEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        TurfData temp = data as TurfData;
        model = temp.model;
        texture = temp.texture;
        density = temp.density;
        opaque = temp.opaque;
    }
    [Export]
    public bool density;                // blocks movement
    [Export]
    public bool opaque;               // blocks vision
    // End of template data


    AtmoController.AtmoCell air_mix = null;
    private NetworkArea area = null;

    public virtual void RandomTick()                // Some turfs respond to random updates, every area will perform a number of them based on the area's size!
    {

    }

    public void AtmosphericsCheck()
    {
        // check all four directions for any atmo diffs, if any flag for update - TODO
    }

    public MapController.GridPos GetGridPosition()
    {
        MapController.GridPos grid = new MapController.GridPos(Position);
        Position = TOOLS.GridToPos(grid); // ensures snapping
        return grid;
    }

    public NetworkArea Area
    {
        get {return area;}
        set {area = value;} // SET USING Area.AddTurf() DO NOT SET DIRECTLY
    }
}
