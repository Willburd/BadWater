using Godot;
using System;

namespace BehaviorEvents
{
    // Handles construction clicks
    public partial class TurfBasic : Behavior
    {
        public TurfBasic(int set_build_level)
        {
            build_level = set_build_level;
        }

        int build_level = 0;

        public override void MapLoadVars(Godot.Collections.Dictionary data)
        {
            
        }
    }

    // Minable turfs
    public partial class TurfMineable : Behavior
    {
        public TurfMineable()
        {
        }

        public override void MapLoadVars(Godot.Collections.Dictionary data)
        {
            
        }
    }
}