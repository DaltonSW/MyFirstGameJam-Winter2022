using Godot;
using System;

public class Player : KinematicBody2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

 // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		Console.Write(delta);
		float movementAmount = 5;
		if (Input.IsKeyPressed((int)KeyList.W))
		{
			MoveAndCollide(new Vector2(0, -movementAmount));
		}

		if (Input.IsKeyPressed((int)KeyList.S))
		{
			MoveAndCollide(new Vector2(0, movementAmount));
		}

		if (Input.IsKeyPressed((int)KeyList.D))
		{
			MoveAndCollide(new Vector2(movementAmount, 0));
		}

		if (Input.IsKeyPressed((int)KeyList.A))
		{
			MoveAndCollide(new Vector2(-movementAmount, 0));
		}
	}
}
