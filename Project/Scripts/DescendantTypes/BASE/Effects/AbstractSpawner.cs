using Godot;
using System;

namespace Behaviors
{
    public partial class AbstractSpawner : AbstractEffect
    {
        public AbstractSpawner()
        {
            entity_type = MainController.DataType.Effect;
        }

        public override void MapLoadVars(Godot.Collections.Dictionary data)
        {

        }

        public override void Crossed(AbstractEntity crosser)
        {
            
        }
    }
}