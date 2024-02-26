using Godot;
using System;
using System.ComponentModel;

[GlobalClass] 
public partial class NetworkEntity : Node3D
{
    public enum EntityType
    {
        Item,
        Structure,
        Machine,
        Mob
    }

    private EntityType entity_type;
    public static NetworkEntity CreateEntity(EntityType type)
    {
        NetworkEntity newEnt = null;
        switch(type)
        {
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
        return newEnt;
    }

    [Export]
    public long id = 0;
    [Export]
    private static long next_entity_id = 0;
    [Export]
    public Vector3 velocity = Vector3.Zero;
    private NetworkEntity container = null;
    [Export]
    public NetworkEntity InContainer
    {
        set { container = value; }
        get { return container; }
    }

    public virtual void Init()
    {

    }

    public virtual void Tick()
    {
        
    }

    public void Process()
    {
        // Handle the tick!
        Tick();
        // Containers don't update velocity, and don't move...
        if(InContainer != null)
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
}
