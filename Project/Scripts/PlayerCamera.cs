using Godot;
using System;

public class PlayerCamera : Camera2D
{
	private Player player;

	private const int BLOCK_WIDTH = 32;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/LevelEditor/Level/PlayerNode/Player");
	}

	public override void _Process(float delta)
	{
		if (player != null)
		{
			float windowWidth = GetViewportRect().Size.x;
			float windowHeight = GetViewportRect().Size.y;
			float centerXOffset = windowWidth / 2;
			float centerYOffset = windowHeight / 2;
			float minX = 0                + centerXOffset;
			float maxX = 26 * BLOCK_WIDTH - centerXOffset;
			float minY = 0                + centerYOffset;
			float maxY = 26 * BLOCK_WIDTH - centerYOffset;
			GlobalPosition = player.GlobalPosition;
		}
	}

	public void loadPlayer()
	{
		// This needs to be not hard coded eventually
		player = GetNode<Player>("/root/LevelEditor/Level/PlayerNode/Player");
	}

}
