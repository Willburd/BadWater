using Godot;
using System;
using System.Collections.Generic;

// Machine entities are objects on a map that perform a regular update, are not living things, and often interact directly with the map. Rarely some objects that are not machines may use this type.
public partial class AbstractStructure : AbstractEntity
{
    public AbstractStructure()
    {
        entity_type = MainController.DataType.Structure;
    }

    public static AbstractStructure CreateStructure(PackData data, string data_string = "")
    {
        AbstractStructure new_structure = null;
        switch(data.behaviorID)
        {
            

            /*****************************************************************
             * Debugging purposes only.
             ****************************************************************/
            default:
            case "_BEHAVIOR_":
                new_structure = new AbstractStructure();
            break;
        }
        return new_structure;
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
