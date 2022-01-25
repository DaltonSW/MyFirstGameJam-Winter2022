using Godot;
using System;

public class Player : KinematicBody2D
{
	private PackedScene projectileScene;
	private PackedScene shotgunScene;


	private AnimatedSprite animatedSprite;
	private Sprite crouchingSprite;
	private Sprite slidingSprite;

	private CollisionShape2D normalCollision;
	private CollisionShape2D crouchingCollision;
	private CollisionShape2D slidingCollision;
	
	private CollisionShape2D normalInteraction;
	private CollisionShape2D crouchingInteraction;
	private CollisionShape2D slidingInteraction;

	private Area2D interactionArea;

	public Vector2 velocity = new Vector2();
	private Vector2 spawnPosition = new Vector2(0, 0);

	[Export] public float HEALTH = 10;

	[Export] public float JUMP_HEIGHT = 145; //pixels
	[Export] public float TIME_IN_AIR = 0.2F; //honestly no idea
	[Export] public float MOVE_SPEED = 60; //pixels per second
	[Export] public float GROUND_SPEED_CAP = 500; //pixels per second
	[Export] public float JUMP_SPEED;
	[Export] public float GRAVITY;
	[Export] public float FRICTION = 40; //no idea
	[Export] public float BASE_WALL_JUMP_AWAY = 350;
	[Export] public float WALL_JUMP_SCALE = 2;

	[Export] private float DASH_SPEED = 400;
	[Export] private float DASH_DISTANCE = 200;
	private float CURRENT_DASH = 0;
	
	[Export] private float SLIDE_SPEED = 400;
	[Export] private float SLIDE_DISTANCE = 1000;
	private float CURRENT_SLIDE = 0;

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
	public bool isCrouching = false;
	public bool isSliding = false;

	private bool canDash = false;
	private bool canSlide = true;

	enum EquippedWeapon
	{

	}

	//if down, change sprite and collisions
	//if holding down + dash, change sprite and collisions, and apply dash velocity
	//	if sliding, be able to jump out (might take a bit to feel good)


	public override void _Ready()
	{
		projectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
		shotgunScene = GD.Load<PackedScene>("res://Scenes/Shotgun.tscn");

		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		crouchingSprite = GetNode<Sprite>("CrouchingSprite");
		slidingSprite = GetNode<Sprite>("SlidingSprite");

		normalCollision = GetNode<CollisionShape2D>("NormalCollision");
		crouchingCollision = GetNode<CollisionShape2D>("CrouchingCollision");
		slidingCollision = GetNode<CollisionShape2D>("SlidingCollision");

		normalInteraction = GetNode<CollisionShape2D>("InteractionArea/NormalInteraction");
		crouchingInteraction = GetNode<CollisionShape2D>("InteractionArea/CrouchingInteraction");
		slidingInteraction = GetNode<CollisionShape2D>("InteractionArea/SlidingInteraction");

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

		if (isSliding)
		{
			CURRENT_SLIDE += SLIDE_SPEED * delta;
			MoveAndSlide(velocity);
			if(CURRENT_SLIDE > SLIDE_DISTANCE)
			{
				velocity = new Vector2(0, 0);
				isSliding = false;
				ClearSpritesAndHitboxes();
				ActivateNormal();
				animatedSprite.Play("idle");
				CURRENT_SLIDE = 0;
			}
		}

		if (isDashing)
		{
			if (IsOnWall())
			{
				isDashing = false;
			}
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

		bool right = 		Input.IsActionPressed("player_right"); 
		bool left = 		Input.IsActionPressed("player_left"); 
		bool crouch =		Input.IsActionPressed("player_crouch");

		bool jump = 		Input.IsActionJustPressed("player_jump");
		bool dash = 		Input.IsActionJustPressed("player_dash");
		bool shoot = 		Input.IsActionJustPressed("player_shoot");
		bool melee = 		Input.IsActionJustPressed("player_melee");
		bool interacted = 	Input.IsActionJustPressed("ui_select");

		
		// Iterate through bodies colliding with interaction hitbox
		foreach (Node2D body in interactionArea.GetOverlappingBodies()) {
			// Interact if button just pressed
			string interactionMethodName = "interact_with_player";
			if (!interacting && interacted && body.HasMethod(interactionMethodName)) {
				interacting = true;
				body.Call(interactionMethodName);
			}
			// Collect if collectable on collision
			string collectMethodName = "collect";
			if (body.HasMethod(collectMethodName)) {
				body.Call(collectMethodName);
			}
		}
		
		if (right && !crouch) 
		{
			velocity.x = Math.Min(velocity.x + MOVE_SPEED, GROUND_SPEED_CAP);
			GlobalTransform = new Transform2D(new Vector2(2, 0), new Vector2(0, 2), new Vector2(Position.x, Position.y));
			isFacingLeft = false;

		}

		if (left && !crouch) 
		{
			velocity.x = Math.Max(velocity.x - MOVE_SPEED, -GROUND_SPEED_CAP);
			GlobalTransform = new Transform2D(new Vector2(-2, 0), new Vector2(0, 2), new Vector2(Position.x, Position.y));
			isFacingLeft = true;
		}
		
		if (crouch && !isSliding && !isDashing)
		{
			ClearSpritesAndHitboxes();
			ActivateCrouch();

			isCrouching = true;
		}

		if (isCrouching && !crouch)
		{
			animatedSprite.Visible = true;
			crouchingSprite.Visible = false;

			normalCollision.Disabled = false;
			crouchingCollision.Disabled = true;

			normalInteraction.Disabled = false;
			crouchingInteraction.Disabled = true;
		}

		if (Input.IsActionJustPressed("player_dash"))
		{
			if (!IsOnFloor() && canDash)
			{
				Dash();
				MoveAndSlide(velocity);
				return;
			}

			if (IsOnFloor())
			{
				Slide();
				MoveAndSlide(velocity);
				return;
			}
			
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

		if (velocity.x > 0){
			velocity.x = Math.Max(0, velocity.x - FRICTION);
		}

		if (velocity.x < 0){
			velocity.x = Math.Min(0, velocity.x + FRICTION);
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

	private void Slide()
	{
		ClearSpritesAndHitboxes();
		ActivateSlide();

		GD.Print("Is sliding!");

		if (isFacingLeft)
		{
			velocity = new Vector2(-SLIDE_SPEED, 0);
		}

		else
		{
			velocity = new Vector2(SLIDE_SPEED, 0);
		}
		isSliding = true;
		//canSlide = false;
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

	private void ClearSpritesAndHitboxes()
	{
		animatedSprite.Visible = false;
		crouchingSprite.Visible = false;
		slidingSprite.Visible = false;

		normalCollision.Disabled = true;
		crouchingCollision.Disabled = true;
		slidingCollision.Disabled = true;

		normalInteraction.Disabled = true;
		crouchingInteraction.Disabled = true;
		slidingInteraction.Disabled = true;
	}

	private void ActivateNormal()
	{
		animatedSprite.Visible = true;
		normalCollision.Disabled = false;
		normalInteraction.Disabled = false;
	}

	private void ActivateCrouch()
	{
		crouchingSprite.Visible = true;
		crouchingCollision.Disabled = false;
		crouchingInteraction.Disabled = false;
	}

	private void ActivateSlide()
	{
		slidingSprite.Visible = true;
		slidingCollision.Disabled = false;
		slidingInteraction.Disabled = false;
	}
}
