using Godot;
using System;

public class PlayerCamera : Camera2D
{
	private Player player;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/LevelHolder/Level/PlayerNode/Player");
	}

	public override void _Process(float delta)
	{
		if (player == null)
		{
			loadPlayer();
		}

		if (player != null)
		{
			GlobalPosition = player.GlobalPosition;
		}
	}

	public void SetLimits(int minX, int maxX, int minY, int maxY)
	{
		LimitLeft   = minX;
		LimitRight  = maxX;
		LimitTop    = minY;
		LimitBottom = maxY;
	}

	public void loadPlayer()
	{
		// This needs to be not hard coded eventually
		player = GetNode<Player>("/root/LevelHolder/Level/PlayerNode/Player");
	}

}
