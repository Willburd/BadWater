using Godot;
using System;
using System.Collections.Generic;

// Item entities are entities that can be picked up by players upon click, and have behaviors when used in hands.

public partial class AbstractItem : AbstractEntity
{
    public static AbstractItem CreateItem(PackData data)
    {
        AbstractItem new_item = null;
        switch(data.behaviorID)
        {
            

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
        size_category   = temp.size_category;
        force           = temp.force;
        tool_tag        = temp.tool_tag;
        // set flags
        flags.NOBLUDGEON              = temp.NOBLUDGEON;
        flags.NOCONDUCT               = temp.NOCONDUCT;
        flags.ON_BORDER               = temp.ON_BORDER;
        flags.NOBLOODY                = temp.NOBLOODY; 
        flags.CHEMCONTAINER           = temp.CHEMCONTAINER;
        flags.PHORONGUARD	          = temp.PHORONGUARD;
        flags.NOREACT	              = temp.NOREACT;
        flags.THICKMATERIAL           = temp.THICKMATERIAL;
        flags.AIRTIGHT                = temp.AIRTIGHT;
        flags.NOSLIP                  = temp.NOSLIP;
        flags.BLOCK_GAS_SMOKE_EFFECT  = temp.BLOCK_GAS_SMOKE_EFFECT;
        flags.FLEXIBLEMATERIAL        = temp.FLEXIBLEMATERIAL;
        flags.ALLOW_SURVIVALFOOD      = temp.ALLOW_SURVIVALFOOD;
    }
    public DAT.SizeCategory size_category = DAT.SizeCategory.SMALL;   // Size of item in world and bags
    public float force = 0f; // Weapon impact force
    public DAT.ToolTag tool_tag = DAT.ToolTag.NONE;
    public Flags flags;
    public struct Flags
    {
        public Flags() {}
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
