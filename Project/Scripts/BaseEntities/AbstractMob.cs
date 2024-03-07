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

    /*****************************************************************
     * INVENTORY SLOTS, hands, worn etc.
     ****************************************************************/
    public enum InventorySlot
    {
        Rhand,
        Lhand,
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
    private AbstractEntity[] inventory_slots = new AbstractEntity[1];
    public AbstractEntity ActiveHand    { get {return inventory_slots[active_hand];} set {inventory_slots[active_hand] = value;}}
    public AbstractEntity R_hand        { get {return inventory_slots[(int)InventorySlot.Rhand];}   set {inventory_slots[(int)InventorySlot.Rhand] = value;}}
    public AbstractEntity L_hand        { get {return inventory_slots[(int)InventorySlot.Lhand];}   set {inventory_slots[(int)InventorySlot.Lhand] = value;}}
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
        ActiveHand?.Drop(GetTurf(),this);
    }
    public void DropSlot(InventorySlot slot)
    {
        inventory_slots[(int)slot]?.Drop(GetTurf(),this);
    }

    /*****************************************************************
     * Input and AI control
     ****************************************************************/
    public new void ControlUpdate(Godot.Collections.Dictionary client_input_data)
    {
        if(client_input_data.Keys.Count == 0) return;
        behavior_type?.HandleInput(this,entity_type,client_input_data);
    }
}
