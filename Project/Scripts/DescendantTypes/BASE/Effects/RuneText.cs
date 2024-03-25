using Godot;
using System;
using System.Threading.Tasks;


namespace Behaviors_BASE
{
    public partial class RuneText : AbstractEffect
    {
        public RuneText(string message)
        {
            runemessage = message;
        }
        int lifetime = 4; // seconds
        private string runemessage = "";
        public override void Init()
        {
            Task.Delay(new TimeSpan(0, 0, lifetime)).ContinueWith(o => 
            { 
                DeleteEntity();
            });
        }
        public override void UpdateCustomNetworkData()
        {
            if(LoadedNetworkEntity is NetworkEffect network_effect)
            {
                network_effect.synced_text = runemessage;
            }
        }
    }
}
