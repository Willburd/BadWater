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
        // Time it took to solve server tick
        ulong peak_serv = MainController.controller.logged_times.GetPeak();
        double avrg_serv = MainController.controller.logged_times.GetAverage();
        status.Text += "Tick time: " + Mathf.Round(avrg_serv) + "ms / " + (1000f * (1f / MainController.tick_rate)) + "ms (peak: " + Mathf.Round(peak_serv) + "ms)\n";
        float tick_serv_percent = (float)avrg_serv / (1000f * (1f / MainController.tick_rate));
        status.Text += "Tick used: " + Mathf.Round(tick_serv_percent) + "% \n";
        // Time between ticks
        double peak_ticker = MainController.controller.tick_gap_times.GetPeak();
        double avrg_ticker = MainController.controller.tick_gap_times.GetAverage();
        status.Text += "Tick time: " + Mathf.Floor(avrg_ticker) + "ms ) (peak: " + Mathf.Round(peak_ticker) + "ms)\n";
        // Stats
        status.Text += "============" + "\n";
        status.Text += "Network Clients : " + MainController.controller.client_container.GetChildCount() + "\n";
        status.Text += "Network Entities: " + MainController.controller.entity_container.GetChildCount() + "\n";
        // Sub controller info
        for(int i = 0; i < MainController.GetSubControllerCount(); i++) 
		{
            DeligateController con = MainController.GetSubControllerAtIndex(i);
            status.Text += "============" + "\n";
            status.Text += "(" + (con.did_tick ? "X" : "_") + ") : " + con.display_name + "\n";
            status.Text += "Paused   : " + (con.IsPaused ? "YES" : "no") + "\n";
            status.Text += "Tickdelay: " + con.GetTickRate() + "\n";
            if(con is MobController mb_con) 
            {
                status.Text += "Ghost   : " + mb_con.ghost_entities.Count + "\n";
                status.Text += "Living  : " + mb_con.living_entities.Count + "\n";
                status.Text += "Dead    : " + mb_con.dead_entities.Count + "\n";
            }
            if(con is MachineController ma_con) 
            {
                status.Text += "Ghost   : " + ma_con.entities.Count + "\n";
            }
            if(con is MapController mp_con) 
            {
                status.Text += "Effects : " + mp_con.effects.Count + "\n";
                status.Text += "Spawners: " + mp_con.spawners.Count + "\n";
            }
            ulong peak = con.logged_times.GetPeak();
            double avrg = con.logged_times.GetAverage();
            status.Text += "Process time: " + Mathf.Round(avrg) + "ms / " + Mathf.Round(1000f * (con.GetTickRate() * (1f / MainController.tick_rate))) + "ms (peak: " + Mathf.Round(peak) + "ms)\n";
            status.Text += "Tick used: " + Mathf.Round(Mathf.Round(avrg) / Mathf.Round(1000f * (con.GetTickRate() * (1f / MainController.tick_rate))) * 100f) + "% \n";
		}
    }
}
