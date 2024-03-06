using Godot;
using System;

namespace BehaviorEvents
{
    // Handles construction clicks
    public partial class OnStepped : Behavior
    {
        public OnStepped()
        {

        }
        
        private string teleport_tag = "";

        public override void MapLoadVars(Godot.Collections.Dictionary data)
        {
            teleport_tag = TOOLS.ApplyExistingTag(data,"teleport_pos",teleport_tag);
        }

        public override void Crossed(AbstractEntity owner, MainController.DataType entity_type, AbstractEntity crosser)
        {
            
        }
    }
}