using Godot;
using System;

public class Player : KinematicBody2D
{
	PackedScene projectileScene;
	PackedScene shotgunScene;

	Vector2 velocity = new Vector2();

	[Export] private float JUMP_HEIGHT = 50; //pixels
	[Export] private float TIME_IN_AIR = 0.25F; //honestly no idea
	[Export] private float MOVE_SPEED = 15; //pixels per second
	[Export] private float GROUND_SPEED_CAP = 150; //pixels per second
	[Export] private float JUMP_SPEED;
	[Export] private float GRAVITY;
	[Export] private float FRICTION = 7; //no idea

	[Export] private float JUMP_LOCKOUT = 10; //frames
	[Export] private float CUR_JUMP_BUFFER;

	[Export] private float SHOTGUN_LOCKOUT = 1; //seconds
	[Export] private float CUR_SHOTGUN_BUFFER;

	[Export(PropertyHint.Range, "1,20,")] 
	private int SHOTGUN_BLAST_COUNT = 7;

	private bool IS_SHOTGUN_EQUIPPED = false;

	public bool isFacingLeft = false;

	enum EquippedWeapon
	{

	}


	public override void _Ready()
	{
		projectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
		shotgunScene = GD.Load<PackedScene>("res://Scenes/Shotgun.tscn");
		
		GRAVITY = (float)(JUMP_HEIGHT / (2 * Math.Pow(TIME_IN_AIR, 2)));
		JUMP_SPEED = (float)Math.Sqrt(2 * JUMP_HEIGHT * GRAVITY);
	}

	public override void _PhysicsProcess(float delta)
	{
		velocity.y += GRAVITY * delta;
		
		if (velocity.x > 0){
			velocity.x = Math.Max(0, velocity.x - FRICTION);
		}

		if (velocity.x < 0){
			velocity.x = Math.Min(0, velocity.x + FRICTION);
		}

		bool right = Input.IsActionPressed("ui_right"); 
		bool left = Input.IsActionPressed("ui_left"); 
		bool jump = Input.IsActionJustPressed("ui_up"); 

		if (right) {
			velocity.x = Math.Min(velocity.x + MOVE_SPEED, GROUND_SPEED_CAP);
			GlobalTransform = new Transform2D(new Vector2(1,0), new Vector2(0,1), new Vector2(Position.x, Position.y));
			isFacingLeft = false;

		}

		if (left) {
			velocity.x = Math.Max(velocity.x - MOVE_SPEED, -GROUND_SPEED_CAP);
			GlobalTransform = new Transform2D(new Vector2(-1,0), new Vector2(0,1), new Vector2(Position.x, Position.y));
			isFacingLeft = true;
		}

		if (jump && CUR_JUMP_BUFFER == 0)
		{
			if (IsOnFloor())
			{
				velocity.y -= JUMP_SPEED;
			}

			else if (IsOnWall())
			{
				velocity.y = (float)(-1.1 * JUMP_SPEED);
			}
			CUR_JUMP_BUFFER += 1;
		}

		if(CUR_JUMP_BUFFER != 0)
		{
			if(IsOnFloor())
			{
				CUR_JUMP_BUFFER = 0;
			}

			else{
				CUR_JUMP_BUFFER += 1;
				if(CUR_JUMP_BUFFER > JUMP_LOCKOUT)
				{
					CUR_JUMP_BUFFER = 0;
				}
			}
		}

		velocity = MoveAndSlide(velocity, new Vector2(0, -1));
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("ui_select") && IS_SHOTGUN_EQUIPPED && (CUR_SHOTGUN_BUFFER == 0))
		{
			CUR_SHOTGUN_BUFFER = delta;
			ShootShotgun();
		}

		if (CUR_SHOTGUN_BUFFER != 0)
		{
			CUR_SHOTGUN_BUFFER += delta;	
		}

		if (CUR_SHOTGUN_BUFFER > SHOTGUN_LOCKOUT)
		{
			CUR_SHOTGUN_BUFFER = 0;
		}
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.G)
			{
				if(!IS_SHOTGUN_EQUIPPED)
				{
					EquipShotgun();
				}
				else
				{
					UnequipShotgun();
				}
			}

			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.R)
			{
				Position2D spawnPoint = (Position2D)GetParent().GetNode("SpawnPoint");
				Position = spawnPoint.Position;
			}
		}
	}

	private void UnequipShotgun()
	{
		GetNode("Shotgun").QueueFree();
		IS_SHOTGUN_EQUIPPED = false;
	}

	private void EquipShotgun()
	{
		IS_SHOTGUN_EQUIPPED = true;
		Shotgun shotgun = (Shotgun)shotgunScene.Instance();
		Position2D itemHoldPosition = (Position2D)GetNode("ItemHold");
		shotgun.Position = itemHoldPosition.Position;
		AddChild(shotgun);
	}

	private void ShootShotgun()
	{
		for (int i = 1; i <= SHOTGUN_BLAST_COUNT; i++)
		{
			Projectile projectile = (Projectile)projectileScene.Instance();
			Position2D bulletSpawn = (Position2D)GetNode("Shotgun/BulletSpawn");
			projectile.Position = bulletSpawn.GlobalPosition;
			GetParent().GetParent().AddChild(projectile); //Have to use 2 to get the root of the level, not the Node2D the player is stored in
		}
	}
}
