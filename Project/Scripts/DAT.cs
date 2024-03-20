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

    public const int TK_MAXRANGE = 15;

    public const float ADJACENT_DISTANCE = 0.98f;


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

    public static bool DirIsCardinal(Dir dir)
    {
        if(((int)dir & (int)Dir.North)  == (int)Dir.North) return true;
        if(((int)dir & (int)Dir.South)  == (int)Dir.South) return true;
        if(((int)dir & (int)Dir.East)  == (int)Dir.East) return true;
        if(((int)dir & (int)Dir.West)  == (int)Dir.West) return true;
        return false;
    }

    public static bool DirIsUpDown(Dir dir)
    {
        if(((int)dir & (int)Dir.Up)  == (int)Dir.Up) return true;
        if(((int)dir & (int)Dir.Down)  == (int)Dir.Down) return true;
        return false;
    }

    public static bool DirIsDiagonal(Dir dir)
    {
        // checks if not cardinal, and NOT updown either! Just inversing DirIsCardinal() won't do this, infact it will be true on up/down!
        if(DirIsUpDown(dir)) return false;
        return !DirIsCardinal(dir);
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

    public static Dir VectorToDir(float x, float y)
    {
        Dir newdir = Dir.None;
        if(x < 0) newdir |= Dir.West;
        if(x > 0) newdir |= Dir.East;
        if(y < 0) newdir |= Dir.North;
        if(y > 0) newdir |= Dir.South;
        return newdir;
    }

    public static Dir VectorToCardinalDir(float x, float y)
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

    public enum LifeState
    {
        Alive,
        Unconscious,
        Dead
    }

    public enum Intent
    {
        Help,
        Hurt
    }

    public enum ZoneSelection
    {
        Miss,
        UpperBody,
        LowerBody,
        Head,
        Eyes,
        Mouse,
        LeftArm,
        RightArm,
        LeftHand,
        RightHand,
        RightLeg,
        LeftLeg,
        RightFoot,
        LeftFoot
    }

    public enum InventorySlot
    {
        Rhand,
        Lhand,
        RhandLower,
        LhandLower,
        Head,
        Mask,
        Eyes,
        Uniform,
        Suit,
        Shoes,
        Lear,
        Rear,
        Gloves,
        Back,
        ID,
        Belt,
        Bag,
        Lpocket,
        Rpocket
    }

    public enum ToolTag
    {
        CROWBAR,
        MULTITOOL,
        SCREWDRIVER,
        WIRECUTTER,
        WRENCH,
        WELDER,
        CABLE_COIL,
        ANALYZER,
        MINING,
        SHOVEL,
        RETRACTOR,
        HEMOSTAT,
        CAUTERY,
        DRILL,
        SCALPEL,
        SAW,
        BONESET,
        KNIFE,
        BLOODFILTER,
        ROLLINGPIN
    }


    public enum SizeCategory
    {
        TINY,
        SMALL,
        MEDIUM, // Normal
        LARGE,
        HUGE,
        ITEMSIZE_NO_CONTAINER
    }

    public enum DamageType
    {
        // Standard damages
        BRUTE,      // Your basic vanilla bonking damage
        BURN,       // Can be caused with firestacks as well, burns are likely to get infected too!
        FREEZE,     // Burn but without fire stacks, and different stuff resists it... Also bleeds cause that's cool
        TOX,        // Poison in the body that will eventually kill you
        OXY,        // Suffocation
        CLONE,      // Genetic or nerve damage
        HALLOSS,    // Fake damage caused by hallucinations
        // Special damages
        ELECTROCUTE,// Special burn type with special resistances
        ACID,       // Generic chemical burns, doesn't have much in the way of any resistances to it!
        SEARING     // Brute + Burn combo damage
    }

    public enum WoundType
    {
        CUT,
        BRUISE,
        PIERCE
    }

    public enum StatusEffectType
    {
        STUN,
        WEAKEN,
        PARALYZE,
        IRRADIATE,
        AGONY,
        SLUR,
        STUTTER,
        EYE_BLUR,
        DROWSY
    }
    
    public enum ArmorType 
    {
        Melee,
        Bullet,
        Laser,
        Energy,
        Bomb,
        Bio,
        Rad
    }

    public static bool DamageTypeBleeds(DamageType type)
    {
        return type == DamageType.BRUTE || type == DamageType.FREEZE;
    }
}