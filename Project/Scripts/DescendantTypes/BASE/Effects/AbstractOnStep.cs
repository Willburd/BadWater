using Godot;
using System;

namespace Behaviors_BASE
{
    public partial class AbstractOnStep : AbstractEffect
    {
        public AbstractOnStep()
        {
            entity_type = MainController.DataType.Effect;
        }
        
        private string teleport_tag = "";

        public override void MapLoadVars(Godot.Collections.Dictionary data)
        {
            teleport_tag = JsonHandler.ApplyExistingTag(data,"teleport_tag",teleport_tag);
        }

        public override void Crossed(AbstractEntity crosser)
        {
            
        }
    }
}