using Godot;
using System;

public class LevelHolder : Node2D
{
	private Player player;
	private AnimatedSprite playerSprite;
	private RespawnScene respawnScene;

	public override void _Ready()
	{
		respawnScene = GetNode<RespawnScene>("RespawnScene");
		Global.isPlaying = true;
		ConnectPlayer();
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if(inputEvent is InputEventKey eventKey)
		{
			if (eventKey.Scancode == (int)KeyList.K && eventKey.Pressed)
			{
				if (player != null)
				{
					player.KillPlayer();
				}
			}
		}
	}

	private void ConnectPlayer()
	{
		player = GetNodeOrNull<Player>("PlayerNode/Player");
		playerSprite = GetNodeOrNull<AnimatedSprite>("PlayerNode/Player/AnimatedSprite");
		player.Connect("PlayerKilled", this, "PlayerKilledCallback");
		playerSprite.Connect("animation_finished", this, "AnimationFinishedCallback");
	}

	private void AnimationFinishedCallback()
	{
		if(playerSprite.Animation == "health_death")
		{
			respawnScene.Visible = true;
			playerSprite.Stop();
			GD.Print("Woooo");
		}
	}
	
	private void PlayerKilledCallback()
	{
		GD.Print("oh no");
	}
}



