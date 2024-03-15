using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

// Mob entities are objects on a map perform regular life updates, have special inventory slots to wear things, and recieve inputs from clients that they decide how to interpret.
public partial class AbstractMob : AbstractEntity
{ // Returns subtypes of behavior object
    public static AbstractMob CreateMob(PackData data)
    {
        AbstractMob new_mob = null;
        switch(data.behaviorID)
        {
            /*****************************************************************
             * MOB BEHAVIORS (Living creatures and players)
             ****************************************************************/
            case "MOB_SIMPLE": // Life, death, status effects, breathing, eating, reagent processing.
                new_mob = new Behaviors_BASE.AbstractSimpleMob();
            break;

            case "MOB_COMPLEX": // Has species, unique markings, Has organs and limbs, DNA, traits, mutations, on top of all the stuff MOB has.
                new_mob = new Behaviors_BASE.AbstractSimpleMob();
            break;

            /*****************************************************************
             * UNIQUE MOB BEHAVIORS (Hyper specialized mobs that need entirely unique features)
             ****************************************************************/
            case "MOB_BORER":
                new_mob = new Behaviors_BASE.AbstractSimpleMob();
            break;

            /*****************************************************************
             * Debugging purposes only.
             ****************************************************************/
            default:
            case "_BEHAVIOR_":
                new_mob = new AbstractMob();
            break;
        }
        return new_mob;
    }


    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        MobData temp = data as MobData;
        max_health      = temp.max_health;
        health          = temp.max_health;
        walk_speed      = temp.walk_speed;
        run_speed       = temp.run_speed;

        has_hands       = temp.has_hands;
        extra_hands     = temp.extra_hands;
        complex_tools   = temp.complex_tools;

        wears_hats      = temp.wears_hats;
        wears_mask      = temp.wears_mask;
        wears_eyes      = temp.wears_eyes;
        wears_uniform   = temp.wears_uniform;
        wears_suit      = temp.wears_suit;
        wears_shoe      = temp.wears_shoe;
        wears_ears      = temp.wears_ears;
        wears_glove     = temp.wears_glove;
        wears_belt      = temp.wears_belt;
    }
    // Gameplay interaction
    public float max_health = 0;   // Size of item in world and bags
    public float health = 0;    // Size of item in world and bags
    public float walk_speed = (float)0.25;
    public float run_speed = 1;
    public bool has_hands;      // Can pick up objects
    public bool extra_hands;    // Has extra hand slots(basically just pockets without needing uniform)
    public bool complex_tools;  // If mob can use complex tools 
    // Inventory slots
    public bool wears_hats;
    public bool wears_mask;
    public bool wears_eyes;
    public bool wears_uniform; // Controls more than uniform slot, allows pockets, id, back, etc
    public bool wears_suit;
    public bool wears_shoe;
    public bool wears_ears;
    public bool wears_glove;
    public bool wears_belt;
    // End of template data

    /*****************************************************************
     * INVENTORY SLOTS, hands, worn etc.
     ****************************************************************/
    public enum InventorySlot
    {
        Rhand,
        Lhand,
        RhandLower,
        LhandLower,
        Head,
        Mask,
        Eyes,
        Uniform,
        Suit,
        Shoes,
        Lear,
        Rear,
        Gloves,
        Back,
        ID,
        Belt,
        Bag,
        Lpocket,
        Rpocket
    }
    private int active_hand = 0; // L or R
    private AbstractEntity[] inventory_slots;
    public override AbstractEntity ActiveHand    { get {return inventory_slots[active_hand];} set {inventory_slots[active_hand] = value;}}
    public AbstractEntity R_hand        { get {return inventory_slots[(int)InventorySlot.Rhand];}   set {inventory_slots[(int)InventorySlot.Rhand] = value;}}
    public AbstractEntity L_hand        { get {return inventory_slots[(int)InventorySlot.Lhand];}   set {inventory_slots[(int)InventorySlot.Lhand] = value;}}
    public AbstractEntity R_handlower   { get {return inventory_slots[(int)InventorySlot.Rhand];}   set {inventory_slots[(int)InventorySlot.Rhand] = value;}}
    public AbstractEntity L_handlower   { get {return inventory_slots[(int)InventorySlot.LhandLower];}   set {inventory_slots[(int)InventorySlot.LhandLower] = value;}}
    public AbstractEntity SlotHead      { get {return inventory_slots[(int)InventorySlot.Head];}    set {inventory_slots[(int)InventorySlot.Head] = value;}}
    public AbstractEntity SlotMask      { get {return inventory_slots[(int)InventorySlot.Mask];}    set {inventory_slots[(int)InventorySlot.Mask] = value;}}
    public AbstractEntity SlotEyes      { get {return inventory_slots[(int)InventorySlot.Eyes];}    set {inventory_slots[(int)InventorySlot.Eyes] = value;}}
    public AbstractEntity SlotUniform   { get {return inventory_slots[(int)InventorySlot.Uniform];} set {inventory_slots[(int)InventorySlot.Uniform] = value;}}
    public AbstractEntity SlotSuit      { get {return inventory_slots[(int)InventorySlot.Suit];}    set {inventory_slots[(int)InventorySlot.Suit] = value;}}
    public AbstractEntity SlotShoes     { get {return inventory_slots[(int)InventorySlot.Shoes];}   set {inventory_slots[(int)InventorySlot.Shoes] = value;}}
    public AbstractEntity SlotLEar      { get {return inventory_slots[(int)InventorySlot.Lear];}    set {inventory_slots[(int)InventorySlot.Lear] = value;}}
    public AbstractEntity SlotREar      { get {return inventory_slots[(int)InventorySlot.Rear];}    set {inventory_slots[(int)InventorySlot.Rear] = value;}}
    public AbstractEntity SlotGloves    { get {return inventory_slots[(int)InventorySlot.Gloves];}  set {inventory_slots[(int)InventorySlot.Gloves] = value;}}
    public AbstractEntity SlotBack      { get {return inventory_slots[(int)InventorySlot.Back];}    set {inventory_slots[(int)InventorySlot.Back] = value;}}
    public AbstractEntity SlotID        { get {return inventory_slots[(int)InventorySlot.ID];}      set {inventory_slots[(int)InventorySlot.ID] = value;}}
    public AbstractEntity SlotBelt      { get {return inventory_slots[(int)InventorySlot.Belt];}    set {inventory_slots[(int)InventorySlot.Belt] = value;}}
    public AbstractEntity SlotBag       { get {return inventory_slots[(int)InventorySlot.Bag];}     set {inventory_slots[(int)InventorySlot.Bag] = value;}}
    public AbstractEntity SlotLPocket   { get {return inventory_slots[(int)InventorySlot.Lpocket];} set {inventory_slots[(int)InventorySlot.Lpocket] = value;}}
    public AbstractEntity SlotRPocket   { get {return inventory_slots[(int)InventorySlot.Rpocket];} set {inventory_slots[(int)InventorySlot.Rpocket] = value;}}

    public AbstractMob()
    {
        inventory_slots = new AbstractEntity[ Enum.GetNames(typeof(InventorySlot)).Length ];
    }

    /*****************************************************************
     * INVENTORY MANAGEMENT
     ****************************************************************/
    public void SwapHands()
    {
        active_hand = ++active_hand % 2;
    }
    public void Pickup(AbstractEntity collect)
    {
        collect?.PickedUp(this,this);
    }
    public void DropActiveHand()
    {
        if(ActiveHand != null) return;
        ActiveHand?.Drop(GetTurf(),this);
    }
    public void DropSlot(InventorySlot slot)
    {
        if(inventory_slots[(int)slot] != null) return;
        inventory_slots[(int)slot]?.Drop(GetTurf(),this);
    }
    public bool SlotInUse(InventorySlot slot)
    {
        return inventory_slots[(int)slot] != null;
    }
    public AbstractEntity GetSlotEntity(InventorySlot slot)
    {
        return inventory_slots[(int)slot];
    }

    public void UseActiveHand(AbstractEntity target)
    {
        ActiveHand?.AttackSelf(this);
    }

    public void EquipActiveHand(AbstractEntity target)
    {
        if(ActiveHand != null) return;
        // TODO =================================================================================================================================
    }

    /*****************************************************************
     * Input and AI control
     ****************************************************************/
    public override void ControlUpdate(Godot.Collections.Dictionary client_input_data)
    {
        DAT.Dir old_dir = direction;
        if(client_input_data.Keys.Count == 0) return;



        if(old_dir != direction) UpdateNetwork(false);
    }
}
