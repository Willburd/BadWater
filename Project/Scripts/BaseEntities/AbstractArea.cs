using Godot;
using System;
using System.Collections.Generic;

public class AbstractArea : AbstractEntity
{
    public AbstractArea()
    {
        entity_type = MainController.DataType.Area;
    }

    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        AreaData temp = data as AreaData;
        base_turf_ID = temp.base_turf_ID;
        is_space = temp.is_space;
        always_powered = temp.always_powered;
    }
    // Unique data
    [Export]
    public string base_turf_ID;
    [Export]
    public bool always_powered;
    [Export]
    public bool is_space;
    // End of template data
    public void AddTurf(AbstractTurf turf)
    {
        // Remove from other areas
        turf.Area = this;
    }
}