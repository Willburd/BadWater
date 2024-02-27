using Godot;
using System;
using System.Collections.Generic;

// Turfs are map tiles that other entities move on. Turfs have a list of entities they currently contain.
[GlobalClass] 
public partial class NetworkTurf : NetworkEntity
{
    private TurfData data;

    AtmoController.AtmoCell air_mix = null;
    private NetworkArea area = null;
    private string grid_lookup;

    public virtual void RandomTick()                // Some turfs respond to random updates, every area will perform a number of them based on the area's size!
    {

    }

    public void AtmosphericsCheck()
    {
        // check all four directions for any atmo diffs, if any flag for update - TODO
    }

    public void SetGridPosition(Vector3 gridpos)
    {
        Position = gridpos * MapController.TileSize;
        UpdateGridPosition();
    }

    public Vector3 GetGridPosition()
    {
        Vector3 gridpos = Position / MapController.TileSize;
        gridpos.X = Mathf.Floor(gridpos.X);
        gridpos.Y = Mathf.Floor(gridpos.Y);
        gridpos.Z = Mathf.Floor(gridpos.Z);
        Position = gridpos * MapController.TileSize; // ensures snapping
        return gridpos;
    }

    public void UpdateGridPosition()                              // ABSOLUTELY CRITICAL TO GAME FUNCTION. Turfs that have moved MUST UPDATE THEIR GRID LOCATION WITH THIS. EVEN ON CREATION.
    {
        Vector3 current_pos = GetGridPosition(); // snaps tile to grid here...
        grid_lookup = MapController.FormatWorldPosition(map_id_string,current_pos);
        MapController.turf_at_location[grid_lookup] = this; // UPDATE MAP!
    }


    public NetworkArea Area
    {
        get {return area;}
        set {area = value;} // SET USING Area.AddTurf() DO NOT SET DIRECTLY
    }
}
