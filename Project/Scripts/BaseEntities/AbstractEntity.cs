using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;


public partial class AbstractEntity
{
    // Beginning of template data
    protected PackRef PackRef;
    public MapController.GridPos grid_pos; 
    private NetworkEntity loaded_entity; // Puppet this
    public virtual void ApplyMapCustomData(Godot.Collections.Dictionary data)
    {
        // Update our template with newly set variables
        PackData template_data = TemplateWrite();
        template_data.SetVars(data); // Override with custom set!
        TemplateRead(template_data);
    }
    public virtual void TemplateRead(PackData data)
    {
        PackRef = new PackRef( data, entity_type);
        SetBehavior(Behavior.CreateBehavior(data));
        SetTag(data.tag);
        model = data.model;
        texture = data.texture;
        anim_speed = data.anim_speed;
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
                // data = new MobData();
            break;
        }
        data.Clone(AssetLoader.GetPackFromRef(PackRef));
        return data;
    }
    public string tag = "";
    public string model = "Plane";
    public string texture = "";
    public double anim_speed = 0;
    public bool density = false;              // blocks movement
    public bool opaque = false;               // blocks vision
    public bool intangible = false;           // can move through solids
    // End of template data
    public string GetUniqueID
    {
        get { return PackRef.modid; }
    }
    protected MainController.DataType entity_type;
    public static AbstractEntity CreateEntity(string mapID, string type_ID, MainController.DataType type)
    {
        PackData typeData = null;
        AbstractEntity newEnt = null;
        switch(type)
        {
            case MainController.DataType.Area:
                newEnt = new AbstractArea();
                newEnt.entity_type = type;
                typeData = AssetLoader.loaded_areas[type_ID];
                break;
            case MainController.DataType.Turf:
                newEnt = new AbstractTurf();
                newEnt.entity_type = type;
                typeData = AssetLoader.loaded_turfs[type_ID];
                break;
            case MainController.DataType.Effect:
                newEnt = new AbstractEffect();
                newEnt.entity_type = type;
                MapController.effects.Add(newEnt as AbstractEffect);
                typeData = AssetLoader.loaded_effects[type_ID];
                break;
            case MainController.DataType.Item:
                newEnt = new AbstractItem();
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                typeData = AssetLoader.loaded_items[type_ID];
                break;
            case MainController.DataType.Structure:
                newEnt = new AbstractStructure();
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                typeData = AssetLoader.loaded_structures[type_ID];
                break;
            case MainController.DataType.Machine:
                newEnt = new AbstractMachine();
                newEnt.entity_type = type;
                MachineController.entities.Add(newEnt);
                typeData = AssetLoader.loaded_machines[type_ID];
                break;
            case MainController.DataType.Mob:
                newEnt = new AbstractMob();
                newEnt.entity_type = type;
                MobController.entities.Add(newEnt);
                typeData = AssetLoader.loaded_mobs[type_ID];
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
    private Behavior behavior_type;
    public void SetBehavior(Behavior set_behavior)
    {
        behavior_type = set_behavior;
    }
    public void Init()          // Called upon creation to set variables or state, usually detected by map information.
    {
        behavior_type?.Init(this, entity_type);
    }
    public void LateInit()      // Same as above, but when we NEED everything else Init() before we can properly tell our state!
    {
        behavior_type?.LateInit(this, entity_type);
    }
    public void Tick()                  // Called every process tick on the Fire() tick of the subcontroller that owns them
    {
        // Ask our behavior for info!
        behavior_type?.Tick(this, entity_type);
    }
    public virtual void SyncNetwork(bool include_mesh)
    {
        if(loaded_entity == null) return;
        loaded_entity.Position = grid_pos.WorldPos();
        if(include_mesh) loaded_entity.MeshUpdate();
    }
    public void UpdateIcon()    // It's tradition~ Pushes graphical state changes.
    {
        // Ask our behavior for info!
        behavior_type?.UpdateIcon(this, entity_type);
        UpdateNetworkVisibility();
    }

    public virtual void Crossed(AbstractEntity crosser)
    {
        behavior_type?.Crossed( this, entity_type, crosser);
    }

    public virtual void UnCrossed(AbstractEntity crosser)
    {
        behavior_type?.UnCrossed( this, entity_type, crosser);
    }
    
    public void Bump(AbstractEntity hitby) // When we are bumped by an incoming entity
    {
        behavior_type?.Bump( this, entity_type, hitby);
    }


    /*****************************************************************
     * Processing
     ****************************************************************/
    public Vector3 velocity = Vector3.Zero;
    public void Process()
    {
        // Handle the tick!
        Tick();
        ProcessVelocity();
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
    }

    public void UnloadNetworkEntity()
    {
        loaded_entity?.Kill();
        loaded_entity = null;
    }

    /*****************************************************************
     * Client input and control
     ****************************************************************/
    public void ControlUpdate(Godot.Collections.Dictionary client_input_data)
    {
        if(client_input_data.Keys.Count == 0) return;
        // Got an actual control update!
        MapController.GridPos new_pos = grid_pos;
        double dat_x = client_input_data["x"].AsDouble();
        double dat_y = client_input_data["y"].AsDouble();
        new_pos.hor += (float)dat_x;
        new_pos.ver += (float)dat_y;
        Move(map_id_string, new_pos);
    }

    /*****************************************************************
     * Movement and storage
     ****************************************************************/
    public string map_id_string;
    public AbstractTurf GetTurf()
    {
        return MapController.GetTurfAtPosition(map_id_string,grid_pos);
    }

    AbstractEntity location = null; 
    private List<AbstractEntity> contains = new List<AbstractEntity>();
    public List<AbstractEntity> Contents
    {
        get {return contains;}
    }
    
    private void EnterLocation(AbstractEntity absLoc)
    {
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

    public void Move(string new_mapID, MapController.GridPos new_pos, bool perform_turf_actions = true)
    {
        Move(new_mapID, TOOLS.GridToPosWithOffset(new_pos), perform_turf_actions);
    }
    public void Move(string new_mapID, Vector3 new_pos, bool perform_turf_actions = true)
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
            
            if(!intangible)
            {
                // Check to see on each axis if we bump... This allows sliding!
                bool bump_h = false;
                AbstractTurf hor_turf = MapController.GetTurfAtPosition(map_id_string,new MapController.GridPos(new_grid.hor,grid_pos.ver,grid_pos.dep));
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
                AbstractTurf ver_turf = MapController.GetTurfAtPosition(map_id_string,new MapController.GridPos(grid_pos.hor,new_grid.ver,grid_pos.dep));
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
                AbstractTurf corner_turf = MapController.GetTurfAtPosition(map_id_string,new_grid);
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
                    Vector3 distVec = new_grid.WorldPos() - grid_pos.WorldPos();
                    if(distVec.Length() > 0.01)
                    {
                        Bump(corner_turf);
                        corner_turf.Bump(this);
                    }
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
                    if(bump_h && distVec.X > 0.01) // bonk horizontal.
                    {
                        Bump(hor_turf);
                        hor_turf.Bump(this);
                    }
                    if(bump_v && distVec.Z > 0.01) // bonk verticle.
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
            SyncNetwork(false);
            return;
        }

        // Leave old location, perform uncrossing events! Enter new turf...
        LeaveOldLoc(perform_turf_actions);
        map_id_string = new_mapID;
        grid_pos = new_grid;
        // Enter new location!
        AbstractTurf new_turf = MapController.GetTurfAtPosition(map_id_string,grid_pos);
        new_turf?.EntityEntered(this,perform_turf_actions);
        SyncNetwork(false);
    }
    public void Move(AbstractEntity new_container, bool perform_turf_actions = true)
    {
        // If in same container, don't bother with entrance/exit actions.
        if(location == new_container) return;
        // Leave old location, perform uncrossing events!
        LeaveOldLoc(perform_turf_actions);
        // Enter new location
        map_id_string = "BAG";
        new_container.EntityEntered(this,perform_turf_actions);
        SyncNetwork(false);
    }
    public void Move(bool perform_turf_actions = true) // Move to nullspace
    {
        // Leave old location, perform uncrossing events!
        LeaveOldLoc(perform_turf_actions);
        // Enter new location
        map_id_string = "NULL";
        SyncNetwork(false);
    }

    // Another entity has entered us...
    public void EntityEntered(AbstractEntity abs, bool perform_action)
    {
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

    /*****************************************************************
     * Tag control
     ****************************************************************/
    public void SetTag(string new_tag)
    {
        MapController.Internal_UpdateTag(this,new_tag);
        tag = new_tag;
    }
    public void ClearTag()
    {
        SetTag("");
    }
    public string GetTag()
    {
        return tag;
    }

    /*****************************************************************
     * Network entity spawning/despawning and visibility
     ****************************************************************/
    protected virtual bool IsNetworkVisible()
    {
        if(location is AbstractTurf)
        {
            if(behavior_type != null) return behavior_type.IsNetworkVisible();
            return true; // By default, visible on turfs.
        }
        if(location is AbstractMob)
        {
            // Handle.... HANDS... And other slots.
            // TODO - mob hands and other slots!
            return false;
        }
        return false;
    }
    private void UpdateNetworkVisibility()
    {
        if(MapController.IsChunkLoaded(map_id_string,grid_pos.ChunkPos())) 
        {
            if(this is AbstractTurf) 
            {
                // If turf, update mesh...
                MapController.GetChunk(map_id_string,grid_pos.ChunkPos()).MeshUpdate();
                return;
            }
            else if(location == null) return; // nullspace vanish
            // Otherwise, what does our behavior say?
            bool is_vis = IsNetworkVisible();
            if(is_vis && loaded_entity == null)
            {
                loaded_entity = NetworkEntity.CreateEntity( this, map_id_string, entity_type);
                SyncNetwork(true);
            }
            if(!is_vis && loaded_entity != null)
            {
                UnloadNetworkEntity();
            }
        }
    }
}
