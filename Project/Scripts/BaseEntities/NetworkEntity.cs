using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[GlobalClass] 
public partial class NetworkEntity : Node3D
{
    public enum EntityType
    {
        Area,
        Turf,
        Item,
        Structure,
        Machine,
        Mob
    }

    private EntityType entity_type;
    public static NetworkEntity CreateEntity(string mapID, EntityType type)
    {
        NetworkEntity newEnt = null;
        switch(type)
        {
            case EntityType.Area:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkArea.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                MapController.areas.Add(newEnt as NetworkArea);
                break;
            case EntityType.Turf:
                // Add to processing list is handled by the turf's creation in MapController.AddTurf()
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkTurf.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                break;
            case EntityType.Item:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkItem.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                break;
            case EntityType.Structure:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkStructure.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                break;
            case EntityType.Machine:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkMachine.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                MachineController.entities.Add(newEnt);
                break;
            case EntityType.Mob:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkMob.tscn").Instantiate() as NetworkEntity;
                newEnt.entity_type = type;
                MobController.entities.Add(newEnt);
                break;
        }
        // Entity init
        newEnt.id = next_entity_id++;
        newEnt.map_id_string = mapID;
        newEnt.Init();

        // Finally add!
        MainController.controller.entity_container.AddChild(newEnt,true);
        return newEnt;
    }

    [Export]
    public string map_id_string;
    
    [Export]
    public long id = 0;
    [Export]
    private static long next_entity_id = 0;
    [Export]
    public Vector3 velocity = Vector3.Zero;
    [Export]
    public bool hidden;                 // doesn't render
    [Export]
    public bool density;                // blocks movement
    [Export]
    public bool occludes;               // blocks vision


    NetworkEntity location; // Current NetworkEntity that this entity is inside of, including turf.
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

    public virtual void UpdateIcon()    // It's tradition~ Pushes graphical state changes.
    {
        
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
        if(location is not NetworkTurf) 
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

    public void Move(string new_mapID, Vector3 new_pos, bool perform_turf_actions = true)
    {
        // If on same turf, don't bother with entrance/exit actions.
        if(MapController.FormatWorldPosition(map_id_string,Position) == MapController.FormatWorldPosition(new_mapID,new_pos)) return;
        // Leave old turf
        NetworkTurf old_turf = MapController.GetTurfAtPosition(map_id_string,Position);
        old_turf.EntityExited(this,perform_turf_actions);
        // Enter new turf
        map_id_string = new_mapID;
        Position = new_pos;
        NetworkTurf new_turf = MapController.GetTurfAtPosition(map_id_string,Position);
        new_turf.EntityEntered(this,perform_turf_actions);
    }

    public void Kill()
    {
        switch(entity_type)
        {
            case EntityType.Area:
                MapController.areas.Remove(this as NetworkArea);
                break;
            case EntityType.Turf:
                MapController.RemoveTurf(this as NetworkTurf, false);
                break;
            case EntityType.Item:
                MapController.entities.Remove(this);
                break;
            case EntityType.Structure:
                MapController.entities.Remove(this);
                break;
            case EntityType.Machine:
                MachineController.entities.Remove(this);
                break;
            case EntityType.Mob:
                MobController.entities.Remove(this);
                break;
        }
        QueueFree();
    }

    public NetworkTurf GetTurf()
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
        ent.location = this;
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
    }

    public virtual void Crossed(NetworkEntity crosser)
    {
        
    }

    public virtual void UnCrossed(NetworkEntity crosser)
    {
        
    }
}
