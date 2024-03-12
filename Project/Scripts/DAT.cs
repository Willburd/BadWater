using Godot;
using System;

public static class DAT
{
    public const string station_orig	= "Outpost 21";
    public const string station_short	= "OP21";
    public const string dock_name		= "Eshui Central Command";
    public const string boss_name		= "Central Command";
    public const string boss_short	    = "CentCom";
    public const string company_name	= "Eshui Atmospherics";
    public const string company_short	= "ES";
    public const string star_name		= "SL-340";
    public const string starsys_name	= "SL-340";



    public enum Dir
    {
        // Cardinals
        None = 0,
        North = 1,
        South = 2,
        East = 4,
        West = 8,
        Up = 10,
        Down = 20,
        // Non-cardinal
        NorthWest = North | West,
        NorthEast = North | East,
        SouthWest = South | West,
        SouthEast = South | East,
        // Non-cardinal up
        NorthWestUp = North | West | Up,
        NorthEastUp = North | East | Up,
        SouthWestUp = South | West | Up,
        SouthEastUp = South | East | Up,
        // Non-cardinal down
        NorthWestDown = North | West | Down,
        NorthEastDown = North | East | Down,
        SouthWestDown = South | West | Down,
        SouthEastDown = South | East | Down,
    }

    public static Dir ReverseDir(Dir dir)
    {
        if(dir == Dir.None) return Dir.None;
        uint rev = 0;
        if(((uint)dir & (uint)Dir.North) > 0)
        {
            rev |= (uint)Dir.South;
        }
        if(((uint)dir & (uint)Dir.South) > 0)
        {
            rev |= (uint)Dir.North;
        }
        if(((uint)dir & (uint)Dir.East) > 0)
        {
            rev |= (uint)Dir.West;
        }
        if(((uint)dir & (uint)Dir.West) > 0)
        {
            rev |= (uint)Dir.East;
        }
        if(((uint)dir & (uint)Dir.Up) > 0)
        {
            rev |= (uint)Dir.Down;
        }
        if(((uint)dir & (uint)Dir.Down) > 0)
        {
            rev |= (uint)Dir.Up;
        }
        return (Dir)rev;
    }
}