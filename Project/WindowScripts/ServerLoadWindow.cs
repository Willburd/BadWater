using Godot;
using System;

[GlobalClass]
public partial class ServerLoadWindow : GameWindows
{
    [Export]
    public RichTextLabel status;

    public override void _Process(double delta)
    {
        if(MainController.controller == null) return; // Not server
        base._Process(delta);
        status.Text = "";
        if(MainController.WorldTicks < 4) 
        {
            status.Text = "Setup...";
            return;
        }
        // Setup server hud
        status.Text += "Network Clients : " + MainController.controller.client_container.GetChildCount() + "\n";
        status.Text += "Network Entities: " + MainController.controller.entity_container.GetChildCount() + "\n";
        for(int i = 0; i < MainController.GetSubControllerCount(); i++) 
		{
            DeligateController con = MainController.GetSubControllerAtIndex(i);
            status.Text += "============" + "\n";
            status.Text += "(" + (con.did_tick ? "X" : "_") + ") : " + con.display_name + "\n";
            status.Text += "Paused   : " + (con.IsPaused ? "YES" : "no") + "\n";
            status.Text += "Tickdelay: " + con.GetTickRate() + "\n";
            status.Text += "Entities : " + con.entities.Count + "\n";
            if(con is MapController mp_con) 
            {
                status.Text += "Effects : " + mp_con.effects.Count + "\n";
                status.Text += "Spawners: " + mp_con.spawners.Count + "\n";
            }
            ulong accume = 0;
            for(int q = 0; q < con.logged_times.Count; q++) 
            {
                accume += con.logged_times[q];
            }
            double avrg = (double)accume / (double)con.logged_times.Count;
            status.Text += "Process time: " + Mathf.Floor(avrg) + "ms \n";
            int ticks_per_second = con.GetTickRate();
            float tick_percent = (float)avrg / (ticks_per_second * 1000f);
            status.Text += "Tick used: " + Mathf.Floor(tick_percent * 100f) + "% \n";
		}
    }
}
