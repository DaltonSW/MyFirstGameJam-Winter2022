using Godot;
using System;

public class Projectile : Area2D
{
	[Export] private int SPEED = 400;
	[Export] private int SPREAD = 5;
	[Export] private int DISTANCE_ALLOWED = 300;
	[Export] private float DISTANCE_TRAVELLED = 0;

	private static Random RNG = new Random();

	public override void _Ready()
	{
		this.Connect("area_entered", this, "OnCollision");
		this.Connect("body_entered", this, "OnCollision");

		Rotation = (float)(Math.PI * RNG.Next(90 - SPREAD, 90 + SPREAD) / 180);
		Player player = GetNode<Player>("../Node2D/Player");
		if (player.isFacingLeft)
		{
			Rotation += (float)Math.PI;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		float amountToMove = SPEED * delta;
		Position -= Transform.y.Normalized() * amountToMove;
		DISTANCE_TRAVELLED += amountToMove;
		if (DISTANCE_TRAVELLED > DISTANCE_ALLOWED)
		{
			FreeBullet();
		}
	}

	private void FreeBullet()
	{
		this.QueueFree();
	}

	private void OnCollision(Node with)
	{
		if(with.Filename != this.Filename)
		{
			if(!with.IsClass("TileMap"))
			{
				with.QueueFree();
				FreeBullet();
			}

			else
			{
				FreeBullet();
			}
		}
	}
}
