extends Control

signal animation_started(line)
signal accept_pressed()
signal next_line_requested(next_line_id)
signal animation_finished(line)


const Line = preload("res://addons/saywhat_godot/dialogue_line.gd")
const Response = preload("res://addons/saywhat_godot/dialogue_response.gd")


func hide():
	modulate.a = 0
	
	
func show():
	modulate.a = 1
	

func _ready():
	hide()
	
	
func _input(_event):
	if Input.is_action_just_pressed("ui_accept"):
		emit_signal("accept_pressed")
	

func display_line(line: Line) -> void:
	yield(display_main_dialogue(line), "completed")
	
	# If we have responses then show them
	if line.responses.size() > 1:
		populate_responses(line.responses)
	
	else:
		yield(self, "accept_pressed")
		signal_next_line(line.next_id)
		

# async
func display_main_dialogue(line: Line):
	$SpeakerLabel.bbcode_text = line.character + ":" if line.character != null else "???:"
	$BodyLabel.bbcode_text = "[fill]" + line.dialogue + "[/fill]"
	$ControlHint.visible = false;
	yield(get_tree(), "idle_frame")
	
	show()
	
	emit_signal("animation_started", line)
	$Tween.interpolate_property($BodyLabel, "percent_visible", 0, 1, $BodyLabel.get_total_character_count() * 0.05)
	$Tween.start()
	yield($Tween, "tween_all_completed")
	emit_signal("animation_finished", line)
	$ControlHint.visible = true;
	
	
func populate_responses(responses: Array):
	for response in responses:
		var button = create_response_button(response)
		$Responses.add_child(button)
	$Button.queue_free() # Remove the default button
	
	
func create_response_button(response: Response):
	var new_button = $Button.duplicate()
	new_button.text = response.prompt
	new_button.connect("gui_input", self, "_button_gui_input", [response.next_id])
	return new_button
	

func _button_gui_input(event: InputEventMouseButton, next_line_id: String) -> void:
	if event != null:
		signal_next_line(next_line_id)

	
func signal_next_line(next_line_id: String) -> void:
	emit_signal("next_line_requested", next_line_id)
	queue_free()

