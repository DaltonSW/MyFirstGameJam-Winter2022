using Godot;
using System;

public class LevelEditor : Node2D
{
	private EditorObject editorObject;
	private Level level;
	private bool showingUI = true;

	private PauseMenu pauseMenu;

	public override void _Ready()
	{
		editorObject = GetNode<EditorObject>("EditorObject");
		level = GetNode<Level>("Level");
		pauseMenu = GetNode<PauseMenu>("/root/LevelEditor/UI/PauseMenu");
		pauseMenu.Hide();
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("toggle_editor"))
		{
			Global.isPlaying = !Global.isPlaying;
			editorObject.currentMouseSprite.Texture = null;
			editorObject.swapCameras();
		}

		if (Input.IsActionJustPressed("reset_level"))
		{
			level.respawnPlayer();
		}

		if (Input.IsActionJustPressed("toggle_ui"))
		{
			showingUI = !showingUI;
			if(showingUI)
			{
				GetNode<Control>("/root/LevelEditor/UI/DebugControls").Show();
				GetNode<Label>("/root/LevelEditor/UI/BasicInstructions").Show();
			}

			else
			{
				GetNode<Control>("/root/LevelEditor/UI/DebugControls").Hide();
				GetNode<Label>("/root/LevelEditor/UI/BasicInstructions").Hide();
			}
			
		}

		if (Input.IsActionJustPressed("game_pause"))
		{
			Pause();
		}
	}

	private void _on_FileDialog_confirmed()
	{
		editorObject.currentMouseSprite.Texture = null;
		editorObject.isPlacingPlayer = false;
		editorObject.isPlacingTile = false;

		if (editorObject.fileBoxMode == EditorObject.PopupMode.SAVE)
		{
			editorObject.saveLevel();
		}

		else if (editorObject.fileBoxMode == EditorObject.PopupMode.LOAD)
		{
			editorObject.loadLevel();
		}
	}

	private void _on_FileDialog_hide()
	{
		Global.fileDialogShowing = false;
		editorObject.fileBoxMode = EditorObject.PopupMode.NULL;
	}

	private void Pause()
	{
		GetTree().Paused = true;
		pauseMenu.Show();
	}

	private void Resume()
	{
		GetTree().Paused = false;
		pauseMenu.Hide();
	}
	
	private void _on_PauseMenu_ResumeRequested()
	{
		Resume();
	}
}
