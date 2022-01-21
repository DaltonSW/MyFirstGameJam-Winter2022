using Godot;
using System;

public class LabeledSpinner : HBoxContainer
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}


	public void GrabFocusOnSpinner()
	{
		GetNode<SpinBox>("SpinBox").GetLineEdit().GrabFocus();
	}

	public void ConfigureAndConnectValueChangedSignal(
		Godot.Object target, string method, object[] binds,
		string text = "",
		float value = 0,
		float stepSize = 1)
	{
		GetNode<Label>("Label").Text = text;

		var spinBox = GetNode<SpinBox>("SpinBox");
		spinBox.Step = stepSize;
		spinBox.Value = value;
		var godotBinds = new Godot.Collections.Array();
		foreach (var bind in binds)
		{
			godotBinds.Add(bind);
		}
		spinBox.Connect("value_changed", target, method, godotBinds);
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
