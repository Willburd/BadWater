using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

// Mob entities are objects on a map perform regular life updates, have special inventory slots to wear things, and recieve inputs from clients that they decide how to interpret.
public partial class AbstractMob : AbstractEntity
{ // Returns subtypes of behavior object
    public AbstractMob()
    {
        inventory_slots = new AbstractEntity[ Enum.GetNames(typeof(DAT.InventorySlot)).Length ];
    }
    private MobAI mob_ai;


    public static AbstractMob CreateMob(PackData data)
    {
        AbstractMob new_mob = null;
        switch(data.behaviorID)
        {
            /*****************************************************************
             * MOB BEHAVIORS (Living creatures and players)
             ****************************************************************/
            case "MOB_SIMPLE": // Life, death, status effects, breathing, eating, reagent processing.
                new_mob = new AbstractMob();
            break;

            case "MOB_COMPLEX": // Has species, unique markings, Has organs and limbs, DNA, traits, mutations, on top of all the stuff MOB has.
                new_mob = new AbstractMob();
            break;

            /*****************************************************************
             * UNIQUE MOB BEHAVIORS (Hyper specialized mobs that need entirely unique features)
             ****************************************************************/
            case "MOB_BORER":
                new_mob = new AbstractMob();
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
        // flags
        flags.HASHANDS      = temp.has_hands;
        flags.EXTRAHANDS    = temp.extra_hands;
        flags.COMPLEXTOOLS  = temp.complex_tools;
        // Inventory flags
        flags.WEARHAT       = temp.wears_hats;
        flags.WEARMASK      = temp.wears_mask;
        flags.WEAREYES      = temp.wears_eyes;
        flags.WEARUNIFORM   = temp.wears_uniform;
        flags.WEARSUIT      = temp.wears_suit;
        flags.WEARSHOES     = temp.wears_shoe;
        flags.WEAREARS      = temp.wears_ears;
        flags.WEARGLOVES    = temp.wears_glove;
        flags.WEARBELT      = temp.wears_belt;
    }
    public float max_health = 0;   // Size of item in world and bags
    public float health = 0;    // Size of item in world and bags
    public float walk_speed = (float)0.25;
    public float run_speed = 1;
    public Flags flags;
    public struct Flags
    {
        public Flags() {}
        // flags
        public bool HASHANDS = false;      // Can pick up objects
        public bool EXTRAHANDS = false;    // Has extra hand slots(basically just pockets without needing uniform)
        public bool COMPLEXTOOLS = false;  // If mob can use complex tools 
        // Inventory slot flags
        public bool WEARHAT = false;
        public bool WEARMASK = false;
        public bool WEAREYES = false;
        public bool WEARUNIFORM = false; // Controls more than uniform slot, allows pockets, id, back, etc
        public bool WEARSUIT = false;
        public bool WEARSHOES = false;
        public bool WEAREARS = false;
        public bool WEARGLOVES = false;
        public bool WEARBELT = false;
    }
    // End of template data

    // state data
    protected DAT.LifeState stat = DAT.LifeState.Alive;
    public DAT.LifeState Stat
    {
        get {return stat;}
    }
    private DAT.ZoneSelection internal_selecting_zone = DAT.ZoneSelection.UpperBody;
    public override DAT.ZoneSelection SelectingZone
    {
        get {return internal_selecting_zone;}
    }
    private float footstep_timer = 0f;
    public AbstractEntity lastattacked = null;
    public AbstractEntity lastattacker = null;
    private int active_hand = 0; // L or R
    // end state data

    /*****************************************************************
     * INVENTORY SLOTS, hands, worn etc.
     ****************************************************************/
    private AbstractEntity[] inventory_slots;
    public override AbstractEntity ActiveHand    { get {return inventory_slots[active_hand];} set {inventory_slots[active_hand] = value;}}
    public AbstractEntity R_hand        { get {return inventory_slots[(int)DAT.InventorySlot.Rhand];}   set {inventory_slots[(int)DAT.InventorySlot.Rhand] = value;}}
    public AbstractEntity L_hand        { get {return inventory_slots[(int)DAT.InventorySlot.Lhand];}   set {inventory_slots[(int)DAT.InventorySlot.Lhand] = value;}}
    public AbstractEntity R_handlower   { get {return inventory_slots[(int)DAT.InventorySlot.Rhand];}   set {inventory_slots[(int)DAT.InventorySlot.Rhand] = value;}}
    public AbstractEntity L_handlower   { get {return inventory_slots[(int)DAT.InventorySlot.LhandLower];}   set {inventory_slots[(int)DAT.InventorySlot.LhandLower] = value;}}
    public AbstractEntity SlotHead      { get {return inventory_slots[(int)DAT.InventorySlot.Head];}    set {inventory_slots[(int)DAT.InventorySlot.Head] = value;}}
    public AbstractEntity SlotMask      { get {return inventory_slots[(int)DAT.InventorySlot.Mask];}    set {inventory_slots[(int)DAT.InventorySlot.Mask] = value;}}
    public AbstractEntity SlotEyes      { get {return inventory_slots[(int)DAT.InventorySlot.Eyes];}    set {inventory_slots[(int)DAT.InventorySlot.Eyes] = value;}}
    public AbstractEntity SlotUniform   { get {return inventory_slots[(int)DAT.InventorySlot.Uniform];} set {inventory_slots[(int)DAT.InventorySlot.Uniform] = value;}}
    public AbstractEntity SlotSuit      { get {return inventory_slots[(int)DAT.InventorySlot.Suit];}    set {inventory_slots[(int)DAT.InventorySlot.Suit] = value;}}
    public AbstractEntity SlotShoes     { get {return inventory_slots[(int)DAT.InventorySlot.Shoes];}   set {inventory_slots[(int)DAT.InventorySlot.Shoes] = value;}}
    public AbstractEntity SlotLEar      { get {return inventory_slots[(int)DAT.InventorySlot.Lear];}    set {inventory_slots[(int)DAT.InventorySlot.Lear] = value;}}
    public AbstractEntity SlotREar      { get {return inventory_slots[(int)DAT.InventorySlot.Rear];}    set {inventory_slots[(int)DAT.InventorySlot.Rear] = value;}}
    public AbstractEntity SlotGloves    { get {return inventory_slots[(int)DAT.InventorySlot.Gloves];}  set {inventory_slots[(int)DAT.InventorySlot.Gloves] = value;}}
    public AbstractEntity SlotBack      { get {return inventory_slots[(int)DAT.InventorySlot.Back];}    set {inventory_slots[(int)DAT.InventorySlot.Back] = value;}}
    public AbstractEntity SlotID        { get {return inventory_slots[(int)DAT.InventorySlot.ID];}      set {inventory_slots[(int)DAT.InventorySlot.ID] = value;}}
    public AbstractEntity SlotBelt      { get {return inventory_slots[(int)DAT.InventorySlot.Belt];}    set {inventory_slots[(int)DAT.InventorySlot.Belt] = value;}}
    public AbstractEntity SlotBag       { get {return inventory_slots[(int)DAT.InventorySlot.Bag];}     set {inventory_slots[(int)DAT.InventorySlot.Bag] = value;}}
    public AbstractEntity SlotLPocket   { get {return inventory_slots[(int)DAT.InventorySlot.Lpocket];} set {inventory_slots[(int)DAT.InventorySlot.Lpocket] = value;}}
    public AbstractEntity SlotRPocket   { get {return inventory_slots[(int)DAT.InventorySlot.Rpocket];} set {inventory_slots[(int)DAT.InventorySlot.Rpocket] = value;}}


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
    public void DropSlot(DAT.InventorySlot slot)
    {
        if(inventory_slots[(int)slot] != null) return;
        inventory_slots[(int)slot]?.Drop(GetTurf(),this);
    }
    public bool SlotInUse(DAT.InventorySlot slot)
    {
        return inventory_slots[(int)slot] != null;
    }
    public AbstractEntity GetSlotEntity(DAT.InventorySlot slot)
    {
        return inventory_slots[(int)slot];
    }

    public void UseActiveHand(AbstractEntity target)
    {
        ActiveHand?.AttackedSelf(this);
    }

    public void EquipActiveHand(AbstractEntity target)
    {
        if(ActiveHand != null) return;
        // TODO =================================================================================================================================
    }


    /*****************************************************************
     * Click handling
     ****************************************************************/
    public override void Clicked( AbstractEntity used_item, AbstractEntity target, Godot.Collections.Dictionary click_params) 
    {
        // Don't react if click cooldown
        if(CheckClickCooldown()) return;
        SetClickCooldown(1);

        // Handle special clicks
        if(click_params["mod_shift"].AsBool())
        {
            // TODO, Print description in chat EXAMINATE! =======================================================================================================
            GD.Print(target.description);
            return;
        }

        // Check status effects
        if(stat != DAT.LifeState.Alive)
        {
            return;
        }
        // Handle special clicks that require you to be concious
        if(click_params["mod_shift"].AsBool() && click_params["button"].AsInt32() == (int)MouseButton.Middle)
        {
            // Point to object TODO ==========================================================================================================================
            return;
        }
        if(click_params["mod_control"].AsBool())
        {
            // Pulling objects TODO ==========================================================================================================================
            return;
        }

        // Turn to face it
        direction = TOOLS.RotateTowardEntity(this,target);
        // Mecha held control
        // Check restrained
        if(IsRestrained())
        {
            RestrainedClick(target);
            return;
        }
        // Check throwmode
        // Vehicle helm control
        
        // Handle using items on themselves
        AbstractEntity hand_item = ActiveHand;
        if(hand_item == target) 
        {
            hand_item.AttackedSelf(this);
        }
        // Interacting with entities directly in your inventory
        int storage_depth = target.StorageDepth(this);
        if((target is not AbstractTurf && target == GetLocation()) || (storage_depth != -1 && storage_depth <= 1))
        {
            if(hand_item != null)
            {
                bool resolved = hand_item.Attack( this, target, 1, click_params);
                if(!resolved && target != null && hand_item != null) hand_item.AfterAttack( this, target, true, click_params);
            }
            else
            {
                if(target is AbstractMob) SetClickCooldown( GetAttackCooldown(hand_item)); // No instant mob attacking
                UnarmedAttack(target, true);
            }
            return;
        }

        if(GetLocation() is not AbstractTurf) return; // This is going to stop you from telekinesing from inside a closet, but I don't shed many tears for that
        if(IsIntangible()) return; // shadekin can't interact with anything else! They already can't use their bag
            
        //Atoms on turfs (not on your person)
        // A is a turf or is on a turf, or in something on a turf (pen in a box); but not something in something on a turf (pen in a box in a backpack)
        storage_depth = target.StorageDepth(this);
        if(target is AbstractTurf || target.GetLocation() is AbstractTurf  || (storage_depth != -1 && storage_depth <= 1))
        {
            if(TOOLS.Adjacent(this,target) || (hand_item != null && hand_item.AttackCanReach(this, target, hand_item.attack_range)) )
            {
                if(hand_item != null)
                {
                    // Return 1 in AttackedBy() to prevent AfterAttack() effects (when safely moving items for example)
                    if(!hand_item.Attack( target, this, 1f, click_params) && target != null && hand_item != null) hand_item.AfterAttack(target, this, true, click_params);
                }
                else
                {
                    if(target is AbstractMob) SetClickCooldown( GetAttackCooldown(null)); // No instant mob attacking
                    UnarmedAttack(target, true);
                }
                return;
            }
            else // non-adjacent click
            {
                if(hand_item != null)
                {
                    hand_item.AfterAttack(target, this, false, click_params);
                }
                else
                {
                    RangedAttack(target, click_params);
                }
            }
        }
        return;
    }
    protected virtual void RestrainedClick(AbstractEntity target)
    {
        GD.Print(display_name + " CLICKED " + target.display_name + " WHILE RESTRAINED"); // REPLACE ME!!!
    }


    /*****************************************************************
     * Attack handling
     ****************************************************************/
    protected override bool UnarmedAttack(AbstractEntity target, bool proximity)
    {
        if(IsIntangible()) return false;
        if(stat != DAT.LifeState.Alive) return false;
        GD.Print(display_name + " UNARMED ATTACKED " + target.display_name); // REPLACE ME!!!
        return true;
    }

    protected virtual void RangedAttack(AbstractEntity target, Godot.Collections.Dictionary click_parameters)
    {
        if(HasTelegrip())
        {
            if(TOOLS.VecDist(GridPos.WorldPos(), target.GridPos.WorldPos()) > DAT.TK_MAXRANGE) return;
            target.AttackedByTK(this);
        }
    }


    /*****************************************************************
     * Processing
     ****************************************************************/
    public override void ControlUpdate(Godot.Collections.Dictionary client_input_data)
    {
        // Got an actual control update!
        double dat_x = Mathf.Clamp(client_input_data["x"].AsDouble(),-1,1) * MainController.controller.config.input_factor;
        double dat_y = Mathf.Clamp(client_input_data["y"].AsDouble(),-1,1) * MainController.controller.config.input_factor;
        bool walking = client_input_data["walk"].AsBool();

        if(stat != DAT.LifeState.Dead)
        {
            // Trigger mob actions
            if(client_input_data["resist"].AsBool())
            {

            }
            if(client_input_data["rest"].AsBool())
            {
                
            }
            if(client_input_data["equip"].AsBool())
            {
                EquipActiveHand(null);
            }
            if(client_input_data["useheld"].AsBool())
            {
                UseActiveHand(null);
            }

            // Move based on mob speed
            MapController.GridPos new_pos = GridPos;
            float speed = 0f;
            if(client_input_data["mod_control"].AsBool())
            {
                // Inching along with taps at a fixed rate
                new_pos.hor += (float)dat_x * 0.5f;
                new_pos.ver += (float)dat_y * 0.5f;
            }
            else if(walking)
            {
                // slower safer movement
                new_pos.hor += (float)dat_x * walk_speed;
                new_pos.ver += (float)dat_y * walk_speed;
                if(!client_input_data["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) direction = DAT.InputToCardinalDir((float)dat_x,(float)dat_y);
                speed = walk_speed;
            }
            else
            {
                // zoomies as normal
                new_pos.hor += (float)dat_x * run_speed;
                new_pos.ver += (float)dat_y * run_speed;
                if(!client_input_data["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) direction = DAT.InputToCardinalDir((float)dat_x,(float)dat_y);
                speed = run_speed;
            }
            // math for feet speed
            if(dat_x != 0 || dat_y != 0) footstep_timer += Mathf.Lerp(0.05f,0.08f, Mathf.Clamp(speed,0,1.5f));
            AbstractEntity newloc = Move(map_id_string, new_pos);
            if(footstep_timer > 1)
            {
                footstep_timer = 0;
                if(newloc is AbstractTurf)
                {
                    (newloc as AbstractTurf).PlayStepSound(walking);
                }
            }
        }
        else
        {
            // dead or knocked out...
        }

        // Respond in any state, as they are mostly just input states for actions!
        if(client_input_data["swap"].AsBool())
        {
            SwapHands();
        }
        if(client_input_data["throw"].AsBool())
        {
            
        }
        if(client_input_data["drop"].AsBool())
        {
            DropActiveHand();
        }
    }
    public override void Tick()
    {
        if(stat != DAT.LifeState.Dead)
        {
            LifeUpdate();
            mob_ai?.Alive();
        }
        else
        {
            DeathUpdate();
            mob_ai?.Dead();
        }
        ProcessSlotDrops();
    }

    protected virtual void LifeUpdate()
    {

    }

    protected virtual void DeathUpdate()
    {
        
    }

    // Check our current inventory and status... See if we need to drop objects from our hands or slots that no longer exist (uniforms for example give us pockets!)
    protected virtual void ProcessSlotDrops()
    {   
        // knocked out and dead drops hands!
        if(stat != DAT.LifeState.Alive)
        {   
            DropSlot(DAT.InventorySlot.Rhand);
            DropSlot(DAT.InventorySlot.Lhand);
            DropSlot(DAT.InventorySlot.RhandLower);
            DropSlot(DAT.InventorySlot.LhandLower);
        }
        // Not wearing a uniform drops some slots all at once!
        if(!SlotInUse(DAT.InventorySlot.Uniform)) 
        {
            DropSlot(DAT.InventorySlot.Lpocket);
            DropSlot(DAT.InventorySlot.Rpocket);
            DropSlot(DAT.InventorySlot.Back);
            DropSlot(DAT.InventorySlot.ID);
            DropSlot(DAT.InventorySlot.Belt);
        }
    }


    protected virtual void Bleed()
    {

    }

    public virtual void Die()
    {
        stat = DAT.LifeState.Dead;
    }


    /*****************************************************************
     * Conditions
     ****************************************************************/
    public virtual bool IsRestrained()
    {
        return false;
    }
    public virtual bool IsBlind()
    {
        return false;
    }
    public virtual bool IsDeaf()
    {
        return false;
    }
    protected virtual bool HasTelegrip()
    {
        if(flags.WEARGLOVES && false /*SlotGloves == TK GLOVES HERE */) return true; // TODO =======================================================================================================================
        return false;
    }


    /*****************************************************************
     * Click cooldown
     ****************************************************************/
    protected int click_cooldown = 0;  // Time when mob cooldown has finished
    public void SetClickCooldown(int delay)
    {
        click_cooldown = Math.Max(MainController.WorldTicks + delay,click_cooldown);
    }
    protected bool CheckClickCooldown()
    {
        return click_cooldown > MainController.WorldTicks;
    }
    public int GetAttackCooldown(AbstractEntity item_used)
    {
        if(item_used == null) return DAT.DEFAULT_ATTACK_COOLDOWN;
        return DAT.DEFAULT_ATTACK_COOLDOWN;
    }
}
