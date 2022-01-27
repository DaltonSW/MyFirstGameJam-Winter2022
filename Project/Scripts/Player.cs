using Godot;
using System;

public class Player : KinematicBody2D
{
	#region Properties
	private PackedScene projectileScene;
	private PackedScene shotgunScene;

	private AnimatedSprite animatedSprite;
	private Sprite crouchingSprite;
	private Sprite slidingSprite;

	private CollisionShape2D[] normalCollisionBoxes;
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

	[Export] public float DASH_SPEED = 400;
	[Export] public float DASH_DISTANCE = 200;
	private float currentDashDistance = 0;

	[Export] public float SLIDE_SPEED = 600;
	[Export] public float SLIDE_DISTANCE = 200;
	private float currentSlideDistance = 0;

	[Export] private float JUMP_LOCKOUT = 10; //frames
	[Export] private float currentJumpBuffer;
	[Export] private float SHOTGUN_LOCKOUT = 1; //seconds
	[Export] private float CUR_SHOTGUN_BUFFER;

	[Export(PropertyHint.Range, "1,20,")]
	private int SHOTGUN_BLAST_COUNT = 7;

	[Export] public bool interacting = false;

	private bool IS_SHOTGUN_EQUIPPED = false;

	public bool isFacingLeft = false;
	public bool isJumping = false;
	public bool isDashing = false;
	public bool isCrouching = false;
	public bool isSliding = false;

	private bool canDash = false;
	private bool canSlide = true;

	private const int SPRITE_SCALE = 2;

	private readonly Vector2 UP = new Vector2(0, -1);
	#endregion

	public override void _Ready()
	{
		projectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
		shotgunScene = GD.Load<PackedScene>("res://Scenes/Shotgun.tscn");

		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		crouchingSprite = GetNode<Sprite>("CrouchingSprite");
		slidingSprite = GetNode<Sprite>("SlidingSprite");

		normalCollisionBoxes = new CollisionShape2D[]
		{
			GetNode<CollisionShape2D>("NormalCollision0"),
			GetNode<CollisionShape2D>("NormalCollision1"),
			GetNode<CollisionShape2D>("NormalCollision2"),
		};
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
			DoMove();
			return;
		}

		bool right = Input.IsActionPressed("player_right");
		bool left = Input.IsActionPressed("player_left");
		bool crouch = Input.IsActionPressed("player_crouch");

		bool jump = Input.IsActionJustPressed("player_jump");
		bool dash = Input.IsActionJustPressed("player_dash");
		bool shoot = Input.IsActionJustPressed("player_shoot");
		bool melee = Input.IsActionJustPressed("player_melee");

		if (isDashing)
		{
			if (IsOnWall())
			{
				isDashing = false;
			}
			currentDashDistance += DASH_SPEED * delta;
			DoMove();
			if (currentDashDistance > DASH_DISTANCE)
			{
				velocity = new Vector2(0, 0);
				isDashing = false;
				animatedSprite.Play("idle");
				currentDashDistance = 0;
			}
			return;
		}

		velocity.y += GRAVITY * delta;

		if (isSliding)
		{
			if (jump)
			{
				velocity.y -= JUMP_SPEED;
				StopSlide();
				//animatedSprite.Play("jump");
				DoMove();
				return;
			}

			currentSlideDistance += SLIDE_SPEED * delta;
			DoMove();
			if (currentSlideDistance > SLIDE_DISTANCE)
			{
				velocity = new Vector2(0, 0);
				StopSlide();
				animatedSprite.Play("idle");
			}
			return;
		}

		TryInteractions();

		if (right && !crouch)
		{
			velocity.x = Math.Min(velocity.x + MOVE_SPEED, GROUND_SPEED_CAP);
			FaceRight();
		}

		if (left && !crouch)
		{
			velocity.x = Math.Max(velocity.x - MOVE_SPEED, -GROUND_SPEED_CAP);
			FaceLeft();
		}

		if (crouch && !isSliding && !isDashing)
		{
			StartCrouch();
		}

		if (isCrouching && !crouch)
		{
			StopCrouch();
		}

		if (Input.IsActionJustPressed("player_dash"))
		{
			if (!IsOnFloor() && canDash)
			{
				StartDash();
				DoMove();
				return;
			}

			if (IsOnFloor() && isCrouching && canSlide)
			{
				StartSlide();
				DoMove();
				return;
			}

		}

		// Jump
		if (jump && currentJumpBuffer == 0)
		{
			StartJump();
		}

		if (IsOnFloor())
		{
			// Reset dash
			canDash = true;
			currentDashDistance = 0;

			// Reset jump
			StopJump();
		}

		if (currentJumpBuffer != 0)
		{
			currentJumpBuffer += 1;
			if (currentJumpBuffer > JUMP_LOCKOUT)
			{
				currentJumpBuffer = 0;
			}
		}

		ApplyFriction();
		DoMove();
	}

	public override void _Process(float delta)
	{
		if (IsOnWall())
		{
			animatedSprite.Play("wall_slide");
		}
		else if (!IsOnFloor() && velocity.y < 0)
		{
			animatedSprite.Play("jump");
		}
		else if (!IsOnFloor() && velocity.y > 0)
		{
			animatedSprite.Play("fall");
		}
		else if (velocity.x == 0)
		{
			animatedSprite.Play("idle");
		}
		else
		{
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

	private void TryInteractions()
	{
		bool interacted = Input.IsActionJustPressed("ui_select");
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
	}

	private void ApplyFriction()
	{
		if (velocity.x > 0)
		{
			velocity.x = Math.Max(0, velocity.x - FRICTION);
		}

		if (velocity.x < 0)
		{
			velocity.x = Math.Min(0, velocity.x + FRICTION);
		}
	}

	private void DoMove()
	{
		bool snapToGround = !isJumping;
		Vector2 snapVector = snapToGround ? new Vector2(0, 10) : new Vector2(0, 0);
		velocity = MoveAndSlideWithSnap(velocity, snapVector, UP, true);
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.G)
			{
				if (!IS_SHOTGUN_EQUIPPED)
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

	#region Movement Methods
	private void StartJump()
	{
		if (IsOnFloor())
		{
			velocity.y -= JUMP_SPEED;
		}

		else if (IsOnWall())
		{
			float mult = WALL_JUMP_SCALE;
			mult *= GetSlideCollision(0).Normal.x > 0 ? 1 : -1;
			velocity.y = (float)(-1.1 * JUMP_SPEED);
			velocity.x = (float)(mult * BASE_WALL_JUMP_AWAY);
		}
		isJumping = true;
		currentJumpBuffer += 1;
	}

	private void StopJump()
	{
		isJumping = false;
		currentJumpBuffer = 0;
	}

	private void StartCrouch()
	{
		SwitchToCrouchSpriteAndHitboxes();
		isCrouching = true;
	}

	private void StopCrouch()
	{
		SwitchToNormalSpriteAndHitboxes();
		isCrouching = false;
	}

	private void StartSlide()
	{
		SwitchToSlideSpriteAndHitboxes();
		int xMultiplier = isFacingLeft ? -1 : 1;
		velocity = new Vector2(xMultiplier * SLIDE_SPEED, 0);
		isSliding = true;
		canSlide = false;
	}

	private void StopSlide()
	{
		isSliding = false;
		canSlide = true;
		currentSlideDistance = 0;
		SwitchToNormalSpriteAndHitboxes();
	}
	
	private void StartDash()
	{
		animatedSprite.Play("dash");
		int xMultiplier = isFacingLeft ? -1 : 1;
		velocity = new Vector2(xMultiplier * DASH_SPEED, 0);
		isDashing = true;
		canDash = false;
	}
	#endregion

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

	#region Visual Methods
	private void ClearSpritesAndHitboxes()
	{
		animatedSprite.Visible = false;
		crouchingSprite.Visible = false;
		slidingSprite.Visible = false;

		foreach (CollisionShape2D normalCollisionBox in normalCollisionBoxes)
		{
			normalCollisionBox.Disabled = true;
		}
		crouchingCollision.Disabled = true;
		slidingCollision.Disabled = true;

		normalInteraction.Disabled = true;
		crouchingInteraction.Disabled = true;
		slidingInteraction.Disabled = true;
	}

	private void ActivateNormalSpriteAndHitboxes()
	{
		animatedSprite.Visible = true;
		foreach (CollisionShape2D normalCollisionBox in normalCollisionBoxes)
		{
			normalCollisionBox.Disabled = false;
		}
		normalInteraction.Disabled = false;
	}

	private void SwitchToNormalSpriteAndHitboxes()
	{
		ClearSpritesAndHitboxes();
		ActivateNormalSpriteAndHitboxes();
	}

	private void ActivateCrouchSpriteAndHitboxes()
	{
		crouchingSprite.Visible = true;
		crouchingCollision.Disabled = false;
		crouchingInteraction.Disabled = false;
	}

	private void SwitchToCrouchSpriteAndHitboxes()
	{
		ClearSpritesAndHitboxes();
		ActivateCrouchSpriteAndHitboxes();
	}

	private void ActivateSlideSpriteAndHitboxes()
	{
		slidingSprite.Visible = true;
		slidingCollision.Disabled = false;
		slidingInteraction.Disabled = false;
	}

	private void SwitchToSlideSpriteAndHitboxes()
	{
		ClearSpritesAndHitboxes();
		ActivateSlideSpriteAndHitboxes();
	}
	
	private void Face(bool left)
	{
		int xMultiplier = left ? -1 : 1;
		GlobalTransform = new Transform2D(new Vector2(xMultiplier * SPRITE_SCALE, 0), new Vector2(0, SPRITE_SCALE), new Vector2(Position.x, Position.y));
		isFacingLeft = left;
	}

	private void FaceRight() { Face(false); }
	private void FaceLeft() { Face(true); }
	#endregion
}
