using Godot;
using System;

[GlobalClass]
public partial class SoundPlayer : Node3D
{
    [Export]
    public string path;
    [Export]
    public AudioStreamPlayer player_global;
    public AudioStreamPlayer3D player_pos;

    bool started = false;
    float emergency_ticker = 0.2f;

    public override void _Process(double delta)
    {
        if(!started)
        {
            if(player_global != null)
            {
                player_global.Stream = (AudioStream)GD.Load(path);
                player_global.Play();
                started = true;
            }
            else
            {
                player_pos.Stream = (AudioStream)GD.Load(path);
                player_pos.Play();
                started = true;
            }
        }
        else 
        {
            if(player_global != null)
            {
                if(!player_global.Playing)
                {
                    emergency_ticker -= (float)delta;
                    if(emergency_ticker < 0) Free();
                }
            }
            else
            {
                if(!player_pos.Playing)
                {
                    emergency_ticker -= (float)delta;
                    if(emergency_ticker < 0) Free();
                }
            }
        }
    }
}
