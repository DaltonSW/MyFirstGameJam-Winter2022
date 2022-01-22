using Godot;
using System;

public class EditorObject : Node2D
{
    private bool canPlace = true;
    private bool isPanning = false;
    private Node2D level;
    private Node2D camContainer;
    private Camera2D editorCamera;
    private PackedScene currentItem;

    private Vector2 relativeMousePoint = new Vector2(0, 0);

    private int CAM_SPEED = 5;

    public override void _Ready()
    {
        level = GetNode<Node2D>("/root/LevelEditor/Level");
        camContainer = GetNode<Node2D>("/root/LevelEditor/CamContainer");
        editorCamera = camContainer.GetNode<Camera2D>("Camera");

        editorCamera.Current = true;
    }

    public override void _Process(float delta)
    {
        GlobalPosition = GetGlobalMousePosition();
        if (Input.IsActionJustPressed("mb_left"))
        {
            if (currentItem != null && canPlace)
            {
                Node2D newItem = (Node2D)currentItem.Instance();
                level.AddChild(newItem);
                newItem.GlobalPosition = GetGlobalMousePosition();
            }
        }

        if (Input.IsActionPressed("mb_middle"))
        {
            isPanning = true;
            relativeMousePoint = GetViewport().GetMousePosition();
        }

        else
        {
            isPanning = false;
        }

        moveEditor();

    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.IsPressed())
            {
                if (mouseEvent.ButtonIndex == (int)ButtonList.WheelUp)
                {
                    editorCamera.Zoom -= new Vector2(0.1F, 0.1F);
                }

                if (mouseEvent.ButtonIndex == (int)ButtonList.WheelDown)
                {
                    editorCamera.Zoom += new Vector2(0.1F, 0.1F);
                }
            }
        }

        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            if (isPanning)
            {
                Vector2 referencePoint = GetViewport().GetMousePosition();
                camContainer.GlobalPosition -= new Vector2((referencePoint.x - relativeMousePoint.x), (referencePoint.y - relativeMousePoint.y));
            }
        }
    }

    private void moveEditor()
    {
        if (Input.IsActionPressed("w"))
        {
            camContainer.GlobalPosition = new Vector2(camContainer.GlobalPosition.x, camContainer.GlobalPosition.y - CAM_SPEED);
        }
        
        if (Input.IsActionPressed("a"))
        {
            camContainer.GlobalPosition = new Vector2(camContainer.GlobalPosition.x - CAM_SPEED, camContainer.GlobalPosition.y);
        }
        
        if (Input.IsActionPressed("s"))
        {
            camContainer.GlobalPosition = new Vector2(camContainer.GlobalPosition.x, camContainer.GlobalPosition.y + CAM_SPEED);
        }
        
        if (Input.IsActionPressed("d"))
        {
            camContainer.GlobalPosition = new Vector2(camContainer.GlobalPosition.x + CAM_SPEED, camContainer.GlobalPosition.y);
        }
    }

    

}
