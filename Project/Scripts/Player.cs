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
	private CollisionShape2D crouchingArrowUpShape;
	private Area2D crouchingArrowUp;
	private CollisionShape2D slidingCollision;
	
	private Area2D meleeCollision;
	private CollisionShape2D meleeCollisionShape;

	private CollisionShape2D normalInteraction;
	private CollisionShape2D crouchingInteraction;
	private CollisionShape2D slidingInteraction;

	private Area2D interactionArea;

	public Vector2 velocity = new Vector2();
	private Vector2 spawnPosition = new Vector2(0, 0);

	[Signal] delegate void PlayerKilled();

	[Export] public float MAX_HEALTH = 5;
	public float CURRENT_HEALTH;
	private AnimatedSprite healthSprite;

	private float INVINCIBILITY_BUFFER = 0.5F;
	private float CURRENT_INVINCIBILITY = 0;

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
	public bool isDying = false;
	public bool isSwinging = false;
	public bool swingStruck = false;

	private bool canDash = false;
	private bool canSlide = true;

	private const int SPRITE_SCALE = 2;

	private readonly Vector2 UP = new Vector2(0, -1);
	private Tween tween;
	#endregion

	#region Sound Properties
	private AudioStreamPlayer audioPlayer;
	private AudioStreamSample jumpSound;
	private AudioStreamSample shootSound;
	private AudioStreamSample hurtSound;
	private AudioStreamSample guitarHitSound;
	private AudioStreamSample guitarMissSound;
	#endregion

	public override void _Ready()
	{
		projectileScene = GD.Load<PackedScene>("res://Scenes/ShotgunPellet.tscn");
		shotgunScene = GD.Load<PackedScene>("res://Scenes/Shotgun.tscn");

		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		crouchingSprite = GetNode<Sprite>("CrouchingSprite");
		slidingSprite = GetNode<Sprite>("SlidingSprite");
		tween = GetNode<Tween>("Tween");
		tween.Connect("tween_all_completed", this, nameof(OnTweenCompleted));
		
		healthSprite = GetNode<AnimatedSprite>("/root/LevelHolder/UI/HealthBar");
		healthSprite.Frame = 5;
		healthSprite.Playing = false;

		normalCollisionBoxes = new CollisionShape2D[]
		{
			GetNode<CollisionShape2D>("NormalCollision0"),
			GetNode<CollisionShape2D>("NormalCollisionDown1"),
			GetNode<CollisionShape2D>("NormalCollisionDown2"),
		};
		crouchingCollision = GetNode<CollisionShape2D>("CrouchingCollision");
		crouchingArrowUp = GetNode<Area2D>("CrouchCollisionUp");
		crouchingArrowUpShape = crouchingArrowUp.GetNode<CollisionShape2D>("CrouchCollisionUpShape");
		slidingCollision = GetNode<CollisionShape2D>("SlidingCollision");

		normalInteraction = GetNode<CollisionShape2D>("InteractionArea/NormalInteraction");
		crouchingInteraction = GetNode<CollisionShape2D>("InteractionArea/CrouchingInteraction");
		slidingInteraction = GetNode<CollisionShape2D>("InteractionArea/SlidingInteraction");

		meleeCollision = GetNode<Area2D>("GuitarHitbox");
		meleeCollisionShape = meleeCollision.GetNode<CollisionShape2D>("CollisionShape2D");
		meleeCollisionShape.Disabled = true;

		interactionArea = GetNode<Area2D>("InteractionArea");

		GRAVITY = (float)(JUMP_HEIGHT / (2 * Math.Pow(TIME_IN_AIR, 2)));
		JUMP_SPEED = (float)Math.Sqrt(2 * JUMP_HEIGHT * GRAVITY);

		isDying = false;
		CURRENT_HEALTH = MAX_HEALTH;

		LoadSounds();
		ActivateNormalSpriteAndHitboxes();
		crouchingArrowUpShape.Disabled = true;
	}

	public override void _PhysicsProcess(float delta)
	{
		if (isSwinging)
		{
			if (animatedSprite.Frame == 2 && !swingStruck)
			{
				meleeCollisionShape.Disabled = false;
			}

			else 
			{
				meleeCollisionShape.Disabled = true;
			}

			if (!meleeCollisionShape.Disabled)
			{
				if (meleeCollision.GetOverlappingBodies().Count != 0)
				{
					foreach (Node2D body in meleeCollision.GetOverlappingBodies())
					{
						if (body is Enemy enemy)
						{
							enemy.HurtEnemy();
						}

						if (body is FlyingEnemy flyingEnemy)
						{
							flyingEnemy.HurtEnemy();
						}

						if (body is GroundedMiniboss groundedMiniboss)
						{
							groundedMiniboss.HurtEnemy();
						}
						meleeCollisionShape.Disabled = true;
						swingStruck = true;
						audioPlayer.Stream = guitarHitSound;
						audioPlayer.Play();
					}
				}
			}
		}

		else if (!isDying)
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
					audioPlayer.Stream = jumpSound;
					audioPlayer.Play();
					DoMove();
					return;
				}

				currentSlideDistance += SLIDE_SPEED * delta;
				DoMove();
				if (currentSlideDistance > SLIDE_DISTANCE)
				{
					velocity = new Vector2(0, 0);
					StopSlide();
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

			if (isCrouching && !crouch && crouchingArrowUp.GetOverlappingBodies().Count == 0)
			{
				StopCrouch();
			}

			if (Input.IsActionJustPressed("player_melee") && IsOnFloor() && !isCrouching)
			{
				SwingGuitar();
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
	}

	public override void _Process(float delta)
	{
		if (!isDying)
		{
			if (isSwinging)
			{
				animatedSprite.Play("melee");
			}

			else if (IsOnWall() && !IsOnFloor())
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

			if (Input.IsActionJustPressed("player_shoot") && (CUR_SHOTGUN_BUFFER == 0))
			{
				if (!IS_SHOTGUN_EQUIPPED)
				{
					EquipShotgun();
				}
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

			if (CURRENT_INVINCIBILITY != 0)
			{
				CURRENT_INVINCIBILITY += delta;
			}

			if (CURRENT_INVINCIBILITY > INVINCIBILITY_BUFFER)
			{
				CURRENT_INVINCIBILITY = 0;
				CycleTransparency(true);
			}
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
		if (!isDying)
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
	}

	#region Movement Methods
	private void StartJump()
	{
		if (IsOnFloor())
		{
			velocity.y -= JUMP_SPEED;
			audioPlayer.Stream = jumpSound;
			audioPlayer.Play();
		}

		else if (IsOnWall())
		{
			float mult = WALL_JUMP_SCALE;
			mult *= GetSlideCollision(0).Normal.x > 0 ? 1 : -1;
			velocity.y = (float)(-1.1 * JUMP_SPEED);
			velocity.x = (float)(mult * BASE_WALL_JUMP_AWAY);
			audioPlayer.Stream = jumpSound;
			audioPlayer.Play();
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
		if (crouchingArrowUp.GetOverlappingBodies().Count != 0)
		{
			SwitchToCrouchSpriteAndHitboxes();
		}

		else
		{
			SwitchToNormalSpriteAndHitboxes();
		}
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

	#region Attack Methods
	private void UnequipShotgun()
	{
		if(IS_SHOTGUN_EQUIPPED)
		{
			GetNode("Shotgun").QueueFree();
			IS_SHOTGUN_EQUIPPED = false;
		}
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
		audioPlayer.Stream = shootSound;
		audioPlayer.Play();
		for (int i = 1; i <= SHOTGUN_BLAST_COUNT; i++)
		{
			ShotgunPellet projectile = (ShotgunPellet)projectileScene.Instance();
			Position2D bulletSpawn = (Position2D)GetNode("Shotgun/BulletSpawn");
			projectile.Position = bulletSpawn.GlobalPosition;
			GetParent().GetParent().AddChild(projectile); //Have to use 2 to get the root of the level, not the Node2D the player is stored in
		}
	}

	private void SwingGuitar()
	{
		if (IS_SHOTGUN_EQUIPPED)
		{
			UnequipShotgun();
			IS_SHOTGUN_EQUIPPED = true;
		}
		ClearSpritesAndHitboxes();
		ActivateNormalSpriteAndHitboxes();
		velocity = new Vector2(0, 0);
		MoveAndSlide(velocity);
		animatedSprite.Play("melee");
		animatedSprite.Centered = false;
		animatedSprite.Offset = new Vector2(-20, -45);
		animatedSprite.SpeedScale = 2;
		isSwinging = true;
	}

	public void StopSwing()
	{
		isSwinging = false;
		animatedSprite.Centered = true;
		animatedSprite.Offset = new Vector2(0, 0);
		animatedSprite.SpeedScale = 1;
		if (!swingStruck)
		{
			audioPlayer.Stream = guitarMissSound;
			audioPlayer.Play();
		}

		if (IS_SHOTGUN_EQUIPPED)
		{
			EquipShotgun();
		}

		swingStruck = false;
	}
	#endregion

	#region Visual Methods
	private void ClearSpritesAndHitboxes()
	{
		animatedSprite.Visible = false;
		crouchingSprite.Visible = false;
		slidingSprite.Visible = false;

		foreach (CollisionShape2D normalCollisionBox in normalCollisionBoxes)
		{
			normalCollisionBox.SetDeferred("disabled", true);
		}
		crouchingCollision.SetDeferred("disabled", true);
		crouchingArrowUpShape.SetDeferred("disabled", true);
		slidingCollision.SetDeferred("disabled", true);

		normalInteraction.SetDeferred("disabled", true);
		crouchingInteraction.SetDeferred("disabled", true);
		slidingInteraction.SetDeferred("disabled", true);

		if (IS_SHOTGUN_EQUIPPED)
		{
			UnequipShotgun();
		}
	}

	private void ActivateNormalSpriteAndHitboxes()
	{
		animatedSprite.Visible = true;
		foreach (CollisionShape2D normalCollisionBox in normalCollisionBoxes)
		{
			normalCollisionBox.SetDeferred("disabled", false);
		}
		normalInteraction.SetDeferred("disabled", false);
	}

	private void SwitchToNormalSpriteAndHitboxes()
	{
		ClearSpritesAndHitboxes();
		ActivateNormalSpriteAndHitboxes();
	}

	private void ActivateCrouchSpriteAndHitboxes()
	{
		crouchingSprite.Visible = true;
		crouchingCollision.SetDeferred("disabled", false);
		crouchingArrowUpShape.SetDeferred("disabled", false);
		crouchingInteraction.SetDeferred("disabled", false);
	}

	private void SwitchToCrouchSpriteAndHitboxes()
	{
		ClearSpritesAndHitboxes();
		ActivateCrouchSpriteAndHitboxes();
	}

	private void ActivateSlideSpriteAndHitboxes()
	{
		slidingSprite.Visible = true;
		slidingCollision.SetDeferred("disabled", false);
		crouchingArrowUpShape.SetDeferred("disabled", false);
		slidingInteraction.SetDeferred("disabled", false);
	}

	private void SwitchToSlideSpriteAndHitboxes()
	{
		ClearSpritesAndHitboxes();
		ActivateSlideSpriteAndHitboxes();
	}

	private void CycleTransparency(bool lighten)
	{
		Color tempNormal = animatedSprite.Modulate;
		Color tempCrouch = crouchingSprite.Modulate;
		Color tempSlide = slidingSprite.Modulate;

		tempNormal.a = lighten ? 1 : 0.5F;	
		tempCrouch.a = lighten ? 1 : 0.5F;	
		tempSlide.a = lighten ? 1 : 0.5F;

		animatedSprite.Modulate = tempNormal;
		crouchingSprite.Modulate = tempCrouch;
		slidingSprite.Modulate = tempSlide;
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
	
	public void RecalcPhysics()
	{
		GRAVITY = (float)(JUMP_HEIGHT / (2 * Math.Pow(TIME_IN_AIR, 2)));
		JUMP_SPEED = (float)Math.Sqrt(2 * JUMP_HEIGHT * GRAVITY);
	}

	public void KillPlayer()
	{
		SwitchToNormalSpriteAndHitboxes();
		CycleTransparency(true);
		animatedSprite.Play("health_death");
		Die();
	}

	private void Die()
	{
		healthSprite.Frame = 0;
		isDying = true;
		PauseMode = PauseModeEnum.Process;
		GetTree().Paused = true;
	}

	public void FallAndDie()
	{
		animatedSprite.Play("fall_death");
		Die();
		Tween tween = GetNode<Tween>("Tween");
		tween.InterpolateProperty(this, "position",
			Position, new Vector2(Position.x, Position.y + 200), .3f,
			Tween.TransitionType.Linear, Tween.EaseType.In, .3f);
		tween.Start();
	}

	public void OnTweenCompleted()
	{
		EmitSignal(nameof(PlayerKilled));
	}

	public void HealPlayer()
	{
		CURRENT_HEALTH = MAX_HEALTH;
		healthSprite.Frame = 5;
	}

	public void HurtPlayer()
	{
		if (CURRENT_INVINCIBILITY == 0)
		{
			CURRENT_INVINCIBILITY = 0.016667F;
			CycleTransparency(false);
			CURRENT_HEALTH--;
			healthSprite.Frame = (int)CURRENT_HEALTH;
			audioPlayer.Stream = hurtSound;
			audioPlayer.Play();
			if(CURRENT_HEALTH == 0)
			{
				KillPlayer();
			}
		}

	}

	public void ResetPlayer()
	{
		GD.Print("resetting");
		HealPlayer();
		ClearSpritesAndHitboxes();
		ActivateNormalSpriteAndHitboxes();
		animatedSprite.Play("idle");
		PauseMode = PauseModeEnum.Inherit;
		isDying = false;
		UnequipShotgun();
	}

	private void LoadSounds()
	{
		audioPlayer = GetNode<AudioStreamPlayer>("AudioPlayer");
		audioPlayer.VolumeDb = -18;

		jumpSound = GD.Load<AudioStreamSample>("res://Sounds/SFX/jump.wav");
		shootSound = GD.Load<AudioStreamSample>("res://Sounds/SFX/shoot.wav");
		hurtSound = GD.Load<AudioStreamSample>("res://Sounds/SFX/hurt.wav");
		//guitarHitSound = GD.Load<AudioStreamSample>("res://Sounds/SFX/guitar_hit.wav");
		//guitarMissSound = GD.Load<AudioStreamSample>("res://Sounds/SFX/guitar_miss.wav");
		GD.Print("Sounds");
	}
}
