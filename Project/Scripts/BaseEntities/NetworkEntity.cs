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
    [Export]
    public string tag = "";
    [Export]
    public string model = "Plane";
    [Export]
    public string texture = "";
    [Export]
    public double anim_speed = 0;
    [Export]
    public bool density = false;                    // blocks movement
    [Export]
    public bool opaque = false;                     // blocks vision
    // End of template data
    public static NetworkEntity CreateEntity(AbstractEntity abs, string map_id, MainController.DataType type)
    {
        NetworkEntity newEnt = null;
        switch(type)
        {
            case MainController.DataType.Chunk:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkChunk.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Effect:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkEffect.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Item:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkItem.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Structure:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkStructure.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Machine:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkMachine.tscn").Instantiate() as NetworkEntity;
                break;
            case MainController.DataType.Mob:
                newEnt = GD.Load<PackedScene>("res://Scenes/NetworkMob.tscn").Instantiate() as NetworkEntity;
                break;
        }
        // NetworkEntity init
        newEnt.map_id_string = map_id;
        if(abs != null) newEnt.Sync(abs);
        // Add to active network entities list
        MainController.controller.entity_container.AddChild(newEnt,true);
        return newEnt;
    }

    /*****************************************************************
     * Network entities just follow their abstract owner around...
     ****************************************************************/
    [Export]
    public string map_id_string;
    [Export]
    public Vector3 velocity = Vector3.Zero;
    public void Sync(AbstractEntity abs)
    {
        // sync data
        model = abs.model;
        texture = abs.texture;
        anim_speed = abs.anim_speed;
        density = abs.density;
        opaque = abs.opaque;
        // sync movement
        velocity = abs.velocity;
        Position = TOOLS.GridToPosWithOffset(abs.grid_pos);
    }
    public void Kill()
    {
        QueueFree();
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
