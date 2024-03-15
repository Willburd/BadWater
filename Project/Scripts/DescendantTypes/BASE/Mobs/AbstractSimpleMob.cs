using Godot;
using System;

namespace Behaviors_BASE
{
    public class AbstractSimpleMob : AbstractMob
    {
        public MobAI mob_ai;

        public enum LifeState
        {
            Alive,
            Unconscious,
            Dead
        }
        
        public LifeState stat = LifeState.Alive;

        public float footstep_timer = 0f;
        public int click_cooldown = 0;  // Time when mob cooldown has finished


        public override void ControlUpdate(Godot.Collections.Dictionary client_input_data)
        {
            // Got an actual control update!
            double dat_x = Mathf.Clamp(client_input_data["x"].AsDouble(),-1,1) * MainController.controller.config.input_factor;
            double dat_y = Mathf.Clamp(client_input_data["y"].AsDouble(),-1,1) * MainController.controller.config.input_factor;
            bool walking = client_input_data["walk"].AsBool();

            if(stat != LifeState.Dead)
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
            if(stat != LifeState.Alive)
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
                hand_item.AttackSelf(this);
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
            if(intangible) return; // shadekin can't interact with anything else! They already can't use their bag
                
            //Atoms on turfs (not on your person)
            // A is a turf or is on a turf, or in something on a turf (pen in a box); but not something in something on a turf (pen in a box in a backpack)
            storage_depth = target.StorageDepth(this);
            if(target is AbstractTurf || target.GetLocation() is AbstractTurf  || (storage_depth != -1 && storage_depth <= 1))
            {
                if(TOOLS.Adjacent(this,target) || (hand_item != null && hand_item.AttackCanReach(this, target, hand_item.reach)) )
                {
                    if(hand_item != null)
                    {
                        // Return 1 in attackby() to prevent afterattack() effects (when safely moving items for example)
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

        public virtual void RestrainedClick(AbstractEntity target)
        {
            GD.Print(display_name + " CLICKED " + target.display_name + " WHILE RESTRAINED"); // REPLACE ME!!!
        }

        public bool UnarmedAttack(AbstractEntity target, bool proximity)
        {
            if(intangible) return false;
            if(stat != LifeState.Alive) return false;
            GD.Print(display_name + " UNARMED ATTACKED " + target.display_name); // REPLACE ME!!!
            return true;
        }

        public void RangedAttack(AbstractEntity target, Godot.Collections.Dictionary click_parameters)
        {
            /*
            if(!mutations.len) return
            if((LASER in mutations) && a_intent == I_HURT)
                LaserEyes(A) // moved into a proc below
            else if(has_telegrip())
                if(get_dist(src, A) > tk_maxrange)
                    return
                A.attack_tk(src)
            */
            GD.Print(display_name + " RANGED ATTACKED " + target.display_name); // REPLACE ME!!!
            return;
        }

        public override void Tick()
        {
            if(stat != LifeState.Dead)
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

        // Check our current inventory and status... See if we need to drop objects from our hands or slots that no longer exist (uniforms for example give us pockets!)
        public virtual void ProcessSlotDrops()
        {   
            // knocked out and dead drops hands!
            if(stat != LifeState.Alive)
            {   
                DropSlot(AbstractMob.InventorySlot.Rhand);
                DropSlot(AbstractMob.InventorySlot.Lhand);
                DropSlot(AbstractMob.InventorySlot.RhandLower);
                DropSlot(AbstractMob.InventorySlot.LhandLower);
            }
            // Not wearing a uniform drops some slots all at once!
            if(!SlotInUse(AbstractMob.InventorySlot.Uniform)) 
            {
                DropSlot(AbstractMob.InventorySlot.Lpocket);
                DropSlot(AbstractMob.InventorySlot.Rpocket);
                DropSlot(AbstractMob.InventorySlot.Back);
                DropSlot(AbstractMob.InventorySlot.ID);
                DropSlot(AbstractMob.InventorySlot.Belt);
            }
        }

        protected virtual void LifeUpdate()
        {

        }

        protected virtual void DeathUpdate()
        {
            
        }


        public virtual void Bleed()
        {

        }

        public virtual void Die()
        {
            stat = LifeState.Dead;
        }


        // Conditions
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

        // Attacking procs!
        public void SetClickCooldown(int delay)
        {
            click_cooldown = Math.Max(MainController.WorldTicks + delay,click_cooldown);
        }
        public bool CheckClickCooldown()
        {
            return click_cooldown > MainController.WorldTicks;
        }
        public int GetAttackCooldown(AbstractEntity item_used)
        {
            if(item_used == null) return DAT.DEFAULT_ATTACK_COOLDOWN;
            // TODO - item_used attack speed reading ========================================================================================================
            return DAT.DEFAULT_ATTACK_COOLDOWN;
        }
    }
}