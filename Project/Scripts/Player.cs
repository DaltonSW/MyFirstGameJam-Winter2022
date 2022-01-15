using Godot;
using System;

public class Player : KinematicBody2D
{
	private float MOVE_AMT = 2;
	private float GRAVITY = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

 	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		MoveAndCollide(new Vector2(0, GRAVITY));

		if (Input.IsKeyPressed((int)KeyList.W))
		{
			MoveAndCollide(new Vector2(0, -MOVE_AMT));
		}

		if (Input.IsKeyPressed((int)KeyList.S))
		{
			MoveAndCollide(new Vector2(0, MOVE_AMT));
		}

		if (Input.IsKeyPressed((int)KeyList.D))
		{
			MoveAndCollide(new Vector2(MOVE_AMT, 0));
		}

		if (Input.IsKeyPressed((int)KeyList.A))
		{
			MoveAndCollide(new Vector2(-MOVE_AMT, 0));
		}
	}
}
