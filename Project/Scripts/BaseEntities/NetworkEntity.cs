using Godot;
using System;
using System.ComponentModel;

[GlobalClass] 
public partial class NetworkEntity : Node3D
{
    public enum EntityType
    {
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
            case EntityType.Turf:
                // Add to processing list is handled by the turf's creation in MapController.AddTurf()
                newEnt = new NetworkTurf();
                newEnt.entity_type = type;
                break;
            case EntityType.Item:
                newEnt = new NetworkEntity();
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                break;
            case EntityType.Structure:
                newEnt = new NetworkEntity();
                newEnt.entity_type = type;
                MapController.entities.Add(newEnt);
                break;
            case EntityType.Machine:
                newEnt = new NetworkMachine();
                newEnt.entity_type = type;
                MachineController.entities.Add(newEnt);
                break;
            case EntityType.Mob:
                newEnt = new NetworkMob();
                newEnt.entity_type = type;
                MobController.entities.Add(newEnt);
                break;
        }
        // Ready for spawn!
        newEnt.id = next_entity_id++;
        newEnt.Init();
        newEnt.map_id_string = mapID;
        return newEnt;
    }

    [Export]
    protected string map_id_string;

    public string GetMapID
    {
        get {return map_id_string;}
    }
    
    [Export]
    public long id = 0;
    [Export]
    private static long next_entity_id = 0;
    [Export]
    public Vector3 velocity = Vector3.Zero;
    [Export]
    public bool hidden;

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
        // Containers don't update velocity, and don't move...
        if(hidden)
        {
            velocity = Vector3.Zero;
            Position = new Vector3(0,0,-1000);
        }
        else
        {
            // Update position if it has velocity
            if(velocity.Length() < 0.01) velocity *= 0;
            Position += velocity;
        }
    }

    public void Kill()
    {
        switch(entity_type)
        {
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
    }

    public NetworkTurf GetTurf()
    {
        return MapController.GetTurfAtPosition(map_id_string,Position);
    }
}
