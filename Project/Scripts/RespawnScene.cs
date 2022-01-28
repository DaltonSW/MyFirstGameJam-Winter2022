using Godot;
using System;

public class RespawnScene : Node2D
{
	public override void _Ready()
	{
		Global.isPlaying = false;
	}
}
