using Godot;
using System;

public class LevelHolder : Node2D
{
	private Player player;
	private AnimatedSprite playerSprite;
	private RespawnScene respawnScene;
	private PauseMenu pauseMenu;

	private Level level;

	private Camera2D camera;
	private Camera2D respawnCamera;

	private bool isRespawning = false;
	private float RESPAWN_LENGTH = 2;
	private float CURRENT_RESPAWN_COUNT = 0;

	private float DEATHBOX_LOCKOUT = 4;
	private float CURRENT_DEATHBOX_TICKS = 0;
	private bool deathboxActive = true;

	#region Spawn Points
	private Vector2 currentSpawnPoint;
	private Vector2 forestToCavePoint;
	private Vector2 caveToForestPoint;
	private Vector2 forestToTreePoint;
	#endregion

	#region Sound Properties
	private AudioStreamPlayer musicPlayer;
	private AudioStreamPlayer extraPlayer;

	private AudioStreamSample currentSong;
	private AudioStreamSample forestSong;
	private AudioStreamSample caveSong;
	private AudioStreamSample treeSong;
	private AudioStreamSample pauseSong;

	private AudioStreamSample pauseSound;
	private AudioStreamSample checkpointSound;
	#endregion

	public override void _Ready()
	{
		LoadSounds();
		level = GetNodeOrNull<Level>("Level");
		if(level != null)
		{
			currentSpawnPoint = level.GetNode<Position2D>("SpawnPoint").GlobalPosition;
		}
		respawnScene = GetNode<RespawnScene>("RespawnScene");
		camera = GetNode<Camera2D>("PlayerCamera");
		respawnCamera = GetNode<Camera2D>("RespawnCamera");
		pauseMenu = GetNode<PauseMenu>("UI/PauseMenu");
		Global.isPlaying = true;
		ConnectPlayer();
		ConnectCheckpoints();
		SetCameraLimitsToForest();

		forestToCavePoint = GetNode<Position2D>("ForestToCavePoint").GlobalPosition;
		caveToForestPoint = GetNode<Position2D>("CaveToForestPoint").GlobalPosition;
		forestToTreePoint = GetNode<Position2D>("ForestToTreePoint").GlobalPosition;
	}

	public override void _Process(float delta)
	{
		if (isRespawning)
		{
			CURRENT_RESPAWN_COUNT += delta;
			if (CURRENT_RESPAWN_COUNT > RESPAWN_LENGTH)
			{
				CURRENT_RESPAWN_COUNT = 0;
				player.ResetPlayer();
				isRespawning = false;
				GetTree().Paused = false;
				camera.Current = true;
				respawnCamera.Current = false;
			}
		}

		if (CURRENT_DEATHBOX_TICKS > 0)
		{
			CURRENT_DEATHBOX_TICKS += delta;
			if (CURRENT_DEATHBOX_TICKS > DEATHBOX_LOCKOUT)
			{
				deathboxActive = true;
				CURRENT_DEATHBOX_TICKS = 0;
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

		if (Input.IsActionJustPressed("game_pause"))
		{
			Pause();
		}
	}

	private void SetCameraLimits(int minX, int maxX, int minY, int maxY)
	{
		GetNode<PlayerCamera>("PlayerCamera").SetLimits(minX, maxX, minY, maxY);
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
			if (checkpoint.ActivateCheckpoint())
			{
				extraPlayer.Stream = checkpointSound;
				extraPlayer.Play();
			}
		}
	}

	private void AnimationFinishedCallback()
	{
		if (playerSprite.Animation == "health_death" || playerSprite.Animation == "fall_death")
		{
			camera.Current = false;
			respawnCamera.Current = true;
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
		isRespawning = true;
		CURRENT_DEATHBOX_TICKS = 1;
		//respawnScene.GlobalPosition = currentSpawnPoint;
		camera.GlobalPosition = currentSpawnPoint;
		player.GlobalPosition = currentSpawnPoint;
		GetTree().CallGroupFlags((int)SceneTree.GroupCallFlags.Realtime, "enemies", "ResetEnemy");
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
		respawnScene.Visible = true;
		InitiateRespawn();
	}

	public void DeathboxBodyEntered(Node body)
	{
		if(body is Player player && deathboxActive)
		{
			deathboxActive = false;
			player.FallAndDie();
		}
	}
	
	private void Pause()
	{
		extraPlayer.Stream = pauseSound;
		extraPlayer.Play();
		musicPlayer.Stream = pauseSong;
		musicPlayer.Play();
		GetTree().Paused = true;
		pauseMenu.Show();
	}

	private void Resume()
	{
		musicPlayer.Stream = currentSong;
		musicPlayer.Play();
		GetTree().Paused = false;
		pauseMenu.Hide();
	}
	
	private void _on_PauseMenu_ResumeRequested()
	{
		Resume();
	}

	private void LoadSounds()
	{
		musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
		extraPlayer = GetNode<AudioStreamPlayer>("ExtraPlayer");
		extraPlayer.VolumeDb = -10;

		// forestSong = GD.Load<AudioStreamSample>("res://Sounds/forest.wav");
		// caveSong = GD.Load<AudioStreamSample>("res://Sounds/cave.wav");
		// treeSong = GD.Load<AudioStreamSample>("res://Sounds/tree.wav");

		pauseSound = GD.Load<AudioStreamSample>("res://Sounds/SFX/pause.wav");
		checkpointSound = GD.Load<AudioStreamSample>("res://Sounds/SFX/checkpoint.wav");
	}

	public void ForestToCaveCallback(Node with)
	{
		if (!(with is Player))
		{
			return;
		}
		SetCameraLimits(-3392, 2271, 1153, 3968);
		player.GlobalPosition = forestToCavePoint;
		currentSpawnPoint = forestToCavePoint;
		player.velocity = new Vector2(0, 0);
		GetTree().CallGroupFlags((int)SceneTree.GroupCallFlags.Realtime, "enemies", "ResetEnemy");
		currentSong = caveSong;
		musicPlayer.Stream = currentSong;
		musicPlayer.Play();
	}

	public void CaveToForestCallback(Node with)
	{
		if (!(with is Player))
		{
			return;
		}
		SetCameraLimitsToForest();
		player.GlobalPosition = caveToForestPoint;
		currentSpawnPoint = caveToForestPoint;
		player.velocity = new Vector2(0, 0);
		GetTree().CallGroupFlags((int)SceneTree.GroupCallFlags.Realtime, "enemies", "ResetEnemy");
		currentSong = forestSong;
		musicPlayer.Stream = currentSong;
		musicPlayer.Play();
	}

	public void ForestToTreeCallback(Node with)
	{
		if (!(with is Player))
		{
			return;
		}
		SetCameraLimits(2528, 6816, 1792, 4032);
		player.GlobalPosition = forestToTreePoint;
		currentSpawnPoint = forestToTreePoint;
		player.velocity = new Vector2(0, 0);
		GetTree().CallGroupFlags((int)SceneTree.GroupCallFlags.Realtime, "enemies", "ResetEnemy");
		currentSong = treeSong;
		musicPlayer.Stream = currentSong;
		musicPlayer.Play();
	}	
	
	private void SetCameraLimitsToForest()
	{
		SetCameraLimits(0, 5583, -1643, 831);
	}
}





