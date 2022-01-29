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
        GD.Print("Connecting!");
        Godot.Collections.Array binds = new Godot.Collections.Array();
        binds.Add(this);
        Connect("body_entered", levelHolder, "CheckpointBodyEntered", binds);
    }

    public void ActivateCheckpoint()
    {
        GD.Print("Activating!");
        this.sprite.Play("active");
    }

    public void DeactivateCheckpoint()
    {
        GD.Print("Deactivating!");
        this.sprite.Play("inactive");
    }
}
