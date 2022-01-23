using Godot;
using System;

public class LevelEditorSaveLoad : FileDialog
{    
    private EditorObject editorObject;

    public override void _Ready()
    {
        editorObject = GetNode<EditorObject>("/root/LevelEditor/EditorObject");
    }

    public override void _Draw()
    {
        //CurrentDir = "user://SavedScenes";
    }

    private void Refresh()
    {
        this._Draw();
    }
}
