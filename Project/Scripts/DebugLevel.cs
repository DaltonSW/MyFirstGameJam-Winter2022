using Godot;
using System;

public class DebugLevel : Node2D
{
	private PackedScene playerScene;
	private Player player;
	private Position2D SpawnPoint;

	private Label JumpHeightLabel;
	private Label TimeInAirLabel;
	private Label MoveSpeedLabel;
	private Label GroundSpeedCapLabel;
	private Label FrictionLabel;
	private Label GravityLabel;
	private Label JumpSpeedLabel;
	private Label PosXLabel;
	private Label PosYLabel;
	private Label VelXLabel;
	private Label VelYLabel;

	private string OriginalJumpHeightLabel;
	private	string OriginalTimeInAirLabel;
	private	string OriginalMoveSpeedLabel;
	private	string OriginalGroundSpeedCapLabel;
	private	string OriginalFrictionLabel;
	private	string OriginalGravityLabel;
	private	string OriginalJumpSpeedLabel;
	private	string OriginalPosXLabel;
	private	string OriginalPosYLabel;
	private	string OriginalVelXLabel;
	private	string OriginalVelYLabel;

	private CHOSEN_PROP currentProperty = CHOSEN_PROP.JUMP_HEIGHT;

	enum CHOSEN_PROP
	{
		JUMP_HEIGHT = 1,
		TIME_IN_AIR = 2,
		MOVE_SPEED = 3,
		GROUND_SPEED_CAP = 4,
		FRICTION = 5
	}

	public override void _Ready()
	{
		playerScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
		player = (Player)playerScene.Instance();
		SpawnPoint = GetNode<Position2D>("SpawnPoint");
		player.Position = SpawnPoint.Position;
		AddChild(player);

		JumpHeightLabel = GetNode<Label>("LabelGroup/JumpHeight");
		TimeInAirLabel = GetNode<Label>("LabelGroup/TimeInAir");
		MoveSpeedLabel = GetNode<Label>("LabelGroup/MoveSpeed");
		GroundSpeedCapLabel = GetNode<Label>("LabelGroup/GroundSpeedCap");
		FrictionLabel = GetNode<Label>("LabelGroup/Friction");
		GravityLabel = GetNode<Label>("LabelGroup/GRAVITY");
		JumpSpeedLabel = GetNode<Label>("LabelGroup/JUMP_SPEED");
		PosXLabel = GetNode<Label>("LabelGroup/PosX");
		PosYLabel = GetNode<Label>("LabelGroup/PosY");
		VelXLabel = GetNode<Label>("LabelGroup/VelX");
		VelYLabel = GetNode<Label>("LabelGroup/VelY");

		OriginalJumpHeightLabel = JumpHeightLabel.Text;
		OriginalTimeInAirLabel = TimeInAirLabel.Text;
		OriginalMoveSpeedLabel = MoveSpeedLabel.Text;
		OriginalGroundSpeedCapLabel = GroundSpeedCapLabel.Text;
		OriginalFrictionLabel = FrictionLabel.Text;
		OriginalGravityLabel = GravityLabel.Text;
		OriginalJumpSpeedLabel = JumpSpeedLabel.Text;
		OriginalPosXLabel = PosXLabel.Text;
		OriginalPosYLabel = PosYLabel.Text;
		OriginalVelXLabel = VelXLabel.Text;
		OriginalVelYLabel = VelYLabel.Text;
	}

	public override void _Process(float delta)
	{
		JumpHeightLabel.Text = String.Format(OriginalJumpHeightLabel, player.JUMP_HEIGHT);
		TimeInAirLabel.Text = String.Format(OriginalTimeInAirLabel, player.TIME_IN_AIR);
		MoveSpeedLabel.Text = String.Format(OriginalMoveSpeedLabel, player.MOVE_SPEED);
		GroundSpeedCapLabel.Text = String.Format(OriginalGroundSpeedCapLabel, player.GROUND_SPEED_CAP);
		FrictionLabel.Text = String.Format(OriginalFrictionLabel, player.FRICTION);
		GravityLabel.Text = String.Format(OriginalGravityLabel, player.GRAVITY);
		JumpSpeedLabel.Text = String.Format(OriginalJumpSpeedLabel, player.JUMP_SPEED);
		PosXLabel.Text = String.Format(OriginalPosXLabel, player.Position.x);
		PosYLabel.Text = String.Format(OriginalPosYLabel, player.Position.y);
		VelXLabel.Text = String.Format(OriginalVelXLabel, player.velocity.x);
		VelYLabel.Text = String.Format(OriginalVelYLabel, player.velocity.y);
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey eventKey)
		{
			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key1 || eventKey.Scancode == (int)KeyList.Kp1))
			{
				currentProperty = CHOSEN_PROP.JUMP_HEIGHT;
			}
			
			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key2 || eventKey.Scancode == (int)KeyList.Kp2))
			{
				currentProperty = CHOSEN_PROP.TIME_IN_AIR;
			}
			
			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key3 || eventKey.Scancode == (int)KeyList.Kp3))
			{
				currentProperty = CHOSEN_PROP.MOVE_SPEED;
			}

			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key4 || eventKey.Scancode == (int)KeyList.Kp4))
			{
				currentProperty = CHOSEN_PROP.GROUND_SPEED_CAP;
			}

			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key5 || eventKey.Scancode == (int)KeyList.Kp5))
			{
				currentProperty = CHOSEN_PROP.FRICTION;
			}

			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Plus || eventKey.Scancode == (int)KeyList.KpAdd))
			{
				updateProperty(false);
			}

			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Minus || eventKey.Scancode == (int)KeyList.KpSubtract))
			{
				updateProperty(true);
			}
		}

		player.RecalcPhysics();
	}

	private void updateProperty(bool decrease)
	{
		int mult;
		if(decrease)
		{
			mult = -1;
		}

		else
		{
			mult = 1;
		}

		switch (currentProperty)
		{
			case CHOSEN_PROP.JUMP_HEIGHT:
				player.JUMP_HEIGHT += mult * 1;
				break;

			case CHOSEN_PROP.TIME_IN_AIR:
				player.TIME_IN_AIR += (float)mult * 0.05F;
				break;

			case CHOSEN_PROP.MOVE_SPEED:
				player.MOVE_SPEED += mult * 1;
				break;

			case CHOSEN_PROP.GROUND_SPEED_CAP:
				player.GROUND_SPEED_CAP += mult * 5;
				break;

			case CHOSEN_PROP.FRICTION:
				player.FRICTION += mult * 1;
				break;

			default:
				GD.Print("Error!!");
				break;
		}
	}
}
