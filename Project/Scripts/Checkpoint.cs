using Godot;
using System;

public class Checkpoint : Node2D
{
    private AnimatedSprite sprite;

    public bool IsActive = false;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    void ConnectToLevelHolder(LevelHolder levelHolder)
    {
        Godot.Collections.Array binds = new Godot.Collections.Array();
        binds.Add(this);
        Connect("body_entered", levelHolder, "CheckpointBodyEntered", binds);
    }

    public bool ActivateCheckpoint()
    {
        if (sprite.Animation == "active")
        {
            return false;
        }    
        this.sprite.Play("active");
        IsActive = true;
        return true;
    }

    public void DeactivateCheckpoint()
    {
        IsActive = false;
        this.sprite.Play("inactive");
    }
}
