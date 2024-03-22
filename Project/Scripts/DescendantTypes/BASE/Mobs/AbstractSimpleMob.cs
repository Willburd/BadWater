using Godot;
using System;
using System.Collections.Generic;

namespace Behaviors_BASE
{
    public class AbstractSimpleMob : AbstractMob, ICanPull, IPullable
    {
        public AbstractSimpleMob()
        {
            inventory_slots = new AbstractEntity[ Enum.GetNames(typeof(DAT.InventorySlot)).Length ];
        }
        private AIHolder ai_holder;

        public override void TemplateRead(PackData data)
        {
            base.TemplateRead(data);
            MobData temp = data as MobData;
            max_health      = temp.max_health;
            walk_speed      = temp.walk_speed;
            run_speed       = temp.run_speed;
            mob_size        = temp.mob_size;
            pull_size       = temp.pull_size;
            pull_type       = temp.pull_type;
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
        public float walk_speed = (float)0.25;
        public float run_speed = 1;
        DAT.SizeCategory mob_size = DAT.SizeCategory.MEDIUM;
        DAT.SizeCategory pull_size = DAT.SizeCategory.ITEMSIZE_NO_CONTAINER;
        DAT.CanPullType pull_type = DAT.CanPullType.PULL_LARGER;
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

        public struct AttackData
        {
            public AttackData() {}
            //Attack ranged settings
            //var/projectiletype				// The projectiles I shoot
            //var/projectilesound				// The sound I make when I do it
            //var/projectile_accuracy = 0		// Accuracy modifier to add onto the bullet when its fired.
            //var/projectile_dispersion = 0	// How many degrees to vary when I do it.
            //var/casingtype					// What to make the hugely laggy casings pile out of

            // Reloading settings, part of ranged code
            public bool needs_reload = true;							// If TRUE, mob needs to reload occasionally
            public int reload_max = 1;									// How many shots the mob gets before it has to reload, will not be used if needs_reload is FALSE
            public int reload_count = 0;								// A counter to keep track of how many shots the mob has fired so far. Reloads when it hits reload_max.
            //int reload_time = 4 SECONDS						// How long it takes for a mob to reload. This is to buy a player a bit of time to run or fight.
            public string reload_sound = "sound/weapons/flipblade.ogg";// What sound gets played when the mob successfully reloads. Defaults to the same sound as reloading guns. Can be null.

            //Mob melee settings
            public int melee_damage_lower = 2;		// Lower bound of randomized melee damage
            public int melee_damage_upper = 6;		// Upper bound of randomized melee damage
            public List<string> attacktext = new List<string>{"attacked"};  // "You are [attacktext] by the mob!"
            public List<string> friendly = new List<string>{"nuzzles"}; // "The mob [friendly] the person."
            public string attack_sound = null;				// Sound to play when I attack
            public int melee_miss_chance = 0;			// percent chance to miss a melee attack.
            public DAT.ArmorType attack_armor_type = DAT.ArmorType.Melee;// What armor does this check?
            public int attack_armor_pen = 0;			// How much armor pen this attack has.
            public bool attack_sharp = false;			// Is the attack sharp?
            public bool attack_edge = false;				// Does the attack have an edge?

            public int? melee_attack_delay = 2;			// If set, the mob will do a windup animation and can miss if the target moves out of the way.
            public int ranged_attack_delay = 0;
            public int special_attack_delay = 0;

            //Special attacks
            public int special_attack_min_range = 0;		// The minimum distance required for an attempt to be made.
            public int special_attack_max_range = 0;		// The maximum for an attempt.
            public int? special_attack_charges = null;		// If set, special attacks will work off of a charge system, and won't be usable if all charges are expended. Good for grenades.
            public int? special_attack_cooldown = null;    // If set, special attacks will have a cooldown between uses.
        }
        public AttackData attacks = new AttackData();    // Size of item in world and bags
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
        private List<AbstractEntity> embedded_objects = new List<AbstractEntity>();
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
            // Release pulled objects before we drop anything...
            if(I_Pulling != null)
            {
                I_StopPulling();
                return;
            }
            // Dropping whatever we're holding!
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

        /*
            target_zone	- What part is getting hit, if null will check entire body
            attack_flag	- What type of attack, bullet, laser, energy, melee
            armour_pen	- How much armor to ignore.
            absorb_text	- Custom text to send to the player when the armor fully absorbs an attack.
            soften_text	- Similar to absorb_text, custom text to send to the player when some damage is reduced.

            Returns
            A number between 0 and 100, with higher numbers resulting in less damage taken.
        */
        public float RunArmorCheck(DAT.ZoneSelection target_zone =  DAT.ZoneSelection.Miss, DAT.ArmorType attack_flag = DAT.ArmorType.Melee, float armour_pen = 0, string absorb_text = "", string soften_text = "")
        {
            if(armour_pen >= 100) return 0; //might as well just skip the processing

            float armor = GetArmor(target_zone, attack_flag);
            if(armor > 0)
            {
                float armor_variance_range = Mathf.Round(armor * 0.25f); //Armor's effectiveness has a +25%/-25% variance.
                float armor_variance = TOOLS.RandF(-armor_variance_range, armor_variance_range); //Get a random number between -25% and +25% of the armor's base value
                
                armor = Mathf.Min(armor + armor_variance, 100);	//Now we calcuate damage using the new armor percentage.
                armor = Mathf.Max(armor - armour_pen, 0);			//Armor pen makes armor less effective.
                if(armor >= 100)
                {
                    if(absorb_text.Length > 0)
                    {
                        ChatController.InspectMessage( this, absorb_text, ChatController.VisibleMessageFormatting.Danger);
                    }
                    else
                    {
                        ChatController.InspectMessage( this, "Your armor absorbs the blow!", ChatController.VisibleMessageFormatting.Danger);
                    }
                }
                else if(armor > 0)
                {
                    if(soften_text.Length > 0)
                    {
                        ChatController.InspectMessage( this, soften_text, ChatController.VisibleMessageFormatting.Danger);
                    }
                    else
                    {
                        ChatController.InspectMessage( this, "Your armor softens the blow!", ChatController.VisibleMessageFormatting.Danger);
                    }
                }
            }
            return armor;
        }
                
        //Certain pieces of armor actually absorb flat amounts of damage from income attacks
        public float GetArmorSoak(DAT.ZoneSelection target_zone = DAT.ZoneSelection.Miss, DAT.ArmorType attack_flag = DAT.ArmorType.Melee, float armour_pen = 0)
        {
            float soaked = GetSoak(target_zone, attack_flag);
            //5 points of armor pen negate one point of soak
            if(armour_pen > 0) soaked = Mathf.Max(soaked - (armour_pen/5), 0);
            return soaked;
        }

        public virtual float GetArmor(DAT.ZoneSelection target_zone, DAT.ArmorType armor_type)
        {
            return 0;
        }
        
        public virtual float GetSoak(DAT.ZoneSelection target_zone, DAT.ArmorType armor_type)
        {
            return 0;
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

            // prevent animation cancel
            if(GetAnimationLock()) return;

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
                if(TOOLS.Adjacent(this,target,false))
                {
                    if(target is AbstractTurf) I_StopPulling();
                    if(target is IPullable target_pull) I_TryStartPulling(target_pull);
                }
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
                    if(!resolved && target != null && hand_item != null) hand_item.InteractionUnresolved( this, target, true, click_params);
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
                        if(!hand_item._Interact( target, this, 1f, click_params) && target != null && hand_item != null) hand_item.InteractionUnresolved(target, this, true, click_params);
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
                        hand_item.InteractionUnresolved(target, this, false, click_params);
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
            if(this is AbstractSimpleMob self_mob && self_mob.Stat != DAT.LifeState.Alive) return;

            // Special glove functions:
            // If the gloves do anything, have them return true to stop
            // normal _UnarmedInteract() here.
            AbstractEntity gloves = SlotGloves; // not typecast specifically enough in defines
            //if(gloves != null && gloves.Touch(target,true)) return;

            if( flags.HASHANDS && (target is AbstractTurf ||  target is AbstractStructure || target is AbstractMachine ) && SelectingIntent != DAT.Intent.Hurt)
            {
                target.InteractionTouched(this);
                return;
            }

            switch(SelectingIntent)
            {
                case DAT.Intent.Help:
                    if(target is AbstractMob target_mob)
                    {
                        ChatController.VisibleMessage( this, this.display_name + " " + TOOLS.Pick(attacks.friendly) + " the " + target_mob.display_name + ".");
                    }
                    break;

                
                case DAT.Intent.Hurt:
                    /*if(can_special_attack(A) && special_attack_target(A))
                    {
                        return;
                    }
                    else*/ if(attacks.melee_damage_upper == 0 && target is AbstractMob)
                    {
                        ChatController.VisibleMessage( this, this.display_name + " " + TOOLS.Pick(attacks.friendly) + " the " + target.display_name + "!");
                    }
                    else
                    {
                        UnarmedAttackTarget(target);
                    }
                    break;
            }
        }

        protected virtual void RestrainedClick(AbstractEntity target)
        {
            GD.Print(display_name + " CLICKED " + target.display_name + " WHILE RESTRAINED"); // REPLACE ME!!!
        }

        protected virtual void RangedInteraction(AbstractEntity target, Godot.Collections.Dictionary click_parameters)
        {
            if(HasTelegrip())
            {
                if(TOOLS.VecDist(GridPos.WorldPos(), target.GridPos.WorldPos()) > DAT.TK_MAXRANGE) return;
                target.InteractWithTK(this);
            }
        }

        protected override void Examinate(AbstractEntity target)
        {
            if(IsBlind() || stat != DAT.LifeState.Alive)
            {
                ChatController.InspectMessage( this, "Something is there but you can't see it.", ChatController.VisibleMessageFormatting.Notice);
                return;
            }
            base.Examinate(target);
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

        public void UnarmedAttackTarget(AbstractEntity target)
        {
            if(!TOOLS.Adjacent( this, target, false)) return;
            AbstractTurf turf = target.GetTurf();

            direction = TOOLS.RotateTowardEntity(this,target);

            if(attacks.melee_attack_delay != null && attacks.melee_attack_delay.Value > 0)
            {
                // TODO =======================================================================================================================
                //melee_pre_animation(target)
                //. = ATTACK_SUCCESSFUL //Shoving this in here as a 'best guess' since this proc is about to sleep and return and we won't be able to know the real value
                //handle_attack_delay(target, melee_attack_delay) // This will sleep this proc for a bit, which is why waitfor is false.
            }

            // Cooldown testing is done at click code (for players) and interface code (for AI).
            SetClickCooldown(GetAttackCooldown(null));

            // Returns a value, but will be lost if 
            DoUnarmedAttack( target, turf);

            if(attacks.melee_attack_delay != null && attacks.melee_attack_delay.Value > 0)
            {
                // TODO =======================================================================================================================
                //melee_post_animation(target);
            }
        }

        public void DoUnarmedAttack(AbstractEntity target, AbstractTurf turf)
        {
            direction = TOOLS.RotateTowardEntity(this,target);
            bool missed = false;
            if(target is AbstractTurf target_turf)
            {
                target_turf.AttackTurf(null,this);
                return;
            }
            if(!turf.Contents.Contains(target) ) // Turfs don't contain themselves so checking contents is pointless if we're targeting a turf.
            {
                missed = true;
            }
            else if(!TOOLS.Adjacent(this,turf,false))
            {
                missed = true;
            }
            if(missed) // Most likely we have a slow attack and they dodged it or we somehow got moved.
            {
                ChatController.LogAttack(display_name + " Animal-attacked (dodged) " + target?.display_name);
                LoadedNetworkEntity?.AnimationRequest(NetwornAnimations.Animation.ID.Attack, TOOLS.DirVec(GridPos.WorldPos(),turf.GridPos.WorldPos()) );
                AudioController.PlayAt("BASE/Attacks/Punch/Miss", map_id_string ,grid_pos.WorldPos(), AudioController.screen_range, 0);
                ChatController.VisibleMessage(this,"The " + display_name + " misses their attack.", ChatController.VisibleMessageFormatting.Warning);
                return;
            }

            int damage_to_do = TOOLS.RandI(attacks.melee_damage_lower, attacks.melee_damage_upper);
            damage_to_do = ApplyBonusMeleeDamage( target, damage_to_do);

            if(target is AbstractMob mob_target) // Check defenses.
            {
                if(TOOLS.Prob(attacks.melee_miss_chance))
                {
                    ChatController.LogAttack(display_name + " Animal-attacked (miss) " + mob_target?.display_name);
                    LoadedNetworkEntity?.AnimationRequest(NetwornAnimations.Animation.ID.Attack, TOOLS.DirVec(GridPos.WorldPos(),mob_target.GridPos.WorldPos()) );
                    AudioController.PlayAt("BASE/Attacks/Punch/Miss", map_id_string ,grid_pos.WorldPos(), AudioController.screen_range, 0);
                    return; // We missed.
                }
                // TODO shields ==================================================================================================================================
                // if(H.check_shields(damage = damage_to_do, damage_source = src, attacker = src, def_zone = null, attack_text = "the attack")) return; // We were blocked.
            }

            if(ApplyAttack( target, damage_to_do))
            {
                ApplyMeleeEffects(target);
                AudioController.PlayAt(attacks.attack_sound, map_id_string ,grid_pos.WorldPos(), AudioController.screen_range, 0);
            }
        }

        // Override for special effects after a successful attack, like injecting poison or stunning the target.
        public virtual void ApplyMeleeEffects( AbstractEntity target)
        {

        }

        // Override to modify the amount of damage the mob does conditionally.
        // This must return the amount of outgoing damage.
        // Note that this is done before mob modifiers scale the damage.
        public virtual int ApplyBonusMeleeDamage( AbstractEntity target, int damage_amount)
        {
            return damage_amount;
        }

        // Generally used to do the regular attack.
        // Override for doing special stuff with the direct result of the attack.
        public bool ApplyAttack(AbstractEntity target, int damage_to_do)
        {
            return target.AttackedGeneric( this, damage_to_do, TOOLS.Pick(attacks.attacktext));
        }

        /*****************************************************************
         * Damage handling
         ****************************************************************/
        public float HitByWeapon(AbstractEntity used_item, AbstractEntity user, float effective_force, DAT.ZoneSelection target_zone)
        {
            ChatController.VisibleMessage(this,this.display_name + " has been attacked with " + used_item?.display_name + " by " + user?.display_name + "!", ChatController.VisibleMessageFormatting.Danger);
            ai_holder?.ReactToAttack(user);

            float soaked = GetArmorSoak(target_zone, DAT.ArmorType.Melee);
            float blocked = RunArmorCheck(target_zone, DAT.ArmorType.Melee);
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
            float hit_embed_chance = 0f;
            if(used_item is AbstractItem used_weapon)
            {
                weapon_sharp = used_weapon.flags.ISSHARP;
                weapon_edge = used_weapon.flags.HASEDGE;
                hit_embed_chance = used_weapon.embed_chance;
            }
            if(TOOLS.Prob(GetArmor(target_zone, DAT.ArmorType.Melee))) //melee armour provides a chance to turn sharp/edge weapon attacks into blunt ones
            {
                weapon_sharp = false;
                weapon_edge = false;
                hit_embed_chance = used_item.attack_force/((int)used_item.SizeCategory*3);
            }

            ApplyDamage(effective_force, used_item.damtype, target_zone, blocked, soaked, used_item, weapon_sharp, weapon_edge);

            //Melee weapon embedded object code.
            if (used_item != null && used_item.damtype == DAT.DamageType.BRUTE && !used_item.IsAnchored() && !used_item.IsRobotModule() && used_item.embed_chance > 0)
            {
                float damage = effective_force;
                if(blocked > 0)
                {
                    damage *= (100 - blocked)/100;
                    hit_embed_chance *= (100 - blocked)/100;
                }
                //blunt objects should really not be embedding in things unless a huge amount of force is involved
                float embed_threshold = weapon_sharp? 5*(int)used_item.SizeCategory : 15*(int)used_item.SizeCategory;
                if(damage > embed_threshold && TOOLS.Prob(hit_embed_chance)) Embed(used_item, target_zone);
            }
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

        public override bool AttackedGeneric(AbstractEntity user, int damage, string attack_message)
        {
            if(damage <= 0) return false;
            BruteLoss = damage;
            
            ChatController.LogAttack(user?.display_name + " Generic attacked (probably animal) " + display_name); //Usually due to simple_mob attacks
            ai_holder?.ReactToAttack(user);
            
            ChatController.VisibleMessage(this,"The " + user?.display_name + " has " + attack_message + " the " + display_name + "!", ChatController.VisibleMessageFormatting.Danger);
            user?.LoadedNetworkEntity?.AnimationRequest(NetwornAnimations.Animation.ID.Attack, TOOLS.DirVec(user.GridPos.WorldPos(),GridPos.WorldPos()) );
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

        protected void Embed(AbstractEntity used_item, DAT.ZoneSelection target_zone)
        {
            used_item.Move(this,false);
            embedded_objects.Add(used_item);
        }
        
        /*****************************************************************
         * Movement and storage
         ****************************************************************/
        public override AbstractEntity Move(string new_mapID, MapController.GridPos new_grid, bool perform_turf_actions = true)
        {
            // Prior to our move it's already too far away
            AbstractEntity pull_ent = I_Pulling as AbstractEntity;
            if(pull_ent != null && TOOLS.VecDist(this.GridPos.WorldPos(),pull_ent.GridPos.WorldPos()) > 1.3f) I_StopPulling();
            // Shenanigans! Pullee closed into locker for eg.
            if(pull_ent != null && pull_ent.GetLocation() is not AbstractTurf && pull_ent.map_id_string != map_id_string) I_StopPulling();
            // Can't pull with no hands
            if(pull_ent != null && IsRestrained()) I_StopPulling();

            if(map_id_string != new_mapID)
            {
                // warp to same location if jumped maps
                pull_ent?.Move(new_mapID, new MapController.GridPos( GridPos.WorldPos()), perform_turf_actions);
            }
            else if(GridPos.WorldPos() != new_grid.WorldPos())
            {
                // Pulling logic
                if(I_Pulling != null && I_Pulledby is AbstractEntity pullerEnt)
                {
                    // Don't allow us to go far from what's pulling us! Use resist for that!
                    if(TOOLS.VecDist(new_grid.WorldPos(),pullerEnt.GridPos.WorldPos()) > 1.1f) return GetLocation(); // Only move toward puller!
                }
                pull_ent?.Move(new_mapID, new MapController.GridPos( pull_ent.GridPos.WorldPos() + ICanPull.Internal_HandlePull(this)), perform_turf_actions);
            }
            return base.Move(new_mapID, new_grid, perform_turf_actions);
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
            
            // prevent animation canceling
            bool locked_anim = GetAnimationLock();
            if(locked_anim)
            {
                dat_x = 0f;
                dat_y = 0f;
            }

            if(stat != DAT.LifeState.Dead)
            {
                // Trigger mob actions
                if(!locked_anim && client_input_data["resist"].AsBool());
                if(!locked_anim && client_input_data["rest"].AsBool());
                if(!locked_anim && client_input_data["equip"].AsBool()) EquipActiveHand(null);
                if(!locked_anim && client_input_data["useheld"].AsBool()) UseActiveHand(null);

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
                if(dat_x != 0 || dat_y != 0) footstep_timer += Mathf.Lerp(0.075f,0.10f, Mathf.Clamp(speed,0,1.5f));
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
                I_StopPulling();
            }

            // Respond in any state, as they are mostly just input states for actions!
            if(!locked_anim && client_input_data["intentswap"].AsBool()) IntentSwap();
            if(!locked_anim && client_input_data["swap"].AsBool()) SwapHands();
            if(!locked_anim && client_input_data["throw"].AsBool());
            if(!locked_anim && client_input_data["drop"].AsBool()) DropActiveHand();
        }
        public override void Tick()
        {
            if(stat != DAT.LifeState.Dead)
            {
                LifeUpdate();
                ai_holder?.Alive();
            }
            else
            {
                DeathUpdate();
                ai_holder?.Dead();
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
            I_StopPulling();
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
         * Object pulling
         ****************************************************************/
        private ICanPull internal_pulled_by = null;
        private IPullable internal_is_pulling = null;

        public IPullable I_Pulling 
        {
            get {return internal_is_pulling;}
            set {internal_is_pulling = value;}
        }
        public ICanPull I_Pulledby 
        {
            get {return internal_pulled_by;}
            set {internal_pulled_by = value;}
        }

        public void I_TryStartPulling(IPullable target)
        {
            AbstractEntity target_ent = target as AbstractEntity;
            if(target_ent == null || target_ent == this || target_ent.GetLocation() is not AbstractTurf) return;	//if there's no person pulling OR the person is pulling themself OR the object being pulled is inside something: abort!

            if(IsIntangible())
            {
                ChatController.InspectMessage( this, "Cannot pull while phased!", ChatController.VisibleMessageFormatting.Notice);
                return; // stop shadekin from pulling while phased
            }
            if(IsAnchored())
            {
                ChatController.InspectMessage( this, "It won't budge!", ChatController.VisibleMessageFormatting.Warning);
                return;
            }

            if(target_ent is AbstractSimpleMob target_mob)
            {
                if(pull_type == DAT.CanPullType.PULL_NONE)
                {
                    ChatController.InspectMessage( this, "They won't budge!", ChatController.VisibleMessageFormatting.Warning);
                    return;
                }
                if((mob_size < target_mob.mob_size) && (pull_type != DAT.CanPullType.PULL_LARGER))
                {
                    ChatController.InspectMessage( this, target_ent.display_name + " is too large for you to move!", ChatController.VisibleMessageFormatting.Warning);
                    return;
                }
                if((mob_size == target_mob.mob_size) && (pull_type == DAT.CanPullType.PULL_SMALLER))
                {
                    ChatController.InspectMessage( this, target_ent.display_name + " is too heavy for you to move!", ChatController.VisibleMessageFormatting.Warning);
                    return;
                }

                // If your size is larger than theirs and you have some
                // kind of mob pull value AT ALL, you will be able to pull
                // them, so don't bother checking that explicitly.

                if(target.I_Pulledby != null )
                {
                    // Ripping others out of someone ELSES grip!
                    AbstractEntity pull_ent = target.I_Pulledby as AbstractEntity;
                    if(pull_ent != this)
                    {
                        ChatController.VisibleMessage( target_ent, target_ent.display_name + " is pulled free from " + pull_ent?.display_name + "'s grip by the " + this.display_name + "!");
                        ICanPull.Internal_EndPull(target.I_Pulledby);
                    }
                }
            }
            else if(target is AbstractMachine || target is AbstractItem || target is AbstractStructure)
            {
                /* Handling massive unpullable objects! TODO ==================================================================================================================
                var/obj/I = AM
                if(!can_pull_size || can_pull_size < I.w_class)
                    to_chat(src, "<span class='warning'>It won't budge!</span>")
                    return
                */
            }

            // End if already pulling
            if(I_Pulling == target) // Clicking on object we are already pulling, release it!
            {
                ICanPull.Internal_EndPull(this);
                return;
            }

            // Begin pulling!
            ICanPull.Internal_BeginPull(this,target);
            target_ent.velocity *= 0;

            /* TODO feedback messages for complex mobs ========================================================================================================
            if(ishuman(AM))
                var/mob/living/carbon/human/H = AM
                if(H.lying) // If they're on the ground we're probably dragging their arms to move them
                    visible_message(SPAN_WARNING("\The [src] leans down and grips \the [H]'s arms."), SPAN_NOTICE("You lean down and grip \the [H]'s arms."), exclude_mobs = list(H))
                    if(!H.stat)
                        to_chat(H, SPAN_WARNING("\The [src] leans down and grips your arms."))
                else //Otherwise we're probably just holding their arm to lead them somewhere
                    visible_message(SPAN_WARNING("\The [src] grips \the [H]'s arm."), SPAN_NOTICE("You grip \the [H]'s arm."), exclude_mobs = list(H))
                    if(!H.stat)
                        to_chat(H, SPAN_WARNING("\The [src] grips your arm."))
                playsound(src.loc, 'sound/weapons/thudswoosh.ogg', 25) //Quieter than hugging/grabbing but we still want some audio feedback

                if(H.pull_damage())
                    to_chat(src, "<font color='red'><B>Pulling \the [H] in their current condition would probably be a bad idea.</B></font>")
            */
        }
        public void I_StopPulling()
        {
            if(I_Pulling == null) return;
            AbstractEntity pulling_ent = I_Pulling as AbstractEntity;
            
            // Send message and release
            ChatController.ActionMessage( this, "You release your grasp on the " + pulling_ent?.display_name,"The " + display_name + " lets go of the " + pulling_ent.display_name, null, ChatController.VisibleMessageFormatting.Warning);
            ChatController.InspectMessage( pulling_ent, "The " + display_name + " lets go of you.");
            ICanPull.Internal_EndPull(this);
        }
    }
}