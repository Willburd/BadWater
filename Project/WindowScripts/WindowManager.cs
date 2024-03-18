using Godot;
using System;

[GlobalClass]
public partial class WindowManager : Node
{
    [Export]
    public Window main_window;
    [Export]
    public JoinWindow join_window;
}
