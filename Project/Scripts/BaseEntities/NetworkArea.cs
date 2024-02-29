using Godot;
using System;
using System.Collections.Generic;

public partial class NetworkArea : NetworkEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        template_data = data;
        AreaData temp = template_data as AreaData;
        base_turf_ID = temp.base_turf_ID;
        is_space = temp.is_space;
        always_powered = temp.always_powered;
    }
    
    // Unique data
    [Export]
    public string base_turf_ID;
    [Export]
    public bool always_powered;
    [Export]
    public bool is_space;
    // End of template data
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }


    public List<NetworkTurf> turfs = new List<NetworkTurf>();

    public override void Tick()
    {
        RandomTurfUpdate(); // Randomly update some turfs, some types of turfs do things when random ticked, atmo also likes these.
    }

    public void AddTurf(NetworkTurf turf)
    {
        // Remove from other areas
        if(turf.Area != null)
        {
            turf.Area.turfs.Remove(turf);
        }
        // Make ours!
        turf.Area = this;
        turfs.Add(turf);
    }

    public void RandomTurfUpdate()
    {
        // Lower chance of random ticks heavily 
        if(turfs.Count == 0) return;
        if((Mathf.Abs((int)GD.Randi()) % 100) < 80) return;
        // Perform a random number of random turf updates
        int repeat = Mathf.Clamp(Mathf.Abs((int)GD.Randi()) % Mathf.Max((int)(turfs.Count / 50),1), 1, turfs.Count);
        while(repeat-- > 0)
        {
            int check = Mathf.Abs((int)GD.Randi()) % turfs.Count;
            NetworkTurf turf = turfs[check];
            turf.RandomTick();
            turf.AtmosphericsCheck();
        }
    }
}