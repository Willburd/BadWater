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
            // Got an actual control update!
            double dat_x = input["x"].AsDouble();
            double dat_y = input["y"].AsDouble();

            if(stat != LifeState.Dead)
            {
                // Move based on mob speed
                MapController.GridPos new_pos = owner.GridPos;
                new_pos.hor += (float)dat_x;
                new_pos.ver += (float)dat_y;
                owner.Move(owner.map_id_string, new_pos);
            }
            else
            {
                // dead or knocked out...
            }
        }

        public override void Tick(AbstractEntity owner, MainController.DataType entity_type)
        {
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