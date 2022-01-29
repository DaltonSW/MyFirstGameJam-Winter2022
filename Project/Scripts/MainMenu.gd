extends Control

func _ready():
	$StartButton.grab_focus()

func _on_StartButton_pressed():
	get_tree().change_scene("res://Scenes/LevelHolder.tscn")

func _on_QuitButton_pressed():
	get_tree().quit()
