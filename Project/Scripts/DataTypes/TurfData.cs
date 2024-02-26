using Godot;
using System;

[GlobalClass] 
public partial class TurfData : Resource
{
    [Export]
    string name = "Turf";

    [Export]
    bool density = false;

    [Export]
    bool opaque = false;
}
