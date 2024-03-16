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
        base.SetVars(data_override);
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        size_category           = (SizeCategory)TOOLS.ApplyExistingTag(data,"size_category",(int)size_category);
        force                   = TOOLS.ApplyExistingTag(data,"force",(int)force);
        NOBLUDGEON              = TOOLS.ApplyExistingTag(data,"no_bludgeion",NOBLUDGEON);
        NOCONDUCT               = TOOLS.ApplyExistingTag(data,"no_conduct",NOCONDUCT);
        ON_BORDER               = TOOLS.ApplyExistingTag(data,"on_border",ON_BORDER);
        NOBLOODY                = TOOLS.ApplyExistingTag(data,"no_bloody",NOBLOODY);
        CHEMCONTAINER           = TOOLS.ApplyExistingTag(data,"chem_container",CHEMCONTAINER);
        PHORONGUARD	            = TOOLS.ApplyExistingTag(data,"phoron_guard",PHORONGUARD);
        NOREACT	                = TOOLS.ApplyExistingTag(data,"no_react",NOREACT);
        THICKMATERIAL           = TOOLS.ApplyExistingTag(data,"thick_material",THICKMATERIAL);
        AIRTIGHT                = TOOLS.ApplyExistingTag(data,"air_tight",AIRTIGHT);
        NOSLIP                  = TOOLS.ApplyExistingTag(data,"no_slip",NOSLIP);
        BLOCK_GAS_SMOKE_EFFECT  = TOOLS.ApplyExistingTag(data,"block_smoke",BLOCK_GAS_SMOKE_EFFECT);
        FLEXIBLEMATERIAL        = TOOLS.ApplyExistingTag(data,"flexible_material",FLEXIBLEMATERIAL);
        ALLOW_SURVIVALFOOD      = TOOLS.ApplyExistingTag(data,"allow_survival_food",ALLOW_SURVIVALFOOD);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " tag: " + tag + " size: "  + size_category;
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
    
    public override void Clone(PackData source)
    {
        ItemData temp = source as ItemData;
        base.Clone(temp);
        size_category           = temp.size_category;
        force                   = temp.force;
        NOBLUDGEON              = temp.NOBLUDGEON;
        NOCONDUCT               = temp.NOCONDUCT;
        ON_BORDER               = temp.ON_BORDER;
        NOBLOODY                = temp.NOBLOODY; 
        CHEMCONTAINER           = temp.CHEMCONTAINER;
        PHORONGUARD	            = temp.PHORONGUARD;
        NOREACT	                = temp.NOREACT;
        THICKMATERIAL           = temp.THICKMATERIAL;
        AIRTIGHT                = temp.AIRTIGHT;
        NOSLIP                  = temp.NOSLIP;
        BLOCK_GAS_SMOKE_EFFECT  = temp.BLOCK_GAS_SMOKE_EFFECT;
        FLEXIBLEMATERIAL        = temp.FLEXIBLEMATERIAL;
        ALLOW_SURVIVALFOOD      = temp.ALLOW_SURVIVALFOOD;
    }

    // Unique data
    public SizeCategory size_category;
    public float force;
    // flags
    public bool NOBLUDGEON              = false;
    public bool NOCONDUCT               = false;
    public bool ON_BORDER               = false;
    public bool NOBLOODY                = false; 
    public bool CHEMCONTAINER           = false;
    public bool PHORONGUARD	            = false;
    public bool NOREACT	                = false;
    public bool THICKMATERIAL           = false;
    public bool AIRTIGHT                = false;
    public bool NOSLIP                  = false;
    public bool BLOCK_GAS_SMOKE_EFFECT  = false;
    public bool FLEXIBLEMATERIAL        = false;
    public bool ALLOW_SURVIVALFOOD      = false;
}
