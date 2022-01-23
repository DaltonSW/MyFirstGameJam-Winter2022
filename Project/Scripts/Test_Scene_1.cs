using Godot;
using System;

public class Test_Scene_1 : Node2D
{
	private int DEBUG_CLICKS_NEEDED = 3;
	private int DEBUG_CLICKS_INPUT = 0;

	private void _on_Player_input_event(Viewport viewport, InputEvent inputEvent, int shape_idx)
	{
		if (inputEvent is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == (int)ButtonList.Left && mouseButton.Pressed)
			{
				DEBUG_CLICKS_INPUT++;
				if(DEBUG_CLICKS_INPUT == DEBUG_CLICKS_NEEDED)
				{
					GetTree().ChangeScene("res://Scenes/DebugLevel.tscn");
				}
			}
		}
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if(inputEvent is InputEventKey eventKey)
		{
			if(eventKey.Scancode == (int)KeyList.R && eventKey.Pressed)
			{
				Player player = GetNode<Player>("Node2D/Player");
				Position2D spawnPoint = GetNode<Position2D>("SpawnPoint");
				player.Position = spawnPoint.Position;
				player.velocity = new Vector2(0, 0);
			}
		}
	}
}





