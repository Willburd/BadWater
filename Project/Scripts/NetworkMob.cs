using Godot;
using System;
using System.Collections.Generic;

[GlobalClass] 
public partial class NetworkMob : NetworkEntity
{
    int health = 100;
    int hunger = 0;
}
