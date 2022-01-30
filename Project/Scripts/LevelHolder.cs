using Godot;
using System;

public class LevelHolder : Node2D
{
	private Player player;
	private AnimatedSprite playerSprite;
	private RespawnScene respawnScene;

    private Level level;

    private Vector2 currentSpawnPoint;

	private Camera2D camera;

	private bool isRespawning = false;
	private float RESPAWN_LENGTH = 2;
	private float CURRENT_RESPAWN_COUNT = 0;

	private AudioStreamSample currentSong;
	private AudioStreamSample forestSong;
	private AudioStreamSample caveSong;
	private AudioStreamSample treeSong;
	private AudioStreamSample titleSong;
	private AudioStreamSample creditsSong;


	public override void _Ready()
	{
        level = GetNodeOrNull<Level>("Level");
        if(level != null)
        {
            currentSpawnPoint = level.GetNode<Position2D>("SpawnPoint").GlobalPosition;
        }
		respawnScene = GetNode<RespawnScene>("RespawnScene");
		camera = GetNode<Camera2D>("PlayerCamera");
		Global.isPlaying = true;
		ConnectPlayer();
        ConnectCheckpoints();

		// currentSong;
		// forestSong = GD.Load<AudioStreamSample>("res://Sounds/SONG_TITLE.wav");
		// caveSong = GD.Load<AudioStreamSample>("res://Sounds/SONG_TITLE.wav");
		// treeSong = GD.Load<AudioStreamSample>("res://Sounds/SONG_TITLE.wav");
	}

	public override void _Process(float delta)
	{
		if (isRespawning)
		{
			CURRENT_RESPAWN_COUNT += delta;
			if (CURRENT_RESPAWN_COUNT > RESPAWN_LENGTH)
			{
				CURRENT_RESPAWN_COUNT = 0;
				isRespawning = false;
				GetTree().Paused = false;
				respawnScene.Visible = false;
			}
		}
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
		if (playerSprite.Animation == "health_death")
		{
			respawnScene.Visible = true;
			InitiateRespawn();
			playerSprite.Stop();
		}

		else if (playerSprite.Animation == "melee")
		{
			player.StopSwing();
		}
	}

	private void InitiateRespawn()
	{
		player.ResetPlayer();
		isRespawning = true;
		respawnScene.GlobalPosition = currentSpawnPoint;
		camera.GlobalPosition = currentSpawnPoint;
		player.GlobalPosition = currentSpawnPoint;
		GD.Print(player.CURRENT_HEALTH);
		GD.Print(playerSprite.Animation);
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

	public void DeathboxBodyEntered(Node body)
	{
		if(body is Player player)
		{
			player.KillPlayer();
		}
	}
}



