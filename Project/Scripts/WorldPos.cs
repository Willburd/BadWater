using Godot;
using System;

public struct WorldPos
{
    public WorldPos(float x, float y, float z)
    {
        pos = new Vector3(x,y,z);
    }

    public Vector3 pos;
}
