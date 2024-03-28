using Godot;
using System;

public struct ChunkPos
{
    public ChunkPos(int set_hor, int set_ver, int set_dep)
    {
        hor = set_hor;
        ver = set_ver;
        dep = set_dep;
    }
    public ChunkPos(Vector3 worldPos)
    {
        hor = Mathf.FloorToInt(worldPos.X / (ChunkController.chunk_size * MapController.tile_size));
        ver = Mathf.FloorToInt(worldPos.Z / (ChunkController.chunk_size * MapController.tile_size));
        dep = Mathf.FloorToInt(worldPos.Y);
    }

    public readonly bool Equals(GridPos other)
    {
        return hor == other.hor && ver == other.ver && dep == other.dep;
    }

    public int hor;
    public int ver;
    public int dep;
}