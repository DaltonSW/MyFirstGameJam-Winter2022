using Godot;
using System;

public class KinematicPlayer : KinematicBody2D
{
	private float MOVE_SPEED = 10;
	private float JUMP_SPEED = 300;
	private float GRAVITY = 200;
	private float FRICTION = 4;

	Vector2 velocity = new Vector2();

	public override void _PhysicsProcess(float delta)
	{
		velocity.y += GRAVITY * delta;
		//if (IsOnFloor())
		//{
		if (velocity.x > 0){
			velocity.x = Math.Max(0, velocity.x - FRICTION);
		}

		if (velocity.x < 0){
			velocity.x = Math.Min(0, velocity.x + FRICTION);
		}
		//}

		bool right = Input.IsActionPressed("ui_right") || Input.IsKeyPressed((int)KeyList.D);
		bool left = Input.IsActionPressed("ui_left") || Input.IsKeyPressed((int)KeyList.A);
		bool jump = Input.IsActionPressed("ui_up") || Input.IsKeyPressed((int)KeyList.W) || Input.IsKeyPressed((int)KeyList.Space);

		if (right) {
			velocity.x += MOVE_SPEED;
		}

		if (left) {
			velocity.x -= MOVE_SPEED;
		}

		if (jump && IsOnFloor())
		{
			velocity.y -= JUMP_SPEED;
		}

		velocity = MoveAndSlide(velocity, new Vector2(0, -1));
	}
}
