extends StaticBody2D

signal player_interaction_requested

# Declare member variables here. Examples:
# var a = 2
# var b = "text"

func interact_with_player():
	emit_signal("player_interaction_requested")

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func make_angry():
	$Sprite.animation = "angry"

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
