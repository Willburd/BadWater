using Behaviors_BASE;
using Godot;
using System;
using System.Collections.Generic;

// Item entities are entities that can be picked up by players upon click, and have behaviors when used in hands.

public partial class AbstractItem : AbstractEntity
{
    public AbstractItem()
    {
        entity_type = MainController.DataType.Item;
    }

    public static AbstractItem CreateItem(PackData data, string data_string = "")
    {
        AbstractItem new_item = null;
        switch(data.behaviorID)
        {
            case "ITEM":
                new_item = new AbstractItem();
            break;

            // Toolset
            case "CROWBAR":     new_item = new AbstractHandTool(DAT.ToolTag.CROWBAR); break;
            case "MULTITOOL":   new_item = new AbstractHandTool(DAT.ToolTag.MULTITOOL); break;
            case "SCREWDRIVER": new_item = new AbstractHandTool(DAT.ToolTag.SCREWDRIVER); break;
            case "WIRECUTTER":  new_item = new AbstractHandTool(DAT.ToolTag.WIRECUTTER); break;
            case "WRENCH":      new_item = new AbstractHandTool(DAT.ToolTag.WRENCH); break;
            case "WELDER":      new_item = new AbstractHandTool(DAT.ToolTag.WELDER); break;
            case "CABLE_COIL":  new_item = new AbstractHandTool(DAT.ToolTag.CABLE_COIL); break;
            case "ANALYZER":    new_item = new AbstractHandTool(DAT.ToolTag.ANALYZER); break;
            case "MINING":      new_item = new AbstractHandTool(DAT.ToolTag.MINING); break;
            case "SHOVEL":      new_item = new AbstractHandTool(DAT.ToolTag.SHOVEL); break;
            case "RETRACTOR":   new_item = new AbstractHandTool(DAT.ToolTag.RETRACTOR); break;
            case "HEMOSTAT":    new_item = new AbstractHandTool(DAT.ToolTag.HEMOSTAT); break;
            case "CAUTERY":     new_item = new AbstractHandTool(DAT.ToolTag.CAUTERY); break;
            case "DRILL":       new_item = new AbstractHandTool(DAT.ToolTag.DRILL); break;
            case "SCALPEL":     new_item = new AbstractHandTool(DAT.ToolTag.SCALPEL); break;
            case "SAW":         new_item = new AbstractHandTool(DAT.ToolTag.SAW); break;
            case "BONESET":     new_item = new AbstractHandTool(DAT.ToolTag.BONESET); break;
            case "KNIFE":       new_item = new AbstractHandTool(DAT.ToolTag.KNIFE); break;
            case "BLOODFILTER": new_item = new AbstractHandTool(DAT.ToolTag.BLOODFILTER); break;
            case "ROLLINGPIN":  new_item = new AbstractHandTool(DAT.ToolTag.ROLLINGPIN); break;

            /*****************************************************************
             * Debugging purposes only.
             ****************************************************************/
            default:
            case "_BEHAVIOR_":
                new_item = new AbstractItem();
            break;
        }
        return new_item;
    }

    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        ItemData temp   = data as ItemData;
        internal_size_category        = temp.size_category;
        // set flags
        flags.ISSHARP                 = temp.ISSHARP;
        flags.HASEDGE                 = temp.HASEDGE;
        flags.NOBLUDGEON              = temp.NOBLUDGEON;
        flags.NOCONDUCT               = temp.NOCONDUCT;
        flags.ON_BORDER               = temp.ON_BORDER;
        flags.NOBLOODY                = temp.NOBLOODY; 
        flags.CHEMCONTAINER           = temp.CHEMCONTAINER;
        flags.PHORONGUARD	          = temp.PHORONGUARD;
        flags.NOREACT	              = temp.NOREACT;
        // Rest of these probably moved to clothing when that is added! TODO =====================================================================
        flags.THICKMATERIAL           = temp.THICKMATERIAL;
        flags.AIRTIGHT                = temp.AIRTIGHT;
        flags.NOSLIP                  = temp.NOSLIP;
        flags.BLOCK_GAS_SMOKE_EFFECT  = temp.BLOCK_GAS_SMOKE_EFFECT;
        flags.FLEXIBLEMATERIAL        = temp.FLEXIBLEMATERIAL;
        flags.ALLOW_SURVIVALFOOD      = temp.ALLOW_SURVIVALFOOD;
    }
    public DAT.SizeCategory internal_size_category = DAT.SizeCategory.MEDIUM;   // Size of item in world and bags
    public override DAT.SizeCategory SizeCategory
    {
        get { return internal_size_category; }
    }
    public Flags flags;
    public struct Flags
    {
        public Flags() {}
        public bool ISSHARP                 = false; // Can item puncture things like a needle
        public bool HASEDGE                 = false; // Does item have a cutting edge to interact with
        public bool NOBLUDGEON              = false; // When an item has this it produces no "X has been hit by Y with Z" message with the default handler.
        public bool NOCONDUCT               = false; // Conducts electricity. (metal etc.)
        public bool ON_BORDER               = false; // Item has priority to check when entering or leaving.
        public bool NOBLOODY                = false; // Used for items if they don't want to get a blood overlay.
        public bool CHEMCONTAINER           = false; // Is an open container for chemistry purposes.
        public bool PHORONGUARD	            = false; // Does not get contaminated by phoron.
        public bool NOREACT	                = false; // Reagents don't react inside this container.
        public bool THICKMATERIAL           = false; // Prevents syringes, parapens and hyposprays if equipped to slot_suit or slot_head.
        public bool AIRTIGHT                = false; // Functions with internals.
        public bool NOSLIP                  = false; // Prevents from slipping on wet floors, in space, etc.
        public bool BLOCK_GAS_SMOKE_EFFECT  = false; // Blocks the effect that chemical clouds would have on a mob -- glasses, mask and helmets ONLY! (NOTE: flag shared with ONESIZEFITSALL)
        public bool FLEXIBLEMATERIAL        = false; // At the moment, masks with this flag will not prevent eating even if they are covering your face.
        public bool ALLOW_SURVIVALFOOD      = false; // Allows special survival food items to be eaten through it
    };
    // End of template data
}
