using Godot;
using System;

[GlobalClass] 
public partial class ItemData : PackData
{
    public enum SizeCategory
    {
        Tiny,
        Small,
        Medium,
        Large,
        Huge,
        Giant,
        Massive,
        Gigantic
    }

    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        display_name    = TOOLS.ApplyExistingTag(data,"name",display_name);
        description    = TOOLS.ApplyExistingTag(data,"desc",description);
        size_category   = (SizeCategory)TOOLS.ApplyExistingTag(data,"size_category",(int)size_category);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " size: "  + size_category;
    }
    
    public static int InventorySlots(SizeCategory size)
    {
        switch(size)
        {
            case SizeCategory.Tiny:
                return 1;
            case SizeCategory.Small:
                return 1;
            case SizeCategory.Medium:
                return 2;
            case SizeCategory.Large:
                return 3;
            case SizeCategory.Huge:
                return 4;
            case SizeCategory.Giant:
                return 6;
            case SizeCategory.Massive:
                return 8;
            case SizeCategory.Gigantic:
                return 9;
        }
        return 1;
    }

    // Unique data
    public SizeCategory size_category;
}
