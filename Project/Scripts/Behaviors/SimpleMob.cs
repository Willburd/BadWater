using Godot;
using System;

namespace BehaviorEvents
{
    // Handles construction clicks
    public partial class SimpleMob : Behavior
    {
        public MobAI mob_ai;

        public enum LifeState
        {
            Alive,
            Unconscious,
            Dead
        }
        
        public LifeState stat = LifeState.Alive;

        public float footstep_timer = 0;

        public SimpleMob()
        {

        }

        public override void Init(AbstractEntity owner, MainController.DataType entity_type)
        {
            
        }

        public override void MapLoadVars(Godot.Collections.Dictionary data)
        {

        }

        public override void HandleInput(AbstractEntity owner, MainController.DataType entity_type, Godot.Collections.Dictionary input)
        {
            AbstractMob mob = owner as AbstractMob;

            // Got an actual control update!
            double dat_x = Mathf.Clamp(input["x"].AsDouble(),-1,1) * MainController.controller.config.input_factor;
            double dat_y = Mathf.Clamp(input["y"].AsDouble(),-1,1) * MainController.controller.config.input_factor;
            bool walking = input["walk"].AsBool();

            if(stat != LifeState.Dead)
            {
                // Trigger mob actions
                if(input["resist"].AsBool())
                {

                }
                if(input["rest"].AsBool())
                {
                    
                }
                if(input["equip"].AsBool())
                {
                    mob.EquipActiveHand(null);
                }
                if(input["useheld"].AsBool())
                {
                    mob.UseActiveHand(null);
                }

                // Move based on mob speed
                MapController.GridPos new_pos = mob.GridPos;
                float speed = 0f;
                if(input["mod_control"].AsBool())
                {
                    // Inching along with taps at a fixed rate
                    new_pos.hor += (float)dat_x * 0.5f;
                    new_pos.ver += (float)dat_y * 0.5f;
                }
                else if(walking)
                {
                    // slower safer movement
                    new_pos.hor += (float)dat_x * mob.walk_speed;
                    new_pos.ver += (float)dat_y * mob.walk_speed;
                    if(!input["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) mob.direction = DAT.InputToCardinalDir((float)dat_x,(float)dat_y);
                    speed = mob.walk_speed;
                }
                else
                {
                    // zoomies as normal
                    new_pos.hor += (float)dat_x * mob.run_speed;
                    new_pos.ver += (float)dat_y * mob.run_speed;
                    if(!input["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) mob.direction = DAT.InputToCardinalDir((float)dat_x,(float)dat_y);
                    speed = mob.run_speed;
                }
                // math for feet speed
                if(dat_x != 0 || dat_y != 0) footstep_timer += Mathf.Lerp(0.05f,0.08f, Mathf.Clamp(speed,0,1.5f));
                AbstractEntity newloc = owner.Move(owner.map_id_string, new_pos);
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
            if(input["swap"].AsBool())
            {
                mob.SwapHands();
            }
            if(input["throw"].AsBool())
            {
                
            }
            if(input["drop"].AsBool())
            {
                mob.DropActiveHand();
            }
        }

        public override void Tick(AbstractEntity owner, MainController.DataType entity_type)
        {
            AbstractMob mob = owner as AbstractMob;

            if(stat != LifeState.Dead)
            {
                LifeUpdate(owner, entity_type);
                mob_ai?.Alive();
            }
            else
            {
                DeathUpdate(owner, entity_type);
                mob_ai?.Dead();
            }
            ProcessSlotDrops(owner, entity_type);
        }

        // Check our current inventory and status... See if we need to drop objects from our hands or slots that no longer exist (uniforms for example give us pockets!)
        public virtual void ProcessSlotDrops(AbstractEntity owner, MainController.DataType entity_type)
        {   
            AbstractMob mob = owner as AbstractMob;
            // knocked out and dead drops hands!
            if(stat != LifeState.Alive)
            {   
                mob.DropSlot(AbstractMob.InventorySlot.Rhand);
                mob.DropSlot(AbstractMob.InventorySlot.Lhand);
                mob.DropSlot(AbstractMob.InventorySlot.RhandLower);
                mob.DropSlot(AbstractMob.InventorySlot.LhandLower);
            }
            // Not wearing a uniform drops some slots all at once!
            if(!mob.SlotInUse(AbstractMob.InventorySlot.Uniform)) 
            {
                mob.DropSlot(AbstractMob.InventorySlot.Lpocket);
                mob.DropSlot(AbstractMob.InventorySlot.Rpocket);
                mob.DropSlot(AbstractMob.InventorySlot.Back);
                mob.DropSlot(AbstractMob.InventorySlot.ID);
                mob.DropSlot(AbstractMob.InventorySlot.Belt);
            }
        }

        protected virtual void LifeUpdate(AbstractEntity owner, MainController.DataType entity_type)
        {

        }

        protected virtual void DeathUpdate(AbstractEntity owner, MainController.DataType entity_type)
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

        // Click interactions
        public override void Clicked(AbstractEntity self, MainController.DataType entity_type, AbstractEntity use_item, AbstractEntity target, Godot.Collections.Dictionary click_params)
        {
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
            self.direction = TOOLS.RotateTowardEntity(self,target);
            // Mecha held control
            // Check restrained
            if(IsRestrained())
            {
                RestrainedClick(self,entity_type,target);
                return;
            }
            // Check throwmode
            // Vehicle helm control
            
            // Handle using items on themselves
            AbstractEntity hand_item = self.ActiveHand;
            if(hand_item == target) 
            {
                hand_item.AttackSelf(self);
            }
            // Interacting with entities directly in your inventory
            int storage_depth = target.StorageDepth(self);
            if((target is not AbstractTurf && target == self.GetLocation()) || (storage_depth != -1 && storage_depth <= 1))
            {
                if(hand_item != null)
                {
                    bool resolved = false; //hand_item.resolve_attackby(target, self, click_parameters = params);
                    if(!resolved && target != null && hand_item != null)
                    {
                        //hand_item.afterattack(target, self, 1, params); // 1 indicates adjacency
                    }
                }
                else
                {
                    if(target is AbstractMob); // No instant mob attacking
                    {
                        //SetClickCooldown(get_attack_speed());
                    }
                    //UnarmedAttack(target, 1);
                }
                return;
            }
            


            GD.Print(self.display_name + " CLICKED " + target.display_name + " using " + use_item?.display_name); // REPLACE ME!!!
        }

        public override void Dragged(AbstractEntity self, MainController.DataType entity_type, AbstractEntity user, AbstractEntity target,Godot.Collections.Dictionary click_params)
        {
            GD.Print(user.display_name + " DRAGGED " + self.display_name + " TO " + target.display_name); // REPLACE ME!!!
        }

        public virtual void RestrainedClick(AbstractEntity self, MainController.DataType entity_type, AbstractEntity target)
        {
            GD.Print(self.display_name + " CLICKED " + target.display_name + " WHILE RESTRAINED"); // REPLACE ME!!!
        }
    }
}