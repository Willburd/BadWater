using Godot;
using System;
using System.Collections.Generic;

public partial class NetworkArea : NetworkEntity
{
    private AreaData data;

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