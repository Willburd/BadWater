using Godot;
using System;

[GlobalClass] 
public partial class ItemData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        base.SetVars(data_override);
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        size_category           = StringToSizeCategory( JsonHandler.ApplyExistingTag(data,"size_category",size_category.ToString()));
        ISSHARP                 = JsonHandler.ApplyExistingTag(data,"is_sharp",ISSHARP);
        HASEDGE                 = JsonHandler.ApplyExistingTag(data,"has_edge",HASEDGE);
        NOBLUDGEON              = JsonHandler.ApplyExistingTag(data,"no_bludgeon",NOBLUDGEON);
        NOCONDUCT               = JsonHandler.ApplyExistingTag(data,"no_conduct",NOCONDUCT);
        ON_BORDER               = JsonHandler.ApplyExistingTag(data,"on_border",ON_BORDER);
        NOBLOODY                = JsonHandler.ApplyExistingTag(data,"no_bloody",NOBLOODY);
        CHEMCONTAINER           = JsonHandler.ApplyExistingTag(data,"chem_container",CHEMCONTAINER);
        PHORONGUARD	            = JsonHandler.ApplyExistingTag(data,"phoron_guard",PHORONGUARD);
        NOREACT	                = JsonHandler.ApplyExistingTag(data,"no_react",NOREACT);
        THICKMATERIAL           = JsonHandler.ApplyExistingTag(data,"thick_material",THICKMATERIAL);
        AIRTIGHT                = JsonHandler.ApplyExistingTag(data,"air_tight",AIRTIGHT);
        NOSLIP                  = JsonHandler.ApplyExistingTag(data,"no_slip",NOSLIP);
        BLOCK_GAS_SMOKE_EFFECT  = JsonHandler.ApplyExistingTag(data,"block_smoke",BLOCK_GAS_SMOKE_EFFECT);
        FLEXIBLEMATERIAL        = JsonHandler.ApplyExistingTag(data,"flexible_material",FLEXIBLEMATERIAL);
        ALLOW_SURVIVALFOOD      = JsonHandler.ApplyExistingTag(data,"allow_survival_food",ALLOW_SURVIVALFOOD);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " tag: " + tag + " size: "  + size_category;
    }
    
    private static DAT.SizeCategory StringToSizeCategory(string parse)
    {
        if(Enum.TryParse(parse, out DAT.SizeCategory ret)) return ret;
        return DAT.SizeCategory.MEDIUM;
    }

    public static int InventorySlots(DAT.SizeCategory size)
    {
        switch(size)
        {
            case DAT.SizeCategory.TINY:
                return 1;
            case DAT.SizeCategory.SMALL:
                return 1;
            case DAT.SizeCategory.MEDIUM:
                return 2;
            case DAT.SizeCategory.LARGE:
                return 4;
            case DAT.SizeCategory.HUGE:
                return 8;
            case DAT.SizeCategory.ITEMSIZE_NO_CONTAINER:
                return 0;
        }
        return 1;
    }
    
    public override void Clone(PackData source)
    {
        ItemData temp = source as ItemData;
        base.Clone(temp);
        size_category           = temp.size_category;
        ISSHARP                 = temp.ISSHARP;
        HASEDGE                 = temp.HASEDGE;
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
    public DAT.SizeCategory size_category = DAT.SizeCategory.MEDIUM;
    // flags
    public bool ISSHARP                 = false;
    public bool HASEDGE                 = false;
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
