extends Node2D





# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_texture_button_pressed():
	get_tree().change_scene_to_file("res://chess_board.tscn")

var music_bus = AudioServer.get_bus_index("Music")

func _on_toggle_music_pressed():
	AudioServer.set_bus_mute(music_bus, not AudioServer.is_bus_mute(music_bus))
