using Godot;
using System;

namespace BehaviorEvents
{
    // Handles construction clicks
    public partial class OnStepped : Behavior
    {
        private string teleport_tag = "";

        public OnStepped()
        {
        }

        public override void MapLoadVars(Godot.Collections.Dictionary data)
        {
            teleport_tag = TOOLS.ApplyExistingTag(data,"teleport_pos",teleport_tag);
        }


        protected override void Abstract_Crossed(MainController.DataType entity_type, AbstractEntity crosser)
        {
            // Handle abstracts
            // TODO - ...

            // Perform generic stuff
            Generic_Crossed(entity_type);
        }
        protected override void Entity_Crossed(MainController.DataType entity_type, NetworkEntity crosser)
        {
            // Handle entities
            // TODO - ...

            // Perform generic stuff
            Generic_Crossed(entity_type);
        }
        protected override void Generic_Crossed(MainController.DataType entity_type)
        {

        }
    }
}