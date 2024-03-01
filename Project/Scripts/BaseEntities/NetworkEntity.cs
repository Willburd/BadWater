using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

[GlobalClass] 
public partial class NetworkEntity : Node3D
{
    // Beginning of template data
    protected PackRef PackRef;
    public virtual void ApplyMapCustomData(Godot.Collections.Dictionary data)
    {
        // Update our template with newly set variables
        PackData template_data = AssetLoader.GetPackFromModID(PackRef).Clone();
        template_data.SetVars(data); // Override with custom set!
        TemplateRead(template_data);
    }
    public virtual void TemplateRead(PackData data)
    {
        PackRef = new PackRef(data);
    }
    // End of template data
    public string GetUniqueID
    {
        get { return PackRef.modid; }
    }

    private MainController.DataType entity_type;
    public static NetworkEntity CreateEntity(string mapID, string type_ID, MainController.DataType type)
    {
        PackData typeData = null;
        NetworkEntity newEnt = null;
        switch(type)
        {
            case MainController.DataType.Area:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkArea.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                typeData = AssetLoader.loaded_areas[type_ID];
                break;
            case MainController.DataType.Chunk:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkChunk.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                // Data type for these is weird!
                //MapController.loaded_chunks.Add(newEnt);
                break;
            case MainController.DataType.Effect:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkEffect.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                typeData = AssetLoader.loaded_effects[type_ID];
                break;
            case MainController.DataType.Item:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkItem.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                typeData = AssetLoader.loaded_items[type_ID];
                break;
            case MainController.DataType.Structure:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkStructure.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                typeData = AssetLoader.loaded_structures[type_ID];
                break;
            case MainController.DataType.Machine:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkMachine.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                MachineController.entities.Add(newEnt);
                typeData = AssetLoader.loaded_machines[type_ID];
                break;
            case MainController.DataType.Mob:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkMob.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                MobController.entities.Add(newEnt);
                typeData = AssetLoader.loaded_mobs[type_ID];
                break;
        }
        // NetworkEntity init
        newEnt.map_id_string = mapID;
        newEnt.TemplateRead(typeData);
        // Finally add to entity container.
        MainController.controller.entity_container.AddChild(newEnt,true);
        return newEnt;
    }

    [Export]
    public string map_id_string;
    [Export]
    public Vector3 velocity = Vector3.Zero;


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

    // Current NetworkEntity that this entity is inside of, including turf.
    protected AbstractEntity abs_location = null;
    protected NetworkEntity ent_location = null; 

    private List<NetworkEntity> contains_entities = new List<NetworkEntity>();
    public List<NetworkEntity> Contains
    {
        get {return contains_entities;}
    }


    public virtual void Init()          // Called upon creation to set variables or state, usually detected by map information.
    {
        
    }

    public virtual void Tick()          // Called every process tick on the Fire() tick of the subcontroller that owns them
    {
        
    }

    // RPC stuff commented so I have the formatting.
    //[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public virtual void UpdateIcon()    // It's tradition~ Pushes graphical state changes.
    {



        //Rpc(nameof(UpdateIcon));
    }

    public void Process()
    {
        // Handle the tick!
        Tick();
        ProcessVelocity();
    }

    private void ProcessVelocity()
    {
        // Containers don't update velocity, and don't move...
        if(ent_location != null) 
        {
            velocity *= 0;
            return;
        }
        // Update position if it has velocity
        if(velocity.Length() < 0.01) velocity *= 0;
        if(velocity != Vector3.Zero)
        {
            Move(map_id_string, Position + velocity);
        }
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
    public void Move(string new_mapID, Vector3 new_pos, bool perform_turf_actions = true)
    {
        // If on same turf, don't bother with entrance/exit actions.
        if( ent_location == null && new MapController.GridPos(Position).Equals( new MapController.GridPos(new_pos)) && new_mapID == map_id_string) return;
        // Leave old location, perform uncrossing events!
        LeaveOldLoc(perform_turf_actions);
        // Enter new turf
        map_id_string = new_mapID;
        Position = new_pos;
        AbstractTurf new_turf = MapController.GetTurfAtPosition(map_id_string,Position);
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

    public void Kill()
    {
        switch(entity_type)
        {
            case MainController.DataType.Area:
                MapController.areas.Remove(this.GetUniqueID);
                break;
            case MainController.DataType.Chunk:
                // MapController.loaded_chunks.Remove(this); // TODO - CHUNK DELETION
                break;
            case MainController.DataType.Effect:
                MapController.all_effects.Remove(this as NetworkEffect);
                if((this as NetworkEffect).spawner_id != "")
                {
                    MapController.spawners[(this as NetworkEffect).spawner_id].Remove(this as NetworkEffect);
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
        QueueFree();
    }

    public AbstractTurf GetTurf()
    {
        return MapController.GetTurfAtPosition(map_id_string,Position);
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

    public virtual void Crossed(NetworkEntity crosser)
    {
        
    }

    public virtual void UnCrossed(NetworkEntity crosser)
    {
        
    }
    

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
