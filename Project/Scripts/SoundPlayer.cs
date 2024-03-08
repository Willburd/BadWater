using Godot;
using System;

[GlobalClass]
public partial class SoundPlayer : Node3D
{
    [Export]
    public string path;
    [Export]
    public AudioStreamPlayer3D player;

    bool started = false;
    float emergency_ticker = 0.2f;

    public override void _Process(double delta)
    {
        if(!started)
        {
            GD.Print("SOUND PLAY " + path);
            player = new AudioStreamPlayer3D();
            AddChild(player);
            player.Stream = (AudioStream)GD.Load(path);
            player.Play();
            started = true;
        }
        else if(!player.Playing)
        {
            emergency_ticker -= (float)delta;
            if(emergency_ticker < 0) QueueFree();
        }
    }
}
