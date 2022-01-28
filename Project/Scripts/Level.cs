using Godot;
using System;

public class Level : Node2D
{
	public Position2D spawnPoint;
	public Player player;

	public override void _Ready()
	{
		spawnPoint = GetNode<Position2D>("SpawnPoint");
		Global.isPlaying = true;
		Global.inLevelEditor = false;
	}

	public void respawnPlayer()
	{
		player = GetNodeOrNull<Player>("PlayerNode/Player");
		if (player != null)
		{
			player.GlobalPosition = spawnPoint.GlobalPosition;
		}
	}

	private void _on_DeathBox_body_entered(Node2D body)
	{
		GD.Print("Dead!!!!");
		GD.Print(body.Name);
		respawnPlayer();
	}
}
