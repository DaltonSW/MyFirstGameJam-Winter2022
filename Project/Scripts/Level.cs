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

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		// if(inputEvent is InputEventKey eventKey)
		// {
		// 	if(eventKey.Scancode == (int)KeyList.K && eventKey.Pressed)
		// 	{
		// 		player = GetNodeOrNull<Player>("PlayerNode/Player");
		// 		if (player != null)
		// 		{
		// 			player.KillPlayer();
		// 		}
		// 	}
		// }
	}

	private void _on_DeathBox_body_entered(Node2D body)
	{
		GetParent<LevelHolder>().RespawnPlayer();
	}
}
