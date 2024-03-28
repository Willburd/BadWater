using Godot;
using System;

public static class MapTools
{
    public static bool OnSameMap(AbstractEntity A,AbstractEntity B)
    {
        if(A.GetLocation() is not AbstractTurf || B.GetLocation() is not AbstractTurf) return false; // in bag
        return OnSameMap(A.GridPos.GetMapID(),B.GridPos.GetMapID());
    }
    
    public static bool OnSameMap(string A,string B)
    {
        if(A == "BAG" || B == "BAG" || A == "NULL" || B == "NULL") return false; // catch some hardcoded specials for bags and nullspace. Should use the entity version of this to check beforehand, but best to be safe.
        // alright lets do the rest of this proper
        if(A == B) return true;
        if(MapController.GetMap(A).HasSubmap(B) || MapController.GetMap(B).HasSubmap(A)) return true;
        return false;
    }


    

    public static bool Adjacent(AbstractEntity A,AbstractEntity B, bool ignore_corner_density)
    {
        // different maps, and depth doesn't count
        if(!OnSameMap(A,B) || A.GridPos.dep != B.GridPos.dep) return false;
        // Turf pos are centered on the turf
        GridPos A_pos = A.GridPos;
        GridPos B_pos = B.GridPos;
        if(A is AbstractTurf) A_pos = A.GridPos.GetCentered();
        if(B is AbstractTurf) B_pos = B.GridPos.GetCentered(); 
        // center of turfs
        if(A is AbstractTurf || B is AbstractTurf)
        {
            Vector3 dir_vec = GetMapDirection(A,B);
            if(!ignore_corner_density && DAT.DirIsDiagonal( DAT.VectorToDir(dir_vec.X,dir_vec.Y)))
            {
                // Check corner blockages
                // TODO ==================================================================================================
            }
            return Mathf.Abs(A.GridPos.hor - B_pos.hor) < 1 || Mathf.Abs(A.GridPos.ver - B_pos.ver) < 1;
        }
        return Adjacent(A_pos.WorldPos(),B_pos.WorldPos(), ignore_corner_density);
    }
    public static bool Adjacent(Vector3 A_pos,Vector3 B_pos, bool ignore_corner_density) // Assumes OnSameMap(A,B) already passed!
    {
        // Entity checking
        if(Mathf.Floor(A_pos.Y) != Mathf.Floor(B_pos.Y)) return false;
        Vector3 dir_vec = GetMapDirection(A_pos,B_pos);
        if(!ignore_corner_density && DAT.DirIsDiagonal( DAT.VectorToDir(dir_vec.X,dir_vec.Y)))
        {
            // Check corner blockages
            // TODO ==================================================================================================
        }
        return GetMapDistance(A_pos,B_pos) <= DAT.ADJACENT_DISTANCE;
    }




    public static float GetMapDistance(AbstractEntity A,AbstractEntity B)
    {
        if(!OnSameMap(A.GridPos.GetMapID(),B.GridPos.GetMapID())) return Mathf.Inf;  // returns infinity if not on same map
        GridPos A_align = A.GridPos;
        GridPos B_align = B.GridPos;
        if(A is AbstractTurf) A_align = A.GridPos.GetCentered();
        if(B is AbstractTurf) B_align = B.GridPos.GetCentered();
        return GetMapDistance(A_align.WorldPos(),B_align.WorldPos());
    }
    public static float GetMapDistance(Vector3 A_pos,Vector3 B_pos)
    {
        // Flatten
        A_pos.Y *= 0f;
        B_pos.Y *= 0f;
        // Check if on same map beforehand!
        return TOOLS.VecDist(A_pos,B_pos); // should just be world position checks if already on same map. World pos are prealigned
    }



    public static Vector3 GetMapDirection(AbstractEntity A,AbstractEntity B)
    {
        // turf align
        GridPos A_align = A.GridPos;
        GridPos B_align = B.GridPos;
        if(A is AbstractTurf) A_align = A.GridPos.GetCentered();
        if(B is AbstractTurf) B_align = B.GridPos.GetCentered();
        // Check if on same map beforehand!
        return GetMapDirection(A_align.WorldPos(),B_align.WorldPos());
    }
    public static Vector3 GetMapDirection(Vector3 A_pos,Vector3 B_pos)
    {
        // Flatten
        A_pos.Y *= 0;
        B_pos.Y *= 0;
        return TOOLS.DirVec(A_pos,B_pos); // should just be world position checks if already on same map. World pos are prealigned
    }
    


    public static bool GetMapVisibility(AbstractEntity A,AbstractEntity B)  // if A can see B
    {
        // TODO - Check if mob A can see mob B ==================================================================================================
        if(!OnSameMap(A,B)) return false;
        return GetMapVisibility(A.GridPos.WorldPos(),B.GridPos.WorldPos());
    }
    public static bool GetMapVisibility(Vector3 A_pos,Vector3 B_pos)  // if A can see B
    {
        // Check if on same map beforehand!
        return TOOLS.VecDist(A_pos,B_pos) < MapController.screen_visible_range;
    }
}
