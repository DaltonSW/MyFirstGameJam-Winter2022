using Godot;
using System;

public class FlyingEnemy : KinematicBody2D
{

	[Export] public float HEALTH = 10;

	[Export] private int TIME_TO_TURN = 50;
	[Export] private int DIVE_TIME = 20;
	[Export] private int SPEED = 100;
	[Export] private int DIVE_SPEED = 150;
	private int GRAVITY = 0; //Pretty useless, just makes sure they stay stuck to the ground for now. Will be used if they need to jump eventually
	private int tick = 0;
	private int diveTick = 0;
	private Vector2 DIR_LEFT;
	private Vector2 DIR_RIGHT;
	private Vector2 DIR_UP;
	private Vector2 DIR_DOWN;
	private Vector2 currentDirection;
	private Vector2 preDiveDirection;

	private bool isDiving;
	private bool isReturning;
	private RayCast2D lineOfSight;
	private Area2D drillHitbox;


	public override void _Ready()
	{
		lineOfSight = GetNode<RayCast2D>("Sight");
		drillHitbox = GetNode<Area2D>("DrillHitbox");
		isDiving = false;
		DIR_LEFT  = new Vector2(-SPEED, GRAVITY);
		DIR_RIGHT = new Vector2( SPEED, GRAVITY);
		DIR_DOWN  = new Vector2(0, DIVE_SPEED);
		DIR_UP    = new Vector2(0, -DIVE_SPEED);
		drillHitbox.Connect("body_entered", this, nameof(OnDrillBodyEntered));
	}

	private void OnDrillBodyEntered(Node body)
	{
		if (body is Player player)
		{
			player.HurtPlayer();
		}
	}
	public override void _PhysicsProcess(float delta)
	{
		if (isDiving)
		{
			if (!isReturning && diveTick >= DIVE_TIME / 2)
			{
				isReturning = true;
				currentDirection = DIR_UP;
			}
			else if (isReturning && diveTick >= DIVE_TIME)
			{
				isDiving = false;
				isReturning = false;
				currentDirection = preDiveDirection;
			}
			else
			{
				diveTick++;
			}
		}
		else
		{
			if (tick == 0)
			{
				currentDirection = currentDirection == DIR_LEFT 
					? DIR_RIGHT 
					: DIR_LEFT;
			}
			tick++;
			if (tick >= TIME_TO_TURN)
			{
				tick = 0;
			}
		}
		MoveAndSlide(currentDirection);
	}

	public override void _Process(float delta)
	{
		if ((lineOfSight.GetCollider() is Player) && !isDiving)
		{
			StartDive();
		}
	}

	private void StartDive()
	{
		diveTick = 0;
		isDiving = true;
		preDiveDirection = currentDirection;
		currentDirection = DIR_DOWN;
	}

}
