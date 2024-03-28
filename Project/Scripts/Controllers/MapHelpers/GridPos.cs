using Godot;
using System;

public struct GridPos
{
    public GridPos(string map_id, float set_hor, float set_ver, float set_dep)
    {
        mapid = map_id;
        hor = set_hor;
        ver = set_ver;
        dep = set_dep;
    }
    public GridPos(string map_id, Vector3 worldPos)
    {
        mapid = map_id;
        hor = (float)(worldPos.X / MapController.tile_size);
        ver = (float)(worldPos.Z / MapController.tile_size);
        dep = (float)(worldPos.Y / MapController.tile_size);
    }

    public readonly Vector3 WorldPos()
    {
        if(mapid == "NULL" || mapid == "BAG") return Vector3.Zero;
        GridPos align_pos = MapController.GetMap(mapid).submap_pos;
        float align_hor = Mathf.Floor(align_pos.hor);
        float align_ver = Mathf.Floor(align_pos.ver);
        float align_dep = Mathf.Floor(align_pos.dep);
        return new Vector3((hor+align_hor) * MapController.tile_size, (dep+align_dep) * MapController.tile_size,(ver+align_ver) * MapController.tile_size);
    }
    
    public readonly GridPos GetCentered()
    {
        GridPos align_pos = MapController.GetMap(mapid).submap_pos;
        float align_hor = Mathf.Floor(align_pos.hor);
        float align_ver = Mathf.Floor(align_pos.ver);
        float align_dep = Mathf.Floor(align_pos.dep);
        return new GridPos( mapid, hor+align_hor+0.5f, ver+align_ver+0.5f, dep+align_dep);
    }

    public readonly Vector3 WorldPosCentered()
    {
        return GetCentered().WorldPos();
    }

    public readonly bool Equals(GridPos other)
    {
        if(!MapController.OnSameMap(GetMapID(),other.GetMapID())) return false;
        return Mathf.FloorToInt(hor) == Mathf.FloorToInt(other.hor) && Mathf.FloorToInt(ver) == Mathf.FloorToInt(other.ver) && Mathf.FloorToInt(dep) == Mathf.FloorToInt(other.dep);
    }

    public readonly ChunkPos ChunkPos()
    {
        return new ChunkPos(WorldPos());
    }
    
    public readonly string GetMapID()
    {
        return mapid;
    }
    string mapid;
    public float hor;
    public float ver;
    public float dep;
}