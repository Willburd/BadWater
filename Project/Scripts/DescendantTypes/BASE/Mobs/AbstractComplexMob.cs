using Godot;
using System;

namespace Behaviors_BASE
{
    public class AbstractComplexMob : AbstractSimpleMob
    {
        

        public override void RangedAttack(AbstractEntity target, Godot.Collections.Dictionary click_parameters)
        {
            if(false /*(LASER in mutations)*/ && SelectingIntent == DAT.Intent.Hurt)
            {
                // LASUHBEAMZ
                LaserEyes(target);
            }
            else
            {
                // Handle TK
                base.RangedAttack(target,click_parameters);
            }
        }

        /*****************************************************************
         * Specialty and mutations
         ****************************************************************/
        public void LaserEyes(AbstractEntity target)
        {
            GD.Print("PEW PEW PEW TODO!"); // TODO =======================================================================================================================
        }

        /*****************************************************************
         * Conditions
         ****************************************************************/
        public override bool HasTelegrip()
        {
            if(false /*(TELE in mutations)*/ ) return true; // TODO =======================================================================================================================
            return HasTelegrip();
        }
    }
}