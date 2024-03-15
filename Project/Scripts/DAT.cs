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


    public const int DEFAULT_ATTACK_COOLDOWN = 8; //Default timeout for aggressive actions
    public const int DEFAULT_QUICK_COOLDOWN = 4;


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
        int rev = 0;
        if(((int)dir & (int)Dir.North)  == (int)Dir.North) rev |= (int)Dir.South;
        if(((int)dir & (int)Dir.South)  == (int)Dir.South) rev |= (int)Dir.North;
        if(((int)dir & (int)Dir.East)   == (int)Dir.East)  rev |= (int)Dir.West;
        if(((int)dir & (int)Dir.West)   == (int)Dir.West)  rev |= (int)Dir.East;
        if(((int)dir & (int)Dir.Up)     == (int)Dir.Up)    rev |= (int)Dir.Down;
        if(((int)dir & (int)Dir.Down)   == (int)Dir.Down)  rev |= (int)Dir.Up;
        return (Dir)rev;
    }

    public static Dir InputToDir(float x, float y)
    {
        Dir newdir = Dir.None;
        if(x < 0) newdir |= Dir.West;
        if(x > 0) newdir |= Dir.East;
        if(y < 0) newdir |= Dir.North;
        if(y > 0) newdir |= Dir.South;
        return newdir;
    }

    public static Dir InputToCardinalDir(float x, float y)
    {
        Dir newdir = Dir.None;
        if(Mathf.Abs(x) > Mathf.Abs(y))
        {
            if(x < 0) newdir |= Dir.West;
            if(x > 0) newdir |= Dir.East;
        }
        else
        {
            if(y < 0) newdir |= Dir.North;
            if(y > 0) newdir |= Dir.South;
        }
        return newdir;
    }

    public static Dir RotateCardinal(Dir input, int steps)
    {
        steps %= 4; // lets reduce this down to something sensible.
        if(steps == 0) return input; // would be the same
        if(Mathf.Abs(steps) == 2) // just reverse it
        {
            return ReverseDir(input);
        }
        // Rotating left/right by one incriment, 3 steps is just the same as going backward by 1 at the start.
        if(steps == 3) steps = -1;
        if(steps == -3) steps = 1;
        if(steps == -1)
        {
            // left
            switch(input)
            {
                case Dir.North: return Dir.West;
                case Dir.West: return Dir.South;
                case Dir.South: return Dir.East;
                case Dir.East: return Dir.North;
            }
        }
        else
        {
            //right
            switch(input)
            {
                case Dir.North: return Dir.East;
                case Dir.East: return Dir.South;
                case Dir.South: return Dir.West;
                case Dir.West: return Dir.North;
            }
        }
        return Dir.None;
    }
}