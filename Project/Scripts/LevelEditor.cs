using Godot;
using System;

public class LevelEditor : Node2D
{
	private EditorObject editorObject;
	//TODO: Something to handle the E key switching between playing and editing
	//TODO: Not relevant to this file, but need camera to follow player properly

	public override void _Ready()
	{
		editorObject = GetNode<EditorObject>("EditorObject");
	}

	private void _on_FileDialog_confirmed()
	{
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
