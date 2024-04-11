using Godot;
using System;
using System.Threading.Tasks;


namespace Behaviors
{
    public partial class RuneText : AbstractEffect
    {
        public RuneText()
        {
            entity_type = MainController.DataType.Effect;
        }

        public RuneText(string message)
        {
            runemessage = message;
            if(runemessage.Length > 200)
            {
                runemessage = runemessage.Substr(0,100);
                runemessage += "...";
            }
        }
        int lifetime = 4; // seconds
        private string runemessage = "";
        public override void Init()
        {
            Task.Delay(new TimeSpan(0, 0, lifetime)).ContinueWith(o => 
            { 
                AbstractTools.DeleteEntity(this);
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
