using Godot;
using System;

public class Checkpoint : Node2D
{
    private AnimatedSprite sprite;

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

    public void ActivateCheckpoint()
    {
        this.sprite.Play("active");
    }

    public void DeactivateCheckpoint()
    {
        this.sprite.Play("inactive");
    }
}
