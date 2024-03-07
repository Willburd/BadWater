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
                // Move based on mob speed
                MapController.GridPos new_pos = owner.GridPos;
                if(input["mod_alt"].AsBool())
                {
                    // Only face the direction pressed!
                    // Not moving!
                }
                else if(input["mod_control"].AsBool())
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
                }
                else
                {
                    // zoomies as normal
                    new_pos.hor += (float)dat_x * mob.run_speed;
                    new_pos.ver += (float)dat_y * mob.run_speed;
                }
                owner.Move(owner.map_id_string, new_pos);
            }
            else
            {
                // dead or knocked out...
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
    }
}