using Godot;
using System;

public class Projectile : Area2D
{
	[Export] private int SPEED = 400;
	[Export] private int SPREAD = 5;
	[Export] private int DISTANCE_ALLOWED = 300;
	[Export] private float DISTANCE_TRAVELLED = 0;

	public override void _Ready()
	{
		Random rand = new Random();
		Rotation = (float)(Math.PI * rand.Next(90 - SPREAD, 90 + SPREAD) / 180);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		float amountToMove = SPEED * delta;
		Position -= Transform.y.Normalized() * amountToMove;
		DISTANCE_TRAVELLED += amountToMove;
		if (DISTANCE_TRAVELLED > DISTANCE_ALLOWED)
		{
			QueueFree();
		}
	}
}
