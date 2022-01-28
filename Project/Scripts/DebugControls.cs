using Godot;
using System;
using System.Collections.Generic;

/**
 *  Script for a scene which adds controls and labels for editing and showing Player properties.
 *  You must correctly initialize this scene in (at least) one of 3 ways:
 *  1. Instance the scene as a sibling of a Player node called "Player."
 *  2. Instance the scene using the helper factory method Constructors.InstanceDebugControls.
 *     This method instances the scene and requires that you pass the Player node to it.
 *  3. Instance the scene, then call DebugControls.Initialize to pass it a reference to the Player node.
 *  
 *  The key is that the scene must have a reference to a Player node before _Ready is called.
 *  There may be other ways to achieve this, but just be warned that it will throw a NullPointerException if you don't.
 */
public class DebugControls : Control
{
	[Export]
	private bool IN_LEVEL_EDITOR = false;
	
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

		String playerPath = IN_LEVEL_EDITOR ? "/root/LevelEditor/Level/PlayerNode/Player" : "../PlayerNode/Player";

		player = GetNode<Player>(playerPath);

		spinners = new List<LabeledSpinner>();
		spinnerSetters = new List<Action<float>>();
		propertyLabels = new List<(Label, string, Func<object>)>();
		spinnerContainer = GetNode<VBoxContainer>("UI/SpinnerContainer");

		AddSpinner(v => player.JUMP_HEIGHT = v,         "Jump Height",          player.JUMP_HEIGHT);
		AddSpinner(v => player.TIME_IN_AIR = v,         "Time in Air",          player.TIME_IN_AIR,      0.05f);
		AddSpinner(v => player.MOVE_SPEED  = v,         "Move Speed",           player.MOVE_SPEED);
		AddSpinner(v => player.GROUND_SPEED_CAP = v,    "Ground Speed Cap",     player.GROUND_SPEED_CAP, 5);
		AddSpinner(v => player.FRICTION = v,            "Friction",             player.FRICTION);
		AddSpinner(v => player.BASE_WALL_JUMP_AWAY = v, "Wall Jump - Away",     player.BASE_WALL_JUMP_AWAY);
		AddSpinner(v => player.WALL_JUMP_SCALE = v,     "Wall Jump Away Scale", player.WALL_JUMP_SCALE,  0.02f);
		AddSpinner(v => player.SLIDE_DISTANCE = v,      "Slide Distance",       player.SLIDE_DISTANCE,   0.02f);
		AddSpinner(v => player.SLIDE_SPEED = v,         "Slide Speed",          player.SLIDE_SPEED,      0.02f);
		AddSpinner(v => player.DASH_DISTANCE = v,       "Dash Distance",        player.DASH_DISTANCE,    0.02f);
		AddSpinner(v => player.DASH_SPEED = v,          "Dash Speed",           player.DASH_SPEED,       0.02f);

		AddPropertyLabel("Gravity",    () => player.GRAVITY);
		AddPropertyLabel("Jump Speed", () => player.JUMP_SPEED);
		AddPropertyLabel("X Position", () => player.Position.x);
		AddPropertyLabel("Y Position", () => player.Position.y);
		AddPropertyLabel("X Velocity", () => player.velocity.x);
		AddPropertyLabel("Y Velocity", () => player.velocity.y);
	}

	public void Initialize(Player player)
	{
		this.player = player;
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
		spinner.ConfigureAndConnectValueChangedSignal(this, nameof(SetProperty), new object[] { spinnerIndex }, label, initialValue, stepSize);
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
			label.Text = string.Format("{0} = {1:F}", propertyName, getter.Invoke());
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
		player.RecalcPhysics();
	}

	public void _on_DebugControls_mouse_entered()
	{
		if (Global.inLevelEditor)
		{
			EditorObject editorObject = GetNode<EditorObject>("/root/LevelEditor/EditorObject");
			editorObject.currentMouseSprite.Texture = null;
			editorObject.canPlace = false;
			editorObject.isPlacingPlayer = false;
			editorObject.isPlacingTile = false;
		}
	}

	public void _on_DebugControls_mouse_exited()
	{
		if (Global.inLevelEditor)
		{
			EditorObject editorObject = GetNode<EditorObject>("/root/LevelEditor/EditorObject");
			editorObject.canPlace = true;
		}
	}

}

public static class Constructors
{
	public static DebugControls InstanceDebugControls(PackedScene debugControlsScene, Player player)
	{
		DebugControls debugControls = debugControlsScene.Instance<DebugControls>();
		debugControls.Initialize(player);
		return debugControls;
	}
}
