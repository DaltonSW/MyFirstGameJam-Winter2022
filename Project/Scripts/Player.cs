using Godot;
using System;

public class Player : KinematicBody2D
{
	private PackedScene projectileScene;
	private PackedScene shotgunScene;
	private AnimatedSprite animatedSprite;
	private Area2D interactionArea;

	public Vector2 velocity = new Vector2();
	private Vector2 spawnPosition = new Vector2(0, 0);

	[Export] public float JUMP_HEIGHT = 90; //pixels
	[Export] public float TIME_IN_AIR = 0.2F; //honestly no idea
	[Export] public float MOVE_SPEED = 70; //pixels per second
	[Export] public float GROUND_SPEED_CAP = 250; //pixels per second
	[Export] public float JUMP_SPEED;
	[Export] public float GRAVITY;
	[Export] public float FRICTION = 7; //no idea
	[Export] public float BASE_WALL_JUMP_AWAY = 100;
	[Export] public float WALL_JUMP_SCALE = 2;

	[Export] private float DASH_SPEED = 400;
	[Export] private float DASH_DISTANCE = 200;
	private float CURRENT_DASH = 0;

	[Export] private float JUMP_LOCKOUT = 10; //frames
	[Export] private float CUR_JUMP_BUFFER;

	[Export] private float SHOTGUN_LOCKOUT = 1; //seconds
	[Export] private float CUR_SHOTGUN_BUFFER;

	[Export(PropertyHint.Range, "1,20,")] 
	private int SHOTGUN_BLAST_COUNT = 7;

	[Export] public bool interacting = false;
	
	private bool IS_SHOTGUN_EQUIPPED = false;

	public bool isFacingLeft = false;
	public bool isDashing = false;
	private bool canDash = false;

	enum EquippedWeapon
	{

	}


	public override void _Ready()
	{
		projectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
		shotgunScene = GD.Load<PackedScene>("res://Scenes/Shotgun.tscn");
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		interactionArea = GetNode<Area2D>("InteractionArea");
		
		GRAVITY = (float)(JUMP_HEIGHT / (2 * Math.Pow(TIME_IN_AIR, 2)));
		JUMP_SPEED = (float)Math.Sqrt(2 * JUMP_HEIGHT * GRAVITY);
	}

	public override void _PhysicsProcess(float delta)
	{
		if (!Global.isPlaying)
		{
			velocity = new Vector2(0, 0);
			MoveAndSlide(velocity);
			return;
		}

		if (isDashing)
		{
			CURRENT_DASH += DASH_SPEED * delta;
			MoveAndSlide(velocity);
			if (CURRENT_DASH > DASH_DISTANCE)
			{
				velocity = new Vector2(0, 0);
				isDashing = false;
				animatedSprite.Play("idle");
				CURRENT_DASH = 0;
			}
			return;
		}

		
		if (velocity.x > 0){
			velocity.x = Math.Max(0, velocity.x - FRICTION);
		}

		if (velocity.x < 0){
			velocity.x = Math.Min(0, velocity.x + FRICTION);
		}

		bool right = Input.IsActionPressed("ui_right"); 
		bool left = Input.IsActionPressed("ui_left"); 
		bool jump = Input.IsActionJustPressed("ui_up");
		bool interacted = Input.IsActionJustPressed("ui_select");
		
		// Iterate through bodies colliding with interaction hitbox
		foreach (Node2D body in interactionArea.GetOverlappingBodies()) {
			// Interact if button just pressed
			var interactionMethodName = "interact_with_player";
			if (!interacting && interacted && body.HasMethod(interactionMethodName)) {
				interacting = true;
				body.Call(interactionMethodName);
			}
			// Collect if collectable on collision
			var collectMethodName = "collect";
			if (body.HasMethod(collectMethodName)) {
				body.Call(collectMethodName);
			}
		}
		
		if (right) {
			velocity.x = Math.Min(velocity.x + MOVE_SPEED, GROUND_SPEED_CAP);
			GlobalTransform = new Transform2D(new Vector2(2, 0), new Vector2(0, 2), new Vector2(Position.x, Position.y));
			isFacingLeft = false;

		}

		if (left) {
			velocity.x = Math.Max(velocity.x - MOVE_SPEED, -GROUND_SPEED_CAP);
			GlobalTransform = new Transform2D(new Vector2(-2, 0), new Vector2(0, 2), new Vector2(Position.x, Position.y));
			isFacingLeft = true;
		}

		if (Input.IsActionJustPressed("player_dash"))
		{
			Dash();
			MoveAndSlide(velocity);
			return;
		}

		if (jump && CUR_JUMP_BUFFER == 0)
		{
			if (IsOnFloor())
			{
				velocity.y -= JUMP_SPEED;
			}

			else if (IsOnWall())
			{
				float mult = WALL_JUMP_SCALE;
				mult *= GetSlideCollision(0).Normal.x > 0 ? 1 : -1 ;
				velocity.y = (float)(-1.1 * JUMP_SPEED);
				velocity.x = (float)(mult * BASE_WALL_JUMP_AWAY);
			}
			CUR_JUMP_BUFFER += 1;
		}

		if (IsOnFloor())
		{
			canDash = true;
			CUR_JUMP_BUFFER = 0;
			CURRENT_DASH = 0;
		}

		if(CUR_JUMP_BUFFER != 0)
		{
			CUR_JUMP_BUFFER += 1;
			if(CUR_JUMP_BUFFER > JUMP_LOCKOUT)
			{
					CUR_JUMP_BUFFER = 0;
			}
		}

		velocity.y += GRAVITY * delta;

		velocity = MoveAndSlide(velocity, new Vector2(0, -1));
	}

	public override void _Process(float delta)
	{
		if (velocity.x == 0)
		{
			animatedSprite.Play("idle");
		}

		else {
			animatedSprite.Play("run");
		}
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
			
			// Leaving this in so I have the code to reuse, but this should be abstracted to the parent scene
			// Player scene shouldn't have to rely on the above scenes to have a SpawnPoint
			// if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.R)
			// {
			// 	Position2D spawnPoint = (Position2D)GetParent().GetParent().GetNode("SpawnPoint");
			// 	Position = spawnPoint.GlobalPosition;
			// }
		}
	}

	private void Dash()
	{
		animatedSprite.Play("dash");
		if (isFacingLeft)
		{
			velocity = new Vector2(-DASH_SPEED, 0);
		}

		else
		{
			velocity = new Vector2(DASH_SPEED, 0);
		}
		isDashing = true;
		canDash = false;
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

	public void RecalcPhysics()
	{
		GRAVITY = (float)(JUMP_HEIGHT / (2 * Math.Pow(TIME_IN_AIR, 2)));
		JUMP_SPEED = (float)Math.Sqrt(2 * JUMP_HEIGHT * GRAVITY);
	}
}
