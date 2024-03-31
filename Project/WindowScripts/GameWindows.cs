using Godot;
using System;

[GlobalClass]
public partial class GameWindows : Window
{
    [Export]
    public bool lock_aspect;
    private float laspect = 1f;

    public override void _Ready()
    {
        SizeChanged += OnResize;
        laspect = (float)Size.Y / (float)Size.X;
    }

    public void OnResize()
    {
        // Lock to max screen size
        if(!lock_aspect) return;
        Size = new Vector2I(Size.X, (int)(Size.X * laspect));
    }

    public override void _Process(double delta)
    {
        int bound_X = 10;
        int bound_Y = 30;
        int clamped_X = Math.Clamp(Position.X,bound_X,Math.Max(bound_X+1,(int)DisplayServer.WindowGetSize().X - ((int)GetViewport().GetVisibleRect().Size.X + 15)));
        int clamped_Y = Math.Clamp(Position.Y,bound_Y,Math.Max(bound_Y+1,(int)DisplayServer.WindowGetSize().Y - 10));
        Position = new Vector2I( clamped_X, clamped_Y);
    }
}
