using Godot;
using System;

public class DebugLevel : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private PackedScene debugControlsScene;
	private DebugControls debugControls;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		debugControlsScene = GD.Load<PackedScene>("res://Scenes/DebugControls.tscn");
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		var justPressed = inputEvent.IsPressed() && !inputEvent.IsEcho();
		if (Input.IsKeyPressed((int)KeyList.F1) && justPressed)
		{
			if (debugControls == null)
			{
				var player = GetNode<Player>("Player");
				debugControls = Constructors.InstanceDebugControls(debugControlsScene, player);
				AddChild(debugControls);
			}
			else
			{
				debugControls.QueueFree();
				debugControls = null;
			}
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
