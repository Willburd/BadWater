using Godot;
using System;
using System.Threading.Tasks;

namespace Behaviors_BASE
{
    public partial class PointAt : AbstractEffect
    {
        int lifetime = 3; // seconds
        public override void Init()
        {
            Task.Delay(new TimeSpan(0, 0, lifetime)).ContinueWith(o => 
            { 
                DeleteEntity();
            });
        }
    }
}
