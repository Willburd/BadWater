using Godot;
using System;

namespace Behaviors_BASE
{
    public class AbstractObserver : AbstractMob
    {
        /*****************************************************************
         * Click handling
         ****************************************************************/
        public override void Clicked( AbstractEntity used_item, AbstractEntity target, Godot.Collections.Dictionary click_params) 
        {
            // Don't react if click cooldown
            if(CheckClickCooldown()) return;
            SetClickCooldown(1);

            // You can ONLY LOOK!
            Examinate(target);
        }

        /*****************************************************************
         * Processing
         ****************************************************************/
        public override void ControlUpdate(Godot.Collections.Dictionary client_input_data)
        {
            // Got an actual control update!
            double dat_x = Mathf.Clamp(client_input_data["x"].AsDouble(),-1,1);
            double dat_y = Mathf.Clamp(client_input_data["y"].AsDouble(),-1,1);
            bool walking = client_input_data["walk"].AsBool();

            // Move based on mob speed
            MapController.GridPos new_pos = GridPos;
            if(client_input_data["mod_control"].AsBool())
            {
                // Inching along with taps at a fixed rate
                new_pos.hor += (float)dat_x * 0.5f;
                new_pos.ver += (float)dat_y * 0.5f;
            }
            else if(walking)
            {
                // slower safer movement
                new_pos.hor += (float)dat_x * 0.5f;
                new_pos.ver += (float)dat_y * 0.5f;
                if(!client_input_data["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) direction = DAT.VectorToCardinalDir((float)dat_x,(float)dat_y);
            }
            else
            {
                // zoomies as normal
                new_pos.hor += (float)dat_x * 1f;
                new_pos.ver += (float)dat_y * 1f;
                if(!client_input_data["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) direction = DAT.VectorToCardinalDir((float)dat_x,(float)dat_y);
            }
        }
    }
}