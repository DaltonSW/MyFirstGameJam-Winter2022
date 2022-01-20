extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var DialogueBox = preload("res://Scenes/Dialogue Box.tscn")

var dialogue = preload("res://Resources/Dialogue/dialogue_test.tres")

var coin_collected = false

# Called when the node enters the scene tree for the first time.
func _ready():
	pass

func show_dialogue(line_id: String):
	var line = yield(DialogueManager.get_next_dialogue_line(line_id, dialogue), "completed")
	if line == null:
		$Node2D/Player.interacting = false;
		return
	var dialogue_box = DialogueBox.instance()
	$UI.add_child(dialogue_box)
	dialogue_box.display_line(line)
	var next_line_id = yield(dialogue_box, "next_line_requested")
	yield(get_tree().create_timer(.1), "timeout")
	show_dialogue(next_line_id)
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

func _on_Cultist1_player_interaction_requested():
	show_dialogue("Test 1")


func _on_Cultist2_player_interaction_requested():
	show_dialogue("Test 2")
	

func make_cultist_angry():
	$Cultist2.make_angry()
	

func _on_Coin_collected():
	coin_collected = true
	$Coin.queue_free()
