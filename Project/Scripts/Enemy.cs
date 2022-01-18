using Godot;
using System;

public class Enemy : KinematicBody2D
{

	[Export] private int RANDOM_DIR_TICKS = 8;
	[Export] private int SPEED = 100;
	private int GRAVITY = 200; //Pretty useless, just makes sure they stay stuck to the ground for now. Will be used if they need to jump eventually
	private int CUR_DIR_TICK = 0;
	private Vector2 DIR_LEFT;
	private Vector2 DIR_RIGHT;
	private Vector2 CUR_DIR;
	private static Random RNG;

	public override void _Ready()
	{
		DIR_LEFT = new Vector2(-SPEED, GRAVITY);
		DIR_RIGHT = new Vector2(SPEED, GRAVITY);
		RNG = new Random();
	}

	public override void _PhysicsProcess(float delta)
	{
		if(CUR_DIR_TICK == 0)
		{
			CUR_DIR = RNG.Next(0, 2) == 1 ? DIR_LEFT : DIR_RIGHT;
		}

		CUR_DIR_TICK++;

		if (CUR_DIR_TICK >= RANDOM_DIR_TICKS)
		{
			CUR_DIR_TICK = 0;
		}

		MoveAndSlideWithSnap(CUR_DIR, new Vector2(-1, 0));
	}
}
