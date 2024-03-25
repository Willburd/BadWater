using Godot;
using System;
using System.Collections.Generic;

// Machine entities are objects on a map that perform a regular update, are not living things, and often interact directly with the map. Rarely some objects that are not machines may use this type.
public partial class AbstractMachine : AbstractEntity
{
    public static AbstractMachine CreateMachine(PackData data, string data_string = "")
    {
        AbstractMachine new_machine = null;
        switch(data.behaviorID)
        {
            

            /*****************************************************************
             * Debugging purposes only.
             ****************************************************************/
            default:
            case "_BEHAVIOR_":
                new_machine = new AbstractMachine();
            break;
        }
        return new_machine;
    }

    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        //MachineData temp = data as MachineData;
        //model = temp.model;
        //texture = temp.texture;
        //density = template_data.density;
        //opaque = template_data.opaque;
    }
    // End of template data

}
