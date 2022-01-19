extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var DialogueBox = preload("res://Scenes/Dialogue Box.tscn")

# Called when the node enters the scene tree for the first time.
func _ready():
	var dialogue = preload("res://Resources/Dialogue/dialogue_test.tres")
	show_dialogue("Test 1", dialogue)
	pass # Replace with function body.


func show_dialogue(line_id: String, dialogue: DialogueResource):
	var line = yield(DialogueManager.get_next_dialogue_line(line_id, dialogue), "completed")
	if line == null:
		return
	var dialogue_box = DialogueBox.instance()
	$UI.add_child(dialogue_box)
	dialogue_box.display_line(line)
	var next_line_id = yield(dialogue_box, "next_line_requested")
	yield(get_tree().create_timer(.1), "timeout")
	show_dialogue(next_line_id, dialogue)
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
