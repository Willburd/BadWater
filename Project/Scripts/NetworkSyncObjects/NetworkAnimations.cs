using Godot;
using System;

namespace NetwornAnimations
{
    public class Animation
    {
        public enum ID
        {
            Attack,
            Windup,
            Lunge,
            FadeOut,
            FadeOutInDirection
        }
        public static double LookupAnimationLength(ID id) // animation length and length of lockedstatus if applicable.
        {
            switch(id)
            {
                default:
                    return 0;
                case ID.Attack:
                    return 0.5;
            }
        }
        public static bool LookupAnimationLock(ID id) // If animation disables inputs
        {
            switch(id)
            {
                default:
                    return false;
                case ID.Attack:
                    return true;
            }
        }

        public double GetAnimationLength()
        {
            return LookupAnimationLength(id);
        }

        public bool GetAnimationLock()
        {
            return LookupAnimationLock(id);
        }

        public static Animation PlayAnimation(NetworkEntity host, ID id, Vector3 start_off, float start_alpha, Vector3 dir_vec)
        {
            Animation new_anim;
            switch(id)
            {
                default:
                case ID.Attack:
                    new_anim = new Attack();
                break;
            }
            
            // init and give to caller
            new_anim.Init(id,host,start_off,start_alpha,dir_vec);
            return new_anim;
        }   


        private ID id;
        protected NetworkEntity host_entity;

        protected double time_step;
        protected Vector3 direction;
        protected Vector3 start_offset;
        protected float start_alpha;

        public void Init(ID get_id, NetworkEntity host, Vector3 start_off, float start_alp, Vector3 dir_vec)
        {
            id = get_id;
            host_entity = host;
            start_offset = start_off;
            start_alpha = start_alp;
            direction = dir_vec;
            time_step = 0;
        }

        public virtual bool Process(double delta) { time_step += delta; return true; }
    }

    
    public class Attack : Animation
    {
        public override bool Process(double delta)
        {
            // Animate an attack swing
            Vector3 end_offset = direction * 0.25f;
            float percent = (float)(time_step / GetAnimationLength());
            double sin = Mathf.Sin( percent * Mathf.Pi );

            host_entity.SetAnimationVars( start_offset.Lerp(end_offset,(float)sin), 1f);

            // progress
            base.Process(delta);
            return time_step > GetAnimationLength(); // doesn't just return base.Process, because static news don't override if you call the base function, they are only relevant in the child class.
        }
    }
}