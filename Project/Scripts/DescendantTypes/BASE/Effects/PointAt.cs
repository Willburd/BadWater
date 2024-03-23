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
            GD.Print("INIT");
            Task.Delay(new TimeSpan(0, 0, lifetime)).ContinueWith(o => 
            { 
                GD.Print("END");
                DeleteEntity();
            });
        }
    }
}
