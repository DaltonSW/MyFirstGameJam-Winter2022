using Godot;
using System;

public class LabeledSpinner : HBoxContainer
{
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

}
