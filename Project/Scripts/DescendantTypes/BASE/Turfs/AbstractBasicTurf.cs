using Godot;
using System;

namespace Behaviors_BASE
{
    public partial class AbstractBasicTurf : AbstractTurf
    {
        public AbstractBasicTurf(int set_build_level)
        {
            build_level = set_build_level;
        }

        int build_level = 0;

        public override void MapLoadVars(Godot.Collections.Dictionary data)
        {
            
        }
    }
}