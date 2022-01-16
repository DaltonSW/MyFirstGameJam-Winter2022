using Godot;
using System;

public class Projectile : Area2D
{
	private int SPEED = 200;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		Position -= Transform.y.Normalized() * SPEED * delta;
	}
}
