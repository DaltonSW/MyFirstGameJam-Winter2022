using Godot;
using System;

public class LevelHolder : Node2D
{
	private Player player;
	private AnimatedSprite playerSprite;
	private RespawnScene respawnScene;

    private Vector2 currentSpawnPoint;


	public override void _Ready()
	{
		respawnScene = GetNode<RespawnScene>("RespawnScene");
		Global.isPlaying = true;
		ConnectPlayer();
        ConnectCheckpoints();
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

            if(eventKey.Scancode == (int)KeyList.R && eventKey.Pressed)
			{
				RespawnPlayer();
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

	private void ConnectCheckpoints()
	{
        GD.Print("Help");
		GetTree().CallGroup("checkpoints", "ConnectToLevelHolder", this);
	}

    private void CheckpointBodyEntered(Node2D body, Checkpoint checkpoint)
    {
        if (body is Player player)
        {
            currentSpawnPoint = checkpoint.GetNode<Position2D>("SpawnPoint").GlobalPosition;
            checkpoint.GetNode<AnimatedSprite>("AnimatedSprite").Play("active");
        }
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

	private void InitiateRespawn()
	{
		// Have respawn point
		// Reset player animation and health
		// Reset all enemies positions and healths
		
	}

    private void RespawnPlayer()
    {
		player = GetNodeOrNull<Player>("PlayerNode/Player");
		if (player != null)
		{
			player.GlobalPosition = currentSpawnPoint;
            player.velocity = new Vector2(0, 0);
		}
	}
	
	private void PlayerKilledCallback()
	{
		GD.Print("oh no");
	}
}



