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
        walk_speed      = temp.walk_speed;
        run_speed       = temp.run_speed;
        // flags
        flags.GODMODE       = false;
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
    public int max_health = 0;   // Size of item in world and bags
    public struct HealthData
    {
        public HealthData() {}
        public int brute = 0;
        public int fire = 0;
        public int tox = 0;
        public int oxy = 0;
        public int clone = 0;
        public int halloss = 0;
    }
    public HealthData health = new HealthData();    // Size of item in world and bags

    public float walk_speed = (float)0.25;
    public float run_speed = 1;
    public Flags flags;
    public struct Flags
    {
        public Flags() {}
        // flags
        public bool GODMODE = false;
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
        ActiveHand?.InteractionSelf(this);
    }

    public void EquipActiveHand(AbstractEntity target)
    {
        if(ActiveHand != null) return;
        // TODO =================================================================================================================================
    }

    public void Examinate(AbstractEntity target)
    {
        if(IsBlind() || stat != DAT.LifeState.Alive)
        {
            ChatController.InspectMessage( this, "Something is there but you can't see it.", ChatController.VisibleMessageFormatting.Notice);
            return;
        }

        //Could be gone by the time they finally pick something
        if(target is null) return;

        direction = TOOLS.RotateTowardEntity(this,target);

        string results = target.Examine(this);
        if(results == null || results.Length <= 0)
        {
            results = "You were unable to examine that. Tell a developer!";
        }
        ChatController.InspectMessage( this, results);
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
            Examinate(target);
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
            hand_item.InteractionSelf(this);
            return;
        }
        // Interacting with entities directly in your inventory
        int storage_depth = target.StorageDepth(this);
        if((target is not AbstractTurf && target == GetLocation()) || (storage_depth != -1 && storage_depth <= 1))
        {
            if(hand_item != null)
            {
                bool resolved = hand_item._Interact( this, target, 1, click_params);
                if(!resolved && target != null && hand_item != null) hand_item.InteractionAfter( this, target, true, click_params);
            }
            else
            {
                if(target is AbstractMob) SetClickCooldown( GetAttackCooldown(hand_item)); // No instant mob attacking
                _UnarmedInteract(target, true);
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
            if(TOOLS.Adjacent(this,target,false) || (hand_item != null && hand_item.InteractCanReach(this, target, hand_item.attack_range)) )
            {
                if(hand_item != null)
                {
                    // Return 1 in AttackedBy() to prevent AfterInteraction() effects (when safely moving items for example)
                    if(!hand_item._Interact( target, this, 1f, click_params) && target != null && hand_item != null) hand_item.InteractionAfter(target, this, true, click_params);
                }
                else
                {
                    if(target is AbstractMob) SetClickCooldown( GetAttackCooldown(null)); // No instant mob attacking
                    _UnarmedInteract(target, true);
                }
                return;
            }
            else // non-adjacent click
            {
                if(hand_item != null)
                {
                    hand_item.InteractionAfter(target, this, false, click_params);
                }
                else
                {
                    RangedInteraction(target, click_params);
                }
            }
        }
        return;
    }

    public void _UnarmedInteract(AbstractEntity target, bool proximity)
    {
        if(IsIntangible()) return;
        if(this is AbstractMob self_mob && self_mob.Stat != DAT.LifeState.Alive) return;

        /* TODO Funny glove magic ==============================================================================================================
        // Special glove functions:
        // If the gloves do anything, have them return 1 to stop
        // normal attack_hand() here.
        var/obj/item/clothing/gloves/G = gloves // not typecast specifically enough in defines
        if(istype(G) && G.Touch(A,1))
            return
        */

        if( flags.HASHANDS && ( target is AbstractStructure || target is AbstractMachine ) && SelectingIntent != DAT.Intent.Hurt)
        {
            target.InteractionTouched(this);
            return;
        }

        switch(SelectingIntent)
        {
            case DAT.Intent.Help:
                if(target is AbstractMob target_mob)
                {
                    ChatController.VisibleMessage( this, this.display_name + " pets the " + target_mob.display_name);
                    // custom_emote(1,"[pick(friendly)] \the [A]!"); // TODO pet the dog =================================================================
                }
                break;

            /*
            case DAT.Intent.Hurt:
                if(can_special_attack(A) && special_attack_target(A))
                {
                    return;
                }
                else if(melee_damage_upper == 0 && isliving(A))
                {
                    custom_emote(1,"[pick(friendly)] \the [A]!")
                }
                else
                {
                    attack_target(A);
                }
                break;
            */
        }
    }

    protected virtual void RestrainedClick(AbstractEntity target)
    {
        GD.Print(display_name + " CLICKED " + target.display_name + " WHILE RESTRAINED"); // REPLACE ME!!!
    }


    /*****************************************************************
     * Attack handling
     ****************************************************************/
    public int BruteLoss
    {
        get { return health.brute;}
        set { 
                if(flags.GODMODE) return;
                health.brute = Math.Min(Math.Max(health.brute + value, 0),max_health*2);
                UpdateHealth();
            }
    } 
    public int FireLoss
    {
        get { return health.fire;}
        set { 
                if(flags.GODMODE) return;
                health.fire = Math.Min(Math.Max(health.fire + value, 0),max_health*2);
                UpdateHealth();
            }
    } 
    public int ToxLoss 
    {
        get { return health.tox;}
        set { 
                if(flags.GODMODE) return;
                health.tox = Math.Min(Math.Max(health.tox + value, 0),max_health*2);
                UpdateHealth();
            }
    } 
    public int OxyLoss
    {
        get { return health.oxy;}
        set { 
                if(flags.GODMODE) return;
                health.oxy = Math.Min(Math.Max(health.oxy + value, 0),max_health*2);
                UpdateHealth();
            }
    } 
    public int CloneLoss
    {
        get { return health.clone;}
        set { 
                if(flags.GODMODE) return;
                health.clone = Math.Min(Math.Max(health.clone + value, 0),max_health*2);
                UpdateHealth();
            }
    } 
    public int HalLoss
    {
        get { return health.halloss;}
        set { 
                if(flags.GODMODE) return;
                health.halloss = Math.Min(Math.Max(health.halloss + value, 0),max_health*2);
                UpdateHealth();
            }
    } 

    protected virtual void RangedInteraction(AbstractEntity target, Godot.Collections.Dictionary click_parameters)
    {
        if(HasTelegrip())
        {
            if(TOOLS.VecDist(GridPos.WorldPos(), target.GridPos.WorldPos()) > DAT.TK_MAXRANGE) return;
            target.InteractWithTK(this);
        }
    }

    /*****************************************************************
     * Damage handling
     ****************************************************************/
    public float HitByWeapon(AbstractEntity used_item, AbstractEntity user, float effective_force, DAT.ZoneSelection target_zone)
    {
        ChatController.VisibleMessage(this,this.display_name + " has been attacked with " + used_item?.display_name + " by " + user?.display_name + "!", ChatController.VisibleMessageFormatting.Danger);

        /* // TODO Mob AI ================================================================================================================
        if(ai_holder)
        {
            ai_holder.react_to_attack(user)
        }
        */

        // TODO Armor damage reduction ================================================================================================================
        float soaked = 0f;//get_armor_soak(hit_zone, "melee");
        float blocked = 0f;//run_armor_check(hit_zone, "melee");
        StandardWeaponHitEffects(used_item, user, effective_force, blocked, soaked, target_zone);

        if(DAT.DamageTypeBleeds(used_item.damtype) && TOOLS.Prob(33)) // Added blood for whacking non-humans too
        {
            if(GetLocation() is AbstractTurf turf_loc)
            {
                // turf_loc.AddBloodToFloor(this); // TODO bloody turf ================================================================================================================
            }
        }
        return blocked;
    }
    protected bool StandardWeaponHitEffects(AbstractEntity used_item, AbstractEntity user, float effective_force, float blocked, float soaked, DAT.ZoneSelection target_zone)
    {
        if(effective_force <= 0 || blocked >= 100f) return false;

        //If the armor soaks all of the damage, it just skips the rest of the checks
        if(effective_force <= soaked) return false;

        //Apply weapon damage
        bool weapon_sharp = false;
        bool weapon_edge = false;
        //float hit_embed_chance = 0f; // TODO
        if(used_item is AbstractItem used_weapon)
        {
            weapon_sharp = used_weapon.flags.ISSHARP;
            weapon_edge = used_weapon.flags.HASEDGE;
            //hit_embed_chance = used_weapon.embed_chance; // TODO
        }
        /* TODO Armor damage edge mitigation ================================================================================================================
        if(TOOLS.Prob(getarmor(hit_zone, "melee"))) //melee armour provides a chance to turn sharp/edge weapon attacks into blunt ones
        {
            weapon_sharp = false;
            weapon_edge = false;
            hit_embed_chance = used_item.attack_force/(used_item.w_class*3);
        }
        */

        ApplyDamage(effective_force, used_item.damtype, target_zone, blocked, soaked, used_item, weapon_sharp, weapon_edge);

        /* TODO Weapons getting embedded in target on hit ================================================================================================================
        //Melee weapon embedded object code.
        if (used_item != null && used_item.damtype == DAT.DamageType.BRUTE && !used_item.anchored && !is_robot_module(used_item) && used_item.embed_chance > 0)
        {
            float damage = effective_force;
            if(blocked > 0)
            {
                damage *= (100 - blocked)/100;
                hit_embed_chance *= (100 - blocked)/100;
            }
            //blunt objects should really not be embedding in things unless a huge amount of force is involved
            float embed_threshold = weapon_sharp? 5*(int)used_item.SizeCategory : 15*(int)used_item.SizeCategory;
            if(damage > embed_threshold && TOOLS.Prob(hit_embed_chance)) Embed(I, hit_zone);
        }
        */
        return true;
    }
    protected bool ApplyDamage(float damage = 0, DAT.DamageType damagetype = DAT.DamageType.BRUTE, DAT.ZoneSelection target_zone = DAT.ZoneSelection.Miss, float blocked = 0, float soaked = 0, AbstractEntity used_item = null, bool sharp = false, bool edge = false)
    {
        if(damage <= 0 || (blocked >= 100)) return false;
        if(soaked > 0)
        {
            if(soaked >= Mathf.Round(damage*0.8f))
            {
                damage -= Mathf.Round(damage*0.8f);
            }
            else
            {
                damage -= soaked;
            }
        }

        float initial_blocked = blocked;
        blocked = (100-blocked)/100;
        switch(damagetype)
        {
            case DAT.DamageType.BRUTE:
                BruteLoss = (int)(damage * blocked);
                break;
            case DAT.DamageType.BURN:
                FireLoss = (int)(damage * blocked);
                break;
            case DAT.DamageType.FREEZE:
                FireLoss = (int)(damage * blocked);
                break;
            case DAT.DamageType.SEARING:
                ApplyDamage(Mathf.Round(damage / 3), DAT.DamageType.BURN, target_zone, initial_blocked, soaked, used_item, sharp, edge);
                ApplyDamage(Mathf.Round(damage / 3 * 2), DAT.DamageType.BRUTE, target_zone, initial_blocked, soaked, used_item, sharp, edge);
                break;
            case DAT.DamageType.TOX:
                ToxLoss = (int)(damage * blocked);
                break;
            case DAT.DamageType.OXY:
                OxyLoss = (int)(damage * blocked);
                break;
            case DAT.DamageType.CLONE:
                CloneLoss = (int)(damage * blocked);
                break;
            case DAT.DamageType.HALLOSS:
                HalLoss = (int)(damage * blocked);
                break;
            case DAT.DamageType.ELECTROCUTE:
                // electrocute_act(damage, used_item, 1.0, target_zone); // TODO electrocution =========================================================================
                break;
            case DAT.DamageType.ACID:
                if(IsSynthetic())
                {
                    ApplyDamage(damage, DAT.DamageType.BURN, target_zone, initial_blocked, soaked, used_item, sharp, edge);	// Handle it as normal burn.
                }
                else
                {
                    ApplyDamage(Mathf.Round(damage / 3), DAT.DamageType.TOX, target_zone, initial_blocked, soaked, used_item, sharp, edge);
                    ApplyDamage(Mathf.Round(damage / 3 * 2), DAT.DamageType.BRUTE, target_zone, initial_blocked, soaked, used_item, sharp, edge);
                }
                break;
        }
        UpdateHealth();
        return true;
    }

    private void UpdateHealth()
    {
        float cur_health = max_health - FireLoss - BruteLoss - ToxLoss - OxyLoss - CloneLoss;
        //Alive, becoming dead
        if((stat != DAT.LifeState.Dead) && (cur_health <= 0)) Die();
        //Overhealth
        if(cur_health > max_health) cur_health = max_health;
        // TODO hud update ============================================================================================================
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
                if(!client_input_data["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) direction = DAT.VectorToCardinalDir((float)dat_x,(float)dat_y);
                speed = walk_speed;
            }
            else
            {
                // zoomies as normal
                new_pos.hor += (float)dat_x * run_speed;
                new_pos.ver += (float)dat_y * run_speed;
                if(!client_input_data["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) direction = DAT.VectorToCardinalDir((float)dat_x,(float)dat_y);
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
    public virtual bool IsSynthetic()
    {
        return false;
    }
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
