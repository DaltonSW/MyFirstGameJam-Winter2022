using Godot;
using System;

public class Player : KinematicBody2D
{
	PackedScene projectileScene;
	PackedScene shotgunScene;

	private float JUMP_HEIGHT = 50;
	private float TIME_IN_AIR = 0.25F;
	private float MOVE_SPEED = 15;
	private float GROUND_SPEED_CAP = 150;
	private float JUMP_SPEED;
	private float GRAVITY;
	private float FRICTION = 7;
	private float JUMP_LOCKOUT = 10;
	private float CUR_JUMP_BUFFER;
	private bool IS_SHOTGUN_EQUIPPED = false;

	enum EquippedWeapon
	{

	}

	Vector2 velocity = new Vector2();

	public override void _Ready()
	{
		projectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
		shotgunScene = GD.Load<PackedScene>("res://Scenes/Shotgun.tscn");
		
		GRAVITY = (float)(JUMP_HEIGHT / (2 * Math.Pow(TIME_IN_AIR, 2)));
		JUMP_SPEED = (float)Math.Sqrt(2 * JUMP_HEIGHT * GRAVITY);
	}

	public override void _PhysicsProcess(float delta)
	{
		GD.Print(delta);
		velocity.y += GRAVITY * delta;
		
		if (velocity.x > 0){
			velocity.x = Math.Max(0, velocity.x - FRICTION);
		}

		if (velocity.x < 0){
			velocity.x = Math.Min(0, velocity.x + FRICTION);
		}

		bool right = Input.IsActionPressed("ui_right") 
			|| Input.IsKeyPressed((int)KeyList.D);

		bool left = Input.IsActionPressed("ui_left") 
			|| Input.IsKeyPressed((int)KeyList.A);

		bool jump = Input.IsActionPressed("ui_up") 
			|| Input.IsKeyPressed((int)KeyList.W); 

		if (right) {
			velocity.x = Math.Min(velocity.x + MOVE_SPEED, GROUND_SPEED_CAP);
		}

		if (left) {
			velocity.x = Math.Max(velocity.x - MOVE_SPEED, -GROUND_SPEED_CAP);
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

			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Space && IS_SHOTGUN_EQUIPPED)
			{
				ShootBullet();
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

	private void ShootBullet()
	{
		Projectile projectile = (Projectile)projectileScene.Instance();
		Position2D bulletSpawn = (Position2D)GetNode("Shotgun/BulletSpawn");
		projectile.Position = bulletSpawn.GlobalPosition;
		projectile.Rotation = Rotation + (float)(Math.PI * 90 / 180);
		GetParent().AddChild(projectile);
	}
}
