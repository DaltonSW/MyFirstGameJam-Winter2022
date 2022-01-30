using Godot;
using System;

public class ShotgunPellet : Area2D
{
    [Export] public float DAMAGE = 7.5F;

	[Export] private int SPEED = 700;
	[Export] private int SPREAD = 5;
	[Export] private int DISTANCE_ALLOWED = 200;
	[Export] private float DISTANCE_TRAVELLED = 0;

	private static Random RNG = new Random();

	public override void _Ready()
	{
		this.Connect("area_entered", this, "OnCollision");
		this.Connect("body_entered", this, "OnCollision");

		Rotation = (float)(Math.PI * RNG.Next(90 - SPREAD, 90 + SPREAD) / 180);
		Player player = GetNode<Player>("../PlayerNode/Player");
		if (player.isFacingLeft)
		{
			Rotation += (float)Math.PI;
		}
	}

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
			if (!(with is TileMap || with is Player))
			{
				FreeBullet();
				if (with is Enemy enemy)
				{
					enemy.HurtEnemy();
				}

				if (with is FlyingEnemy flyingEnemy)
				{
					flyingEnemy.HurtEnemy();
				}

				if (with is GroundedMiniboss groundedMiniboss)
				{
					groundedMiniboss.HurtEnemy();
				}
			}
		}
	}
}
