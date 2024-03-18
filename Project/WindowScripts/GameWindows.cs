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
    }

    public void OnResize()
    {
        // Lock to max screen size
        if(!lock_aspect) return;
        Size = new Vector2I(Size.X, (int)(Size.X * laspect));
    }

    public override void _Process(double delta)
    {
        Position = new Vector2I( Math.Clamp(Position.X,10,(int)DisplayServer.WindowGetSize().X - ((int)GetViewport().GetVisibleRect().Size.X + 15)), Math.Clamp(Position.Y,30,(int)DisplayServer.WindowGetSize().Y - 10));
    }
}
