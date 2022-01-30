using Godot;
using System;

public class EnemyLaser : Area2D
{
	[Export] public int SPEED = 700;
	[Export] private int SPREAD = 0;
	[Export] public int DISTANCE_ALLOWED = 800;
	[Export] private float DISTANCE_TRAVELLED = 0;

	private static Random RNG = new Random();

	public override void _Ready()
	{
		this.Connect("area_entered", this, "OnCollision");
		this.Connect("body_entered", this, "OnCollision");
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
			if(with is Player player)
			{
				player.HurtPlayer();
				FreeBullet();
			}

			else
			{
				if (!((with is Enemy) || (with is TileMap) || (with is GroundedMiniboss)))
				{
					FreeBullet();
				}
			}
		}
	}
}
