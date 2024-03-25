using Godot;
using System;

[GlobalClass]
public partial class GetSyncText : Label3D
{
    [Export]
    public MeshUpdater mesh_handler;
    [Export]
    public bool auto_fadeout = false;
    [Export]
    public float fade_counter = 1f;

    public override void _Process(double delta)
    {
        if(Text == "") Text = mesh_handler.GetDisplayText;
        if(auto_fadeout)
        {
            fade_counter -= (float)delta / 2f;
            Transparency = 1f - Mathf.Clamp(fade_counter,0f,1f);
        }
    }
}
