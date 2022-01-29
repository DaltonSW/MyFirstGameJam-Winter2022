using Godot;
using System;

public class Checkpoint : Node2D
{
    void ConnectToLevelHolder(LevelHolder levelHolder)
    {
        GD.Print("Connecting!");
        Godot.Collections.Array binds = new Godot.Collections.Array();
        binds.Add(this);
        Connect("body_entered", levelHolder, "CheckpointBodyEntered", binds);
    }
}
