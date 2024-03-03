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
    public string texture = "Error.png";
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
            case MainController.DataType.Turf:
                newEnt = new AbstractTurf();
                newEnt.entity_type = type;
                typeData = AssetLoader.loaded_turfs[type_ID];
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
    public void UpdateIcon()    // It's tradition~ Pushes graphical state changes.
    {
        // Ask our behavior for info!
        behavior_type?.UpdateIcon(this, entity_type);
    }
    public virtual void Crossed(NetworkEntity crosser)
    {
        behavior_type?.Crossed( this, entity_type, crosser);
    }
    public virtual void Crossed(AbstractEntity crosser)
    {
        behavior_type?.Crossed( this, entity_type, crosser);
    }
    public virtual void UnCrossed(NetworkEntity crosser)
    {
        behavior_type?.UnCrossed( this, entity_type, crosser);
    }
    public virtual void UnCrossed(AbstractEntity crosser)
    {
        behavior_type?.UnCrossed( this, entity_type, crosser);
    }

    /*****************************************************************
     * Processing
     ****************************************************************/
    public NetworkEntity Realize()
    {
        // Spawns the NetworkEntity version of the object... DOES NOT ADD TO PROCESSING LISTS

        return new NetworkEntity();
    }
    public void Process()
    {
        // Handle the tick!
        Tick();
    }
    public void Kill()
    {
        switch(entity_type)
        {
            case MainController.DataType.Turf:
                break;
        }
    }

    /*****************************************************************
     * Movement and storage
     ****************************************************************/
    public string map_id_string;
    public AbstractTurf GetTurf()
    {
        return MapController.GetTurfAtPosition(map_id_string,grid_pos);
    }

    AbstractEntity abs_location = null; 
    NetworkEntity ent_location = null; 
    private List<AbstractEntity> stored_abstracts = new List<AbstractEntity>();
    private List<NetworkEntity> stored_entities = new List<NetworkEntity>();
    public List<AbstractEntity> StoredAbstracts
    {
        get {return stored_abstracts;}
    }
    public List<NetworkEntity> StoredEntities
    {
        get {return stored_entities;}
    }
    
    public void EnterLocation(AbstractEntity absLoc)
    {
        ent_location = null;
        abs_location = absLoc;
    }
    public void EnterLocation(NetworkEntity entLoc)
    {
        ent_location = entLoc;
        abs_location = null;
    }
    public void ClearLocation()
    {
        ent_location = null;
        abs_location = null;
    }
    
    private void LeaveOldLoc(bool perform_turf_actions)
    {
        if(ent_location != null)
        {
            NetworkEntity old_ent = ent_location as NetworkEntity;
            old_ent.EntityExited(this,perform_turf_actions);
        }
        if(abs_location != null)
        {
            // Leave old turf
            AbstractTurf old_turf = abs_location as AbstractTurf;
            old_turf.EntityExited(this,perform_turf_actions);
        }
    }

    public void Move(string new_mapID, MapController.GridPos new_pos, bool perform_turf_actions = true)
    {
        Move(new_mapID, TOOLS.GridToPosWithOffset(new_pos), perform_turf_actions);
    }
    public void Move(string new_mapID, Vector3 new_pos, bool perform_turf_actions = true)
    {
        // If on same turf, don't bother with entrance/exit actions.
        if( ent_location == null && grid_pos.Equals( new MapController.GridPos(new_pos)) && new_mapID == map_id_string) return;
        // Leave old location, perform uncrossing events!
        LeaveOldLoc(perform_turf_actions);
        // Enter new turf
        map_id_string = new_mapID;
        grid_pos = new MapController.GridPos(new_pos);
        AbstractTurf new_turf = MapController.GetTurfAtPosition(map_id_string,grid_pos);
        new_turf.EntityEntered(this,perform_turf_actions);
    }
    public void Move(NetworkEntity new_container, bool perform_turf_actions = true)
    {
        // If in same container, don't bother with entrance/exit actions.
        if( ent_location == new_container) return;
        // Leave old location, perform uncrossing events!
        LeaveOldLoc(perform_turf_actions);
        // Enter new location
        map_id_string = "BAG";
        new_container.EntityEntered(this,perform_turf_actions);
    }

    // Another entity has entered us...
    public void EntityEntered(NetworkEntity ent, bool perform_action)
    {
        if(perform_action)
        {
            for(int i = 0; i < stored_abstracts.Count; i++) 
            {
                stored_abstracts[i].Crossed(ent);
            }
            for(int i = 0; i < stored_entities.Count; i++) 
            {
                stored_entities[i].Crossed(ent);
            }
        }
        // Network entity 
        stored_entities.Add(ent);
        ent.EnterLocation(this);
    }
    public void EntityEntered(AbstractEntity abs, bool perform_action)
    {
        if(perform_action)
        {
            for(int i = 0; i < stored_abstracts.Count; i++) 
            {
                stored_abstracts[i].Crossed(abs);
            }
            for(int i = 0; i < stored_entities.Count; i++) 
            {
                stored_entities[i].Crossed(abs);
            }
        }
        // Network entity 
        stored_abstracts.Add(abs);
        abs.EnterLocation(this);
    }
    // An entity stored inside us has gone somewhere else!
    public void EntityExited(NetworkEntity ent, bool perform_action)
    {
        stored_entities.Remove(ent);
        if(perform_action)
        {
            for(int i = 0; i < stored_abstracts.Count; i++) 
            {
                stored_abstracts[i].UnCrossed(ent);
            }
            for(int i = 0; i < stored_entities.Count; i++) 
            {
                stored_entities[i].UnCrossed(ent);
            }
        }
        ent.ClearLocation();
    }
    public void EntityExited(AbstractEntity abs, bool perform_action)
    {
        stored_abstracts.Remove(abs);
        if(perform_action)
        {
            for(int i = 0; i < stored_abstracts.Count; i++) 
            {
                stored_abstracts[i].UnCrossed(abs);
            }
            for(int i = 0; i < stored_entities.Count; i++) 
            {
                stored_entities[i].UnCrossed(abs);
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
}
