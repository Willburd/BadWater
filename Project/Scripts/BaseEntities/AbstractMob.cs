using Godot;
using System;
using System.Collections.Generic;

// Mob entities are objects on a map perform regular life updates, have special inventory slots to wear things, and recieve inputs from clients that they decide how to interpret.
public partial class AbstractMob : AbstractEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        MobData temp = data as MobData;
        max_health = temp.max_health;
        health = temp.max_health;
    }
    public float max_health = 0;   // Size of item in world and bags
    public float health = 0;   // Size of item in world and bags
    // End of template data

    public new void ControlUpdate(Godot.Collections.Dictionary client_input_data)
    {
        if(client_input_data.Keys.Count == 0) return;
        behavior_type?.HandleInput(this,entity_type,client_input_data);
    }
}
