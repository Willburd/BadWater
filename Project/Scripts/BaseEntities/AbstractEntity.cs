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
    private Behavior behavior_type;

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

    public string map_id_string;

    AbstractEntity location = null; // Current NetworkEntity that this entity is inside of, including turf.
    private List<NetworkEntity> contains_entities = new List<NetworkEntity>();
    public List<NetworkEntity> Contains
    {
        get {return contains_entities;}
    }

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

    public AbstractTurf GetTurf()
    {
        return MapController.GetTurfAtPosition(map_id_string,grid_pos);
    }


    public void EntityEntered(NetworkEntity ent, bool perform_action)
    {
        if(perform_action)
        {
            for(int i = 0; i < contains_entities.Count; i++) 
            {
                contains_entities[i].Crossed(ent);
            }
        }
        
        contains_entities.Add(ent);
        ent.EnterLocation(this);
    }
    public void EntityExited(NetworkEntity ent, bool perform_action)
    {
        contains_entities.Remove(ent);
        if(perform_action)
        {
            for(int i = 0; i < contains_entities.Count; i++) 
            {
                contains_entities[i].UnCrossed(ent);
            }
        }
        ent.ClearLocation();
    }


    
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
