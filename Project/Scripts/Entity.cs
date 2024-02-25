using Godot;
using System;

public partial class Entity : Node3D
{
	[Export]
	public int id = -1;
	public WorldPos pos;

	public virtual void MapInit(WorldPos pos)
	{

	}

	public virtual void Spawn(WorldPos pos)
	{
		
	}
	
	public virtual void Destroy()
	{
		
	}
}
