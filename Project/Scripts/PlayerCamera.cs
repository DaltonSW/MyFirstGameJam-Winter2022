using Godot;
using System;

public class PlayerCamera : Camera2D
{
    private Player player;

    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        if (player != null)
        {
            GlobalPosition = player.GlobalPosition;
        }
    }

    public void loadPlayer()
    {
        // This needs to be not hard coded eventually
        player = GetNode<Player>("/root/LevelEditor/Level/PlayerNode/Player");
    }

}
