using Godot;
using System;
using System.Collections.Generic;

public class DebugLevel : Node2D
{
	private PackedScene playerScene;
	private Player player;
	private Position2D SpawnPoint;

	private PackedScene labeledSpinner;

	private VBoxContainer spinnerContainer;
	private List<LabeledSpinner> spinners;
	private List<Action<float>> spinnerSetters;

	private List<(Label, string, Func<object>)> propertyLabels;

	public override void _Ready()
	{
		labeledSpinner = GD.Load<PackedScene>("res://Scenes/LabeledSpinner.tscn");
		playerScene    = GD.Load<PackedScene>("res://Scenes/Player.tscn");
		player = (Player)playerScene.Instance();
		SpawnPoint = GetNode<Position2D>("SpawnPoint");
		player.Position = SpawnPoint.Position;
		AddChild(player);

		spinners = new List<LabeledSpinner>();
		spinnerSetters = new List<Action<float>>();
		propertyLabels = new List<(Label, string, Func<object>)>();
		spinnerContainer = GetNode<VBoxContainer>("UI/SpinnerContainer");

		AddSpinner(v => player.JUMP_HEIGHT = v,      "Jump Height",      player.JUMP_HEIGHT);
		AddSpinner(v => player.TIME_IN_AIR = v,      "Time in Air",      player.TIME_IN_AIR,      0.05f);
		AddSpinner(v => player.MOVE_SPEED  = v,      "Move Speed",       player.MOVE_SPEED);
		AddSpinner(v => player.GROUND_SPEED_CAP = v, "Ground Speed Cap", player.GROUND_SPEED_CAP, 5);
		AddSpinner(v => player.FRICTION = v,         "Friction",         player.FRICTION);

		AddPropertyLabel("Gravity",    () => player.GRAVITY);
		AddPropertyLabel("Jump Speed", () => player.JUMP_SPEED);
		AddPropertyLabel("X Position", () => player.Position.x);
		AddPropertyLabel("Y Position", () => player.Position.y);
		AddPropertyLabel("X Velocity", () => player.velocity.x);
		AddPropertyLabel("Y Velocity", () => player.velocity.y);
	}

	private void AddPropertyLabel(string propertyName, Func<object> getter)
	{
		var label = new Label();
		propertyLabels.Add((label, propertyName, getter));
		GetNode<Control>("UI/LabelGroup").AddChild(label);
	}

	private void AddSpinner(Action<float> action, string label = "", float initialValue = 0, float stepSize = 1)
	{
		var spinner = labeledSpinner.Instance<LabeledSpinner>();
		var spinnerIndex = spinners.Count;
		spinners.Add(spinner);
		spinnerSetters.Add(action);
		spinner.ConfigureAndConnectValueChangedSignal(this, nameof(SetProperty), new object[] { spinnerIndex }, $"({spinnerIndex + 1}) {label}", initialValue, stepSize);
		spinnerContainer.AddChild(spinner);
	}

	private void SetProperty(float value, int spinnerSetterIndex)
	{
		spinnerSetters[spinnerSetterIndex].Invoke(value);
		StealFocusFromControls();
	} 

	public override void _Process(float delta)
	{
		foreach ((Label label, string propertyName, Func<object> getter) in propertyLabels)
		{
			label.Text = propertyName + " = " + getter.Invoke();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			StealFocusFromControls();
		}
	}

	 // Hack to steal focus from spinners
	private void StealFocusFromControls()
	{
		GetNode<Control>("DummyControl").GrabFocus();
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		var numberPressed = inputEvent.GetNumberPressed();
		if (numberPressed is int numberPressedValue)
		{
			GD.Print(numberPressedValue);
			var spinnerIndex = numberPressedValue - 1;
			spinners[spinnerIndex].GrabFocusOnSpinner();
		}

		player.RecalcPhysics();
	}

}

public static class Extensions
{
	public static int? GetNumberPressed(this InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey eventKey)
		{
			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key1 || eventKey.Scancode == (int)KeyList.Kp1))
			{
				return 1;
			}

			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key2 || eventKey.Scancode == (int)KeyList.Kp2))
			{
				return 2;
			}

			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key3 || eventKey.Scancode == (int)KeyList.Kp3))
			{
				return 3;
			}

			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key4 || eventKey.Scancode == (int)KeyList.Kp4))
			{
				return 4;
			}

			if (eventKey.Pressed && (eventKey.Scancode == (int)KeyList.Key5 || eventKey.Scancode == (int)KeyList.Kp5))
			{
				return 5;
			}
		}
		return null;
	}
}
