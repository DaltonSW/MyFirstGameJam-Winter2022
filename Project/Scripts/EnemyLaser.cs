using Godot;
using System;

public class EnemyLaser : Area2D
{
	[Export] public float DAMAGE = 7.5F;

	[Export] private int SPEED = 700;
	[Export] private int SPREAD = 0;
	[Export] private int DISTANCE_ALLOWED = 10000;
	[Export] private float DISTANCE_TRAVELLED = 0;

	private static Random RNG = new Random();

	public override void _Ready()
	{
		this.Connect("area_entered", this, "OnCollision");
		this.Connect("body_entered", this, "OnCollision");

		//Rotation = (float)(Math.PI * RNG.Next(90 - SPREAD, 90 + SPREAD) / 180) + (float)Math.PI;
		// Player player = GetNode<Player>("../PlayerNode/Player");
		// if (player.isFacingLeft)
		// {
		// 	Rotation += (float)Math.PI;
		// }
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		float amountToMove = SPEED * delta;
		Position -= Transform.y.Normalized() * amountToMove;
		DISTANCE_TRAVELLED += amountToMove;
		if (DISTANCE_TRAVELLED > DISTANCE_ALLOWED)
		{
			//FreeBullet();
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
			if(with is Player player)
			{
				// Need to implement something that differentiates if a Node has health or not so we don't try and subtract health from something without that property
				// Then it'll just be: with.HEALTH -= this.DAMAGE; if with.HEALTH < 0: with.QueueFree();
				player.HurtPlayer();
				FreeBullet();
			}

			else
			{
				if (!((with is Enemy) || (with is TileMap)))
				{
					FreeBullet();
				}
			}
		}
	}
}
