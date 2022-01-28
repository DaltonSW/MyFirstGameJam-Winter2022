using Godot;
using System;

public class PauseMenu : Control
{
	[Signal]
	delegate void ResumeRequested();

	public override void _Ready()
	{
		PauseMode = PauseModeEnum.Process; // Allow this node to run when paused
	}

	private void _on_ResumeGameButton_pressed()
	{
		EmitSignal(nameof(ResumeRequested));
	}

	private void _on_ExitToDesktopButton_pressed()
	{
		GetTree().Quit(0); // Exit game
	}
	
	private void _on_ExitToMainMenuButton_pressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeScene("res://Scenes/MainMenu.tscn");
	}

}



