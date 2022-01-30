using Godot;
using System;

public class Credits : Node2D
{
    public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton mouseButton)
        {
            GetTree().Quit();
        }
	}
}
