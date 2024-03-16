using Behaviors_BASE;
using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;


public partial class AbstractEntity
{
    // Beginning of template data
    protected PackRef PackRef;
    protected MapController.GridPos grid_pos;
    protected NetworkClient owner_client;
    public MapController.GridPos GridPos
    {
        get {return grid_pos;}
    }
    public virtual void ApplyMapCustomData(Godot.Collections.Dictionary data)
    {
        // Update our template with newly set variables
        PackData template_data = TemplateWrite();
        template_data.SetVars(data); // Override with custom set!
        TemplateRead(template_data);
        MapLoadVars(data);
    }
    public virtual void MapLoadVars(Godot.Collections.Dictionary data)
    {
        
    }
    public virtual void TemplateRead(PackData data)
    {
        PackRef = new PackRef( data, entity_type);
        SetTag(data.tag);
        display_name = data.display_name;
        description = data.description;
        intangible = data.intangible;
        model = data.model;
        texture = data.texture;
        anim_speed = data.anim_speed;
        attack_range = data.attack_range;
        unstoppable = data.unstoppable;
    }
    public PackData TemplateWrite()
    {
        PackData data = null;
        switch(entity_type)
        {
            case MainController.DataType.Map:
                data = new MapData();
            break;
            case MainController.DataType.Area:
                data = new AreaData();
            break;
            case MainController.DataType.Turf:
                data = new TurfData();
            break;
            case MainController.DataType.Effect:
                data = new EffectData();
            break;
            case MainController.DataType.Item:
                data = new ItemData();
            break;
            case MainController.DataType.Structure:
                // data = new StructureData();
            break;
            case MainController.DataType.Machine:
                // data = new MachineData();
            break;
            case MainController.DataType.Mob:
                data = new MobData();
            break;
        }
        data.Clone(AssetLoader.GetPackFromRef(PackRef));
        return data;
    }
    public string tag = "";
    public string model = "BASE/Turf/Plane.tscn";
    public string texture = "";
    public double anim_speed = 0;
    public string icon_state = "Idle";
    
    public DAT.Dir direction = DAT.Dir.South;
    public string display_name;
    public string description;
    public bool density = false;              // blocks movement
    public bool opaque = false;               // blocks vision
    public bool intangible = false;           // can move through solids
    public string step_sound = "";              // Sound pack ID for steps
    public int attack_range = 1;
    public bool unstoppable = false; 
    // End of template data

    // state data
    private float last_bump_time = 0;
    private const float bump_reset_time = 30; // Ticks
    public string GetUniqueID
    {
        get { return PackRef.modid; }
    }
    public virtual DAT.ZoneSelection SelectingZone
    {
        get {return DAT.ZoneSelection.UpperBody;}
    }
    private DAT.Intent internal_selecting_intent = DAT.Intent.Help;
    public DAT.Intent SelectingIntent
    {
        get {return internal_selecting_intent;}
    }
    protected MainController.DataType entity_type;
    // end state data

    public static AbstractEntity CreateEntity(string mapID, string type_ID, MainController.DataType type)
    {
        PackData typeData = null;
        AbstractEntity newEnt = null;
        switch(type)
        {
            case MainController.DataType.Area:
                typeData = AssetLoader.loaded_areas[type_ID];
                newEnt = new AbstractArea();
                newEnt.entity_type = type;
                break;
            case MainController.DataType.Turf:
                typeData = AssetLoader.loaded_turfs[type_ID];
                newEnt = AbstractTurf.CreateTurf(typeData);
                newEnt.entity_type = type;
                break;
            case MainController.DataType.Effect:
                typeData = AssetLoader.loaded_effects[type_ID];
                newEnt = new AbstractEffect();
                newEnt.entity_type = type;
                MapController.effects.Add(newEnt as AbstractEffect);
                break;
            case MainController.DataType.Item:
                typeData = AssetLoader.loaded_items[type_ID];
                newEnt = AbstractItem.CreateItem(typeData);
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                break;
            case MainController.DataType.Structure:
                typeData = AssetLoader.loaded_structures[type_ID];
                newEnt = AbstractStructure.CreateStructure(typeData);
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                break;
            case MainController.DataType.Machine:
                typeData = AssetLoader.loaded_machines[type_ID];
                newEnt = AbstractMachine.CreateMachine(typeData);
                newEnt.entity_type = type;
                MachineController.entities.Add(newEnt);
                break;
            case MainController.DataType.Mob:
                typeData = AssetLoader.loaded_mobs[type_ID];
                newEnt = AbstractMob.CreateMob(typeData);
                newEnt.entity_type = type;
                MobController.entities.Add(newEnt);
                break;
        }
        // NetworkEntity init
        newEnt.map_id_string = mapID;
        newEnt.TemplateRead(typeData);
        return newEnt;
    }

    /*****************************************************************
     * Behavior hooks
     ****************************************************************/
    private NetworkEntity loaded_entity;    // Puppet this
    public NetworkEntity LoadedNetworkEntity
    {
        get {return loaded_entity;}
    }
    public virtual void Init() {} // Called upon creation to set variables or state, usually detected by map information.
    public void LateInit() { } // Same as above, but when we NEED everything else Init() before we can properly tell our state!
    public virtual void Tick() { } // Called every process tick on the Fire() tick of the subcontroller that owns them
    public virtual void Crossed(AbstractEntity crosser) { }
    public virtual void UnCrossed(AbstractEntity crosser) { }
    public virtual void UpdateIcon() // Remember to call base.UpdateIcon() to handle transmitting data to the clients!
    { 
        // It's tradition~ Pushes graphical state changes.
        UpdateNetwork(true);
    } 
    public void Bump(AbstractEntity hitby) // When we are bumped by an incoming entity
    {
        if(MainController.WorldTicks <= last_bump_time + bump_reset_time) return;
        last_bump_time = MainController.WorldTicks;
    }

    /*****************************************************************
     * Processing
     ****************************************************************/
    public Vector3 velocity = Vector3.Zero;
    public void Process()
    {
        // Handle the tick!
        DAT.Dir old_dir = direction;
        Tick();
        ProcessVelocity();
        UpdateNetworkDirection(old_dir);
    }
    private void ProcessVelocity()
    {
        // Containers don't update velocity, and don't move...
        if(location != null) 
        {
            velocity *= 0;
            return;
        }
        // Update position if it has velocity
        if(velocity.Length() < 0.01) velocity *= 0;
        if(velocity != Vector3.Zero)
        {
            Move(map_id_string, TOOLS.GridToPosWithOffset(grid_pos) + velocity);
        }
    }
    public void Kill()
    {
        UnloadNetworkEntity();
        switch(entity_type)
        {
            case MainController.DataType.Area:
                MapController.areas.Remove(this.GetUniqueID);
                break;
            case MainController.DataType.Turf:
                break;
            case MainController.DataType.Effect:
                MapController.effects.Remove(this as AbstractEffect);
                if((this as AbstractEffect).is_spawner)
                {
                    MapController.spawners[(this as AbstractEffect).GetTag()].Remove(this as AbstractEffect);
                }
                break;
            case MainController.DataType.Item:
                MapController.entities.Remove(this);
                break;
            case MainController.DataType.Structure:
                MapController.entities.Remove(this);
                break;
            case MainController.DataType.Machine:
                MachineController.entities.Remove(this);
                break;
            case MainController.DataType.Mob:
                MobController.entities.Remove(this);
                break;
        }
        owner_client?.ClearFocusedEntity();
        ClearClientOwner();
    }
    public void UnloadNetworkEntity()
    {
        loaded_entity?.Kill();
        loaded_entity = null;
    }

    /*****************************************************************
     * Client input and control
     ****************************************************************/
    public virtual AbstractEntity ActiveHand // Used to get activehand from mob, but in any other entity...
    {
        get {return null;}
        set {}
    }
    public void SetClientOwner(NetworkClient client)
    {
        Debug.Assert(client != null);
        owner_client = client;
    }
    public void ClearClientOwner()
    {
        owner_client = null;
    }
    public virtual void ControlUpdate(Godot.Collections.Dictionary client_input_data)
    {
        DAT.Dir old_dir = direction;
        if(client_input_data.Keys.Count == 0) return;
        // Got an actual control update!
        MapController.GridPos new_pos = grid_pos;
        double dat_x = client_input_data["x"].AsDouble() * MainController.controller.config.input_factor;
        double dat_y = client_input_data["y"].AsDouble() * MainController.controller.config.input_factor;
        new_pos.hor += (float)dat_x;
        new_pos.ver += (float)dat_y;
        Move(map_id_string, new_pos);
        UpdateNetworkDirection(old_dir);
    }
    // Clicking other entities
    public virtual void Clicked( AbstractEntity used_entity, AbstractEntity target, Godot.Collections.Dictionary click_params) 
    {
        GD.Print(display_name + " CLICKED " + target?.display_name + " USING " + used_entity?.display_name); // REPLACE ME!!!
    }
    // Being dragged by other entities to somewhere else
    public virtual void Dragged( AbstractEntity user, AbstractEntity target,Godot.Collections.Dictionary click_params) 
    { 
        GD.Print(user?.display_name + " DRAGGED " + display_name + " TO " + target?.display_name); // REPLACE ME!!!
    }

    /*****************************************************************
     * Attack handling
     ****************************************************************/
    // Base attack logic
    public bool Attack( AbstractEntity user, AbstractEntity target, float attack_modifier, Godot.Collections.Dictionary click_parameters)
    {
        if(PreAttack( user, target,click_parameters)) return true; // PreAttack returns true if it performs a unique action that does not actually cause an attack
        return target.AttackedBy( user, this, attack_modifier, click_parameters);
    }

    protected bool AttackedBy( AbstractEntity user, AbstractEntity used_entity, float attack_modifier, Godot.Collections.Dictionary click_parameters)
    {
        if(user is not AbstractMob) return false;
        if(used_entity is AbstractItem used_item && this is AbstractComplexMob this_complexmob)
        {
            /*if(can_operate(this_complexmob, user) && used_item.do_surgery(this_complexmob,user,user.SelectingZone))
                return TRUE*/  // TODO - Surgery hook! =================================================================================================================================
        }
        return used_entity.WeaponAttack( user, this, user.SelectingZone, attack_modifier);
    }
    
    public virtual bool AttackCanReach(AbstractMob user, AbstractEntity target, int range)
    {
        if(TOOLS.Adjacent(user,target)) return true; // Already adjacent. TODO Handle corners and walls =================================================================================================================================
        return false;
    }
    protected virtual bool WeaponAttack( AbstractEntity user, AbstractEntity target, DAT.ZoneSelection target_zone, float attack_modifier)
    {
        if(target == user && user.SelectingIntent != DAT.Intent.Hurt) return false;
        if(user is AbstractMob user_mob)
        {
            /////////////////////////
            user_mob.lastattacked = target;
            if(target is AbstractMob) (target as AbstractMob).lastattacker = user;
            //add_attack_logs(user,M,"attacked with [name] (INTENT: [uppertext(user.a_intent)]) (DAMTYE: [uppertext(damtype)])");
            /////////////////////////
            user_mob.SetClickCooldown( user_mob.GetAttackCooldown(this) );
            // user_mob.DoAttackAnimation( target); // TODO - attack animation for items ======================================================================================
        }

        if(target is AbstractMob)
        {
            var hit_zone = (target as AbstractMob).ResolveItemAttack(this, user, target_zone);
            if(hit_zone != DAT.ZoneSelection.Miss)
            {
                //ApplyHitEffect(M, user, hit_zone, attack_modifier); // TODO ======================================================================================
                GD.Print(user.display_name + " WEAPON ATTACKED " + target.display_name + " USING " + display_name); // REPLACE ME!!!
            }
        }
        
        return true;
    }
    protected virtual DAT.ZoneSelection ResolveItemAttack(AbstractEntity user, AbstractEntity used_item, DAT.ZoneSelection target_zone)
    {
        return target_zone; // assumes hit... See overrides for proper implimentations. This is how items can miss mid attack.
    }


    // Overrides for responding to attacks
    public virtual void AttackSelf( AbstractEntity user ) 
    { 
        // What happens when an object is used on itself.
    }
    
    public virtual bool PreAttack( AbstractEntity user, AbstractEntity target, Godot.Collections.Dictionary click_parameters) 
    {
        return false; //return TRUE to avoid calling attackby after this proc does stuff
    }

    public virtual void AfterAttack( AbstractEntity user, AbstractEntity target, bool proximity, Godot.Collections.Dictionary click_parameters)
    {
        // What happens after an attack successfully hits.
    }

    public virtual void AttackTK( AbstractEntity user)
    {
        // What happens when telekinetically used.
    }

    /*****************************************************************
     * Movement and storage
     ****************************************************************/
    public string map_id_string;
    public AbstractTurf GetTurf()
    {
        return MapController.GetTurfAtPosition(map_id_string,grid_pos,true);
    }
    public AbstractEntity GetLocation()
    {
        if(location == null) return GetTurf();
        return location;
    }

    AbstractEntity location = null; 
    private List<AbstractEntity> contains = new List<AbstractEntity>();
    public List<AbstractEntity> Contents
    {
        get {return contains;}
    }
    
    private void EnterLocation(AbstractEntity absLoc)
    {
        Debug.Assert(absLoc != null);
        location = absLoc;
    }
    private void ClearLocation()
    {
        location = null;
    }
    
    private void LeaveOldLoc(bool perform_turf_actions)
    {
        if(location != null)
        {
            // Leave old turf
            AbstractTurf old_turf = location as AbstractTurf;
            old_turf.EntityExited(this,perform_turf_actions);
        }
    }

    public AbstractEntity Move(string new_mapID, MapController.GridPos new_pos, bool perform_turf_actions = true)
    {
        return Move(new_mapID, TOOLS.GridToPosWithOffset(new_pos), perform_turf_actions);
    }
    public AbstractEntity Move(string new_mapID, Vector3 new_pos, bool perform_turf_actions = true)
    {
        // Is new location valid?
        MapController.GridPos new_grid = new MapController.GridPos(new_pos);
        Vector2 dir_vec = TOOLS.DirVec( grid_pos.hor, grid_pos.ver, new_grid.hor, new_grid.ver);
        
        if(map_id_string == new_mapID)
        {
            // EDGE LOCK
            float threshold = (float)0.01;
            if(!MapController.IsTurfValid(new_mapID,new MapController.GridPos(new_grid.hor,grid_pos.ver,grid_pos.dep)))
            {
                if(dir_vec.X < 0)
                {   
                    new_grid.hor = Mathf.Floor(grid_pos.hor) + threshold;
                }
                else if(dir_vec.X > 0)
                {
                    new_grid.hor = Mathf.Floor(grid_pos.hor) + 1 - threshold;
                }
            }
            if(!MapController.IsTurfValid(new_mapID,new MapController.GridPos(grid_pos.hor,new_grid.ver,grid_pos.dep)))
            {
                if(dir_vec.Y < 0)
                {
                    new_grid.ver = Mathf.Floor(grid_pos.ver) + threshold;
                }
                else if(dir_vec.Y > 0)
                {
                    new_grid.ver = Mathf.Floor(grid_pos.ver) + 1 - threshold;
                }
            }
            
            if(!IsIntangible())
            {
                // Check to see on each axis if we bump... This allows sliding!
                bool bump_h = false;
                AbstractTurf hor_turf = MapController.GetTurfAtPosition(map_id_string,new MapController.GridPos(new_grid.hor,grid_pos.ver,grid_pos.dep),true);
                if(hor_turf != null && hor_turf != GetTurf() && hor_turf.density)
                {
                    bump_h = true;
                    if(dir_vec.X < 0)
                    {   
                        new_grid.hor = Mathf.Floor(grid_pos.hor) + threshold;
                    }
                    else if(dir_vec.X > 0)
                    {
                        new_grid.hor = Mathf.Floor(grid_pos.hor) + 1 - threshold;
                    }
                }
                bool bump_v = false;
                AbstractTurf ver_turf = MapController.GetTurfAtPosition(map_id_string,new MapController.GridPos(grid_pos.hor,new_grid.ver,grid_pos.dep),true);
                if(ver_turf != null && ver_turf != GetTurf() && ver_turf.density)
                {
                    bump_v = true;
                    if(dir_vec.Y < 0)
                    {
                        new_grid.ver = Mathf.Floor(grid_pos.ver) + threshold;
                    }
                    else if(dir_vec.Y > 0)
                    {
                        new_grid.ver = Mathf.Floor(grid_pos.ver) + 1 - threshold;
                    }
                }
                // Bump solids!
                AbstractTurf corner_turf = MapController.GetTurfAtPosition(map_id_string,new_grid,true);
                if(corner_turf != null && corner_turf.density)
                {
                    // Corner bonking is silly... Needs a unique case when you run into a corner exactly head on!
                    MapController.GridPos original_new = new_grid;
                    if(dir_vec.X < 0)
                    {   
                        new_grid.hor = Mathf.Floor(grid_pos.hor) + threshold;
                    }
                    else if(dir_vec.X > 0)
                    {
                        new_grid.hor = Mathf.Floor(grid_pos.hor) + 1 - threshold;
                    }
                    if(dir_vec.Y < 0)
                    {
                        new_grid.ver = Mathf.Floor(grid_pos.ver) + threshold;
                    }
                    else if(dir_vec.Y > 0)
                    {
                        new_grid.ver = Mathf.Floor(grid_pos.ver) + 1 - threshold;
                    }
                    Bump(corner_turf);
                    corner_turf.Bump(this);
                    
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
                    // check if the bonk is significant enough!
                    Vector3 distVec = new_grid.WorldPos() - grid_pos.WorldPos();
                    if(bump_h) // bonk horizontal.
                    {
                        Bump(hor_turf);
                        hor_turf.Bump(this);
                    }
                    if(bump_v) // bonk verticle.
                    {
                        Bump(ver_turf);
                        ver_turf.Bump(this);
                    }
                }
            }  
        } 
        
        // At same location still, don't bother with much else...
        if(location is AbstractTurf && grid_pos.Equals(new_grid) && new_mapID == map_id_string) 
        {
            // Move around in current turf
            map_id_string = new_mapID;
            grid_pos = new_grid;
            SyncPositionRotation(false);
            return location;
        }

        // Leave old location, perform uncrossing events! Enter new turf...
        LeaveOldLoc(perform_turf_actions);
        map_id_string = new_mapID;
        grid_pos = new_grid;
        // Enter new location!
        AbstractTurf new_turf = MapController.GetTurfAtPosition(map_id_string,grid_pos,true);
        new_turf?.EntityEntered(this,perform_turf_actions);
        UpdateNetwork(false);
        return location;
    }
    public AbstractEntity Move(AbstractEntity new_destination, bool perform_turf_actions = true)
    {
        // If in same container, don't bother with entrance/exit actions.
        if(location == new_destination) return location;
        if(new_destination is AbstractTurf)
        {
            // It's a turf! move normally!
            return Move(new_destination.map_id_string, new_destination.GridPos, perform_turf_actions);
        }
        // Leave old location, perform uncrossing events!
        LeaveOldLoc(perform_turf_actions);
        // Enter new location
        map_id_string = "BAG";
        new_destination.EntityEntered(this,perform_turf_actions);
        UpdateNetwork(false);
        return location;
    }
    public AbstractEntity Move(bool perform_turf_actions = true) // Move to nullspace
    {
        // Leave old location, perform uncrossing events!
        LeaveOldLoc(perform_turf_actions);
        // Enter new location
        map_id_string = "NULL";
        UpdateNetwork(false);
        return location;
    }
    public void Drop(AbstractEntity new_destination, AbstractEntity user)
    {
        Move(new_destination,true);
    }
    public void PickedUp(AbstractEntity new_destination, AbstractEntity user)
    {
        Move(new_destination,true);
    }

    // Another entity has entered us...
    public void EntityEntered(AbstractEntity abs, bool perform_action)
    {
        Debug.Assert(abs != null);
        if(perform_action)
        {
            for(int i = 0; i < contains.Count; i++) 
            {
                contains[i].Crossed(abs);
            }
        }
        // Network entity 
        contains.Add(abs);
        abs.EnterLocation(this);
    }
    // An entity stored inside us has gone somewhere else!
    public void EntityExited(AbstractEntity abs, bool perform_action)
    {
        Debug.Assert(abs != null);
        contains.Remove(abs);
        if(perform_action)
        {
            for(int i = 0; i < contains.Count; i++) 
            {
                contains[i].UnCrossed(abs);
            }
        }
        abs.ClearLocation();
    }

    //Returns the storage depth of an atom. This is the number of storage items the atom is contained in before reaching toplevel (the turf).
    //Returns -1 if the atom was not found in a container.
    public int StorageDepth(AbstractEntity container)
    {
        int depth = 0;
        AbstractEntity cur_entity = this;
        while(cur_entity != null && !container.contains.Contains(cur_entity))
        {
            if(cur_entity.location is AbstractTurf) return -1;
            cur_entity = cur_entity.location;
            depth++;
        }
        if(cur_entity == null) return -1;	//inside something with a null location.
        return depth;
    }

    /*****************************************************************
     * Tag control
     ****************************************************************/
    public void SetTag(string new_tag)
    {
        MapController.Internal_UpdateTag(this,new_tag);
        tag = new_tag;
    }
    public void ClearTag() { SetTag(""); }
    public string GetTag() { return tag; }

    /*****************************************************************
     * Network entity spawning/despawning and visibility
     ****************************************************************/
    public bool IsNetworkVisible()
    {
        if(location is AbstractTurf)
        {

            return true; // By default, visible on turfs.
        }
        if(location is AbstractMob)
        {
            // Handle.... HANDS... And other slots.
            // TODO - mob hands and other slots! =================================================================================================================================
            return false;
        }
        return false;
    }
    public void UpdateNetworkDirection(DAT.Dir old_dir)
    {
        if(old_dir != direction) UpdateNetwork(false);
    }
    public void UpdateNetwork(bool mesh_update) // Spawns and despawns currently loaded entity. While calling SyncPositionRotation(bool mesh_update) is cheaper... Calling this is safer.
    {
        if(MapController.IsChunkLoaded(map_id_string,grid_pos.ChunkPos())) 
        {
            if(this is AbstractTurf) 
            {
                // If turf, update mesh...
                MapController.GetChunk(map_id_string,grid_pos.ChunkPos()).MeshUpdate();
                return;
            }
            if(location == null) return; // nullspace vanish
            // Otherwise, what does our behavior say?
            bool is_vis = IsNetworkVisible();
            if(is_vis && loaded_entity == null)
            {
                loaded_entity = NetworkEntity.CreateEntity( this, map_id_string, entity_type);
                SyncPositionRotation(true);
                return;
            }
            if(!is_vis && loaded_entity != null)
            {
                UnloadNetworkEntity();
                return;
            }
            if(is_vis && loaded_entity != null)
            {
                SyncPositionRotation(mesh_update);
                return;
            }
        }
        else
        {
            // Entity loaded in an unloaded chunk?
            if(loaded_entity != null)
            {
                UnloadNetworkEntity();
                return;
            }
        }
    }
    private void SyncPositionRotation(bool mesh_update) // Updates position and rotation of currently loaded entity
    {
        if(loaded_entity == null) return;
        loaded_entity.Position = grid_pos.WorldPos();
        loaded_entity.direction = direction;
        if(mesh_update) loaded_entity.MeshUpdate();
    }


    /*****************************************************************
     * Conditions
     ****************************************************************/
    public virtual bool IsIntangible()
    {
        return intangible;
    }
}
