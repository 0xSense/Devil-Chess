extends Node2D





# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass

# Make sure this is sending to the chessboard scene when final build is being made
func _on_texture_button_pressed():
	PlayButtonPress.play()
	get_tree().change_scene_to_file("res://game_primary.tscn")

var music_bus = AudioServer.get_bus_index("Music")

func _on_toggle_music_pressed():
	MusicTogglePress.play()
	AudioServer.set_bus_mute(music_bus, not AudioServer.is_bus_mute(music_bus))
	
	
func _on_option_button_pressed():
	DifficultyButtonPress.play()
	


func _on_option_button_item_selected(index):
	DifficultySelected.play()
	



