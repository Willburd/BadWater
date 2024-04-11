using Godot;
using System;
using System.Diagnostics;

public static class AbstractTools
{
    /*****************************************************************
     * Movement
     ****************************************************************/
    public static void Move(AbstractEntity mover, GridPos new_grid, bool perform_turf_actions = true)
    {
        if(mover is AbstractTurf)
        {
            // skip all this, and instantly move if turf
            if(mover.PreMove(new_grid, perform_turf_actions))
            {
                mover.GridPos = new_grid;
                mover.PostMove(mover.GridPos);
            }
            return;
        }

        // Is new location valid?
        Vector3 dir_vec = MapTools.GetMapDirection(mover.GridPos.WorldPos(),new_grid.WorldPos());
        if(MapTools.OnSameMap(mover.GridPos.GetMapID(),new_grid.GetMapID()))
        {
            // EDGE LOCK
            float threshold = (float)0.01;
            if(!AbstractTurf.IsTurfValid(new GridPos(new_grid.GetMapID(),new_grid.hor,mover.GridPos.ver,mover.GridPos.dep)))
            {
                if(dir_vec.X < 0)
                {   
                    new_grid.hor = Mathf.Floor(mover.GridPos.hor) + threshold;
                }
                else if(dir_vec.X > 0)
                {
                    new_grid.hor = Mathf.Floor(mover.GridPos.hor) + 1 - threshold;
                }
            }
            if(!AbstractTurf.IsTurfValid(new GridPos(new_grid.GetMapID(),mover.GridPos.hor,new_grid.ver,mover.GridPos.dep)))
            {
                if(dir_vec.Z < 0)
                {
                    new_grid.ver = Mathf.Floor(mover.GridPos.ver) + threshold;
                }
                else if(dir_vec.Z > 0)
                {
                    new_grid.ver = Mathf.Floor(mover.GridPos.ver) + 1 - threshold;
                }
            }
            
            if(!mover.IsIntangible() && !mover.unstoppable) // ghosts, and unstoppable movers do not bump
            {
                // Check to see on each axis if we bump... This allows sliding!
                bool bump_h = false;
                AbstractTurf hor_turf = AbstractTurf.GetTurfAtPosition(new GridPos(new_grid.GetMapID(),new_grid.hor,mover.GridPos.ver,mover.GridPos.dep),true);
                if(hor_turf != null && hor_turf != mover.GetTurf() && hor_turf.density)
                {
                    bump_h = true;
                    if(dir_vec.X < 0)
                    {   
                        new_grid.hor = Mathf.Floor(mover.GridPos.hor) + threshold;
                    }
                    else if(dir_vec.X > 0)
                    {
                        new_grid.hor = Mathf.Floor(mover.GridPos.hor) + 1 - threshold;
                    }
                }
                bool bump_v = false;
                AbstractTurf ver_turf = AbstractTurf.GetTurfAtPosition(new GridPos(mover.GridPos.GetMapID(),mover.GridPos.hor,new_grid.ver,mover.GridPos.dep),true);
                if(ver_turf != null && ver_turf != mover.GetTurf() && ver_turf.density)
                {
                    bump_v = true;
                    if(dir_vec.Z < 0)
                    {
                        new_grid.ver = Mathf.Floor(mover.GridPos.ver) + threshold;
                    }
                    else if(dir_vec.Z > 0)
                    {
                        new_grid.ver = Mathf.Floor(mover.GridPos.ver) + 1 - threshold;
                    }
                }
                // Bump solids!
                AbstractTurf corner_turf = AbstractTurf.GetTurfAtPosition(new_grid,true);
                if(corner_turf != null && corner_turf.density)
                {
                    // Corner bonking is silly... Needs a unique case when you run into a corner exactly head on!
                    GridPos original_new = new_grid;
                    if(dir_vec.X < 0)
                    {   
                        new_grid.hor = Mathf.Floor(mover.GridPos.hor) + threshold;
                    }
                    else if(dir_vec.X > 0)
                    {
                        new_grid.hor = Mathf.Floor(mover.GridPos.hor) + 1 - threshold;
                    }
                    if(dir_vec.Z < 0)
                    {
                        new_grid.ver = Mathf.Floor(mover.GridPos.ver) + threshold;
                    }
                    else if(dir_vec.Z > 0)
                    {
                        new_grid.ver = Mathf.Floor(mover.GridPos.ver) + 1 - threshold;
                    }
                    mover.Bump(corner_turf);
                    corner_turf.Bump(mover);
                    
                    // Randomly break out of direct headon perfect corner intersections...
                    if(TOOLS.Prob(50)) 
                    {
                        new_grid.hor = original_new.hor;
                    }
                    else 
                    {
                        new_grid.ver = original_new.ver;
                    }
                }
                else
                {
                    if(bump_h) // bonk horizontal.
                    {
                        mover.Bump(hor_turf);
                        hor_turf.Bump(mover);
                    }
                    if(bump_v) // bonk verticle.
                    {
                        mover.Bump(ver_turf);
                        ver_turf.Bump(mover);
                    }
                }
            }  
        } 
        
        // At same location still, don't bother with much else...
        if(mover.GetLocation() is AbstractTurf && mover.GridPos.Equals(new_grid)) 
        {
            // Move around in current turf
            if(mover.PreMove(new_grid, perform_turf_actions))
            {
                mover.GridPos = new_grid;
                mover.UpdateNetwork(false,false);
                mover.PostMove(mover.GridPos);
            }
            return;
        }

        // Leave old location, perform uncrossing events! Enter new turf...
        if(mover.PreMove(new_grid, perform_turf_actions))
        {
            LeaveOldLoc(mover, perform_turf_actions);
            mover.GridPos = new_grid;

            // Enter new location!
            AbstractTurf new_turf = AbstractTurf.GetTurfAtPosition(mover.GridPos,true);
            new_turf?.EntityEntered(mover,perform_turf_actions);
            mover.UpdateNetwork(false,false);
            mover.PostMove(mover.GridPos);
        }
    }
    public static void Move(AbstractEntity mover, AbstractEntity new_destination, bool perform_turf_actions = true)
    {
        // If in same container, don't bother with entrance/exit actions.
        if(mover.GetLocation() == new_destination) return;
        if(new_destination is AbstractTurf)
        {
            // It's a turf! move normally!
            Move( mover, new_destination.GridPos, perform_turf_actions);
            return;
        }
        // Leave old location, perform uncrossing events!
        if(mover.PreMove( new GridPos("BAG",Vector3.Zero), perform_turf_actions))
        {
            LeaveOldLoc(mover, perform_turf_actions);

            // Enter new location
            mover.GridPos = new GridPos("BAG",Vector3.Zero);
            new_destination.EntityEntered(mover,perform_turf_actions);
            mover.UpdateNetwork(false,false);
            mover.PostMove(mover.GridPos);
        }
    }
    public static void Move(AbstractEntity mover) // Move to nullspace
    {
        // Leave old location, perform uncrossing events!
        if(mover.PreMove( new GridPos("NULL",Vector3.Zero), false))
        {
            LeaveOldLoc(mover,false);

            // Enter new location
            mover.GridPos = new GridPos("NULL",Vector3.Zero);
            mover.UpdateNetwork(false,false);
            mover.PostMove(mover.GridPos);
        }
    }
    
    private static void LeaveOldLoc(AbstractEntity mover, bool perform_turf_actions)
    {
        if(mover.GetLocation() != null)
        {
            // Leave old turf
            AbstractTurf old_turf = mover.GetLocation() as AbstractTurf;
            old_turf.EntityExited(mover,perform_turf_actions);
        }
    }

    public static void Drop(AbstractEntity mover, AbstractEntity new_destination, AbstractEntity user)
    {
        Move(mover,new_destination,true);
    }

    public static void PickUp(AbstractEntity mover, AbstractEntity new_destination, AbstractEntity user)
    {
        Move(mover,new_destination,true);
    }


    //Returns the storage depth of an atom. This is the number of storage items the atom is contained in before reaching toplevel (the turf).
    //Returns -1 if the atom was not found in a container.
    public static int StorageDepth(AbstractEntity checking, AbstractEntity container)
    {
        int depth = 0;
        AbstractEntity cur_entity = checking;
        while(cur_entity != null && !container.Contents.Contains(cur_entity))
        {
            if(cur_entity.GetLocation() is AbstractTurf) return -1;
            cur_entity = cur_entity.GetLocation();
            depth++;
        }
        if(cur_entity == null) return -1;	//inside something with a null location.
        return depth;
    }

    /*****************************************************************
     * Creation and destruction
     ****************************************************************/
    public static AbstractEntity CreateEntity( MainController.DataType type, string type_ID, string map_id, Vector3 pos,bool suppress_init = false)
    {
        GridPos? grid = new GridPos(map_id,pos);
        return CreateEntity( type, type_ID, grid, suppress_init);
    }

    public static AbstractEntity CreateEntity( MainController.DataType type, string type_ID, GridPos? pos,bool suppress_init = false, string data_string = "")
    {
        PackData typeData = null;
        AbstractEntity newEnt = null;
        switch(type)
        {
            case MainController.DataType.Area:
                typeData = AssetLoader.loaded_areas[type_ID];
                newEnt = new AbstractArea();
                break;
            case MainController.DataType.Turf:
                typeData = AssetLoader.loaded_turfs[type_ID];
                newEnt = AbstractTurf.CreateTurf(typeData, data_string);
                break;
            case MainController.DataType.Effect:
                typeData = AssetLoader.loaded_effects[type_ID];
                newEnt = AbstractEffect.CreateEffect(typeData, data_string);
                if(newEnt != null) MapController.controller.effects.Add(newEnt as AbstractEffect);
                break;
            case MainController.DataType.Item:
                typeData = AssetLoader.loaded_items[type_ID];
                newEnt = AbstractItem.CreateItem(typeData, data_string);
                if(newEnt != null) MapController.controller.entities.Add(newEnt);
                break;
            case MainController.DataType.Structure:
                typeData = AssetLoader.loaded_structures[type_ID];
                newEnt = AbstractStructure.CreateStructure(typeData, data_string);
                if(newEnt != null) MapController.controller.entities.Add(newEnt);
                break;
            case MainController.DataType.Machine:
                typeData = AssetLoader.loaded_machines[type_ID];
                newEnt = AbstractMachine.CreateMachine(typeData, data_string);
                if(newEnt != null) MachineController.controller.entities.Add(newEnt);
                break;
            case MainController.DataType.Mob:
                typeData = AssetLoader.loaded_mobs[type_ID];
                newEnt = AbstractMob.CreateMob(typeData, data_string);
                if(newEnt != null)
                {
                    if(newEnt is Behaviors.AbstractObserver || newEnt is Behaviors.AbstractMapEditor)
                    {
                        MobController.controller.ghost_entities.Add(newEnt);
                    }
                    else
                    {
                        MobController.controller.living_entities.Add(newEnt);
                    }
                }
                break;
        }
        // NetworkEntity init
        if(newEnt == null)
        {
            GD.PrintErr("INVALID SPAWN, TYPE: " + type + " AS: " + type_ID);
            return null;
        }
        newEnt.GridPos = new GridPos("NULL",0,0,0); // nullspace till placed
        newEnt.TemplateRead(typeData);
        // Automove to location
        if(pos != null) AbstractTools.Move(newEnt,pos.Value,false);
        // Init, handles basic object spawning!
        if(!suppress_init)
        {
            newEnt.Init();
            newEnt.LateInit();
            newEnt.UpdateIcon();
        }
        return newEnt;
    }

    public static void DeleteEntity(AbstractEntity entity)
    {
        switch(entity.EntityType)
        {
            case MainController.DataType.Area:
                MapController.controller.areas.Remove(entity.GetUniqueID);
                break;
            case MainController.DataType.Turf:
                break;
            case MainController.DataType.Effect:
                if(entity is Behaviors.AbstractSpawner) MapController.controller.spawners[(entity as AbstractEffect).GetTag()].Remove(entity as AbstractEffect);
                MapController.controller.effects.Remove(entity as AbstractEffect);
                break;
            case MainController.DataType.Item:
                MapController.controller.entities.Remove(entity);
                break;
            case MainController.DataType.Structure:
                MapController.controller.entities.Remove(entity);
                break;
            case MainController.DataType.Machine:
                MachineController.controller.entities.Remove(entity);
                break;
            case MainController.DataType.Mob:
                MobController.controller.living_entities.Remove(entity);
                MobController.controller.dead_entities.Remove(entity);
                MobController.controller.ghost_entities.Remove(entity);
                break;
        }
        if(entity.GetClientOwner() != null)
        {
            entity.GetClientOwner().ClearFocusedEntity();
            entity.ClearClientOwner();
        }
        entity.UnloadNetworkEntity();
        if(entity is not AbstractTurf) AbstractTools.Move(entity);
    }
}
