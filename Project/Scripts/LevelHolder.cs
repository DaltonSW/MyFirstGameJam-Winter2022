using Godot;
using System;

public class LevelHolder : Node2D
{
	private Player player;
	private AnimatedSprite playerSprite;
	private RespawnScene respawnScene;

    private Level level;

    private Vector2 currentSpawnPoint;


	public override void _Ready()
	{
        level = GetNodeOrNull<Level>("Level");
        if(level != null)
        {
            currentSpawnPoint = level.GetNode<Position2D>("SpawnPoint").GlobalPosition;
        }
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
		player = GetNodeOrNull<Player>("Level/PlayerNode/Player");
		playerSprite = player.GetNode<AnimatedSprite>("AnimatedSprite");
		player.Connect("PlayerKilled", this, "PlayerKilledCallback");
		playerSprite.Connect("animation_finished", this, "AnimationFinishedCallback");
	}

	private void ConnectCheckpoints()
	{
		GetTree().CallGroup("checkpoints", "ConnectToLevelHolder", this);
	}

    private void CheckpointBodyEntered(Node2D body, Checkpoint checkpoint)
    {
        if (body is Player player)
        {
			player.HealPlayer();
			GetTree().CallGroupFlags((int)SceneTree.GroupCallFlags.Realtime, "checkpoints", "DeactivateCheckpoint");
            currentSpawnPoint = checkpoint.GetNode<Position2D>("SpawnPoint").GlobalPosition;
            checkpoint.ActivateCheckpoint();
        }
    }

	private void AnimationFinishedCallback()
	{
		if(playerSprite.Animation == "health_death")
		{
			respawnScene.Visible = true;
			playerSprite.Stop();
		}
	}

	private void InitiateRespawn()
	{
		// Have respawn point
		// Reset player animation and health
		// Reset all enemies positions and healths
		
	}

    public void RespawnPlayer()
    {
		if (player != null)
		{
			player.GlobalPosition = currentSpawnPoint;
            player.velocity = new Vector2(0, 0);
		}
	}
	
	private void PlayerKilledCallback()
	{
	}
}



