using Godot;
using System;

namespace Behaviors
{
    public partial class AbstractMineableTurf : AbstractTurf
    {
        public AbstractMineableTurf()
        {
            entity_type = MainController.DataType.Turf;
        }

        public override void MapLoadVars(Godot.Collections.Dictionary data)
        {
            
        }
    }
}