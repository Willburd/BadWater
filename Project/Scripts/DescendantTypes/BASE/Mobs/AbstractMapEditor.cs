using Godot;
using System;

namespace Behaviors
{
    public class AbstractMapEditor : AbstractMob
    {
        /*****************************************************************
         * Click handling
         ****************************************************************/
        public override void ClientClicking( AbstractEntity used_item, AbstractEntity target, Godot.Collections.Dictionary click_params) 
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
            if(client_input_data["mod_control"].AsBool())
            {
                // Inching along with taps at a fixed rate
                input_dir = new Vector2((float)dat_x * 0.5f,(float)dat_y * 0.5f);
            }
            else if(walking)
            {
                // slower safer movement
                input_dir = new Vector2((float)dat_x * 0.5f,(float)dat_y * 0.5f);
                if(!client_input_data["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) direction = DAT.VectorToCardinalDir((float)dat_x,(float)dat_y);
            }
            else
            {
                // zoomies as normal
                input_dir = new Vector2((float)dat_x * 1f,(float)dat_y * 1f);
                if(!client_input_data["mod_alt"].AsBool() && (dat_x != 0 || dat_y != 0)) direction = DAT.VectorToCardinalDir((float)dat_x,(float)dat_y);
            }
        }

        public override void Tick(int tick_number)
        {
            // Handle movement
            if(input_dir != Vector2.Zero)
            {
                // Input, as solved by ControlUpdate(); is a SCALED value based on mob speed! Not a 0-1 value!
                GridPos new_pos = GridPos;
                new_pos.hor += input_dir.X;
                new_pos.ver += input_dir.Y;
                AbstractTools.Move(this,new_pos);
                input_dir = Vector2.Zero;
            }
        }
    }
}