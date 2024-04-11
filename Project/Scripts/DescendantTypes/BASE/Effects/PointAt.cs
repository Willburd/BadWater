using Godot;
using System;
using System.Threading.Tasks;

namespace Behaviors
{
    public partial class PointAt : AbstractEffect
    {
        public PointAt()
        {
            entity_type = MainController.DataType.Effect;
        }
        
        int lifetime = 3; // seconds
        public override void Init()
        {
            Task.Delay(new TimeSpan(0, 0, lifetime)).ContinueWith(o => 
            { 
                AbstractTools.DeleteEntity(this);
            });
        }
    }
}
