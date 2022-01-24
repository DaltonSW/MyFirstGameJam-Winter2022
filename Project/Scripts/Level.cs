using Godot;
using System;

public class Level : Node2D
{
	public Position2D spawnPoint;
	public Player player;

	public override void _Ready()
	{
		spawnPoint = GetNode<Position2D>("SpawnPoint");
	}

	public void respawnPlayer()
	{
		player = GetNodeOrNull<Player>("PlayerNode/Player");
		if (player != null)
		{
			player.GlobalPosition = spawnPoint.GlobalPosition;
		}
	}
}
