using Godot;
using System;

public class LevelEditor : Node2D
{
	private EditorObject editorObject;
    private Level level;
	//TODO: Not relevant to this file, but need camera to follow player properly

	public override void _Ready()
	{
		editorObject = GetNode<EditorObject>("EditorObject");
        level = GetNode<Level>("Level");
	}

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("toggle_editor"))
        {
            Global.isPlaying = !Global.isPlaying;
            editorObject.currentMouseSprite.Texture = null;
            editorObject.swapCameras();
            GD.Print("Toggle!");
        }

        if (Input.IsActionJustPressed("reset_level"))
        {
            level.respawnPlayer();
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
}
