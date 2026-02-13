extends Control

func _ready() -> void:
	var play_button := get_node_or_null("CenterContainer/VBoxContainer/PlayButton") as Button
	var quit_button := get_node_or_null("CenterContainer/VBoxContainer/QuitButton") as Button

	#print("play_button =", play_button)
	#print("quit_button =", quit_button)

	if play_button:
		play_button.pressed.connect(_on_play_pressed)
	else:
		push_error("PlayButton not found")

	if quit_button:
		quit_button.pressed.connect(_on_quit_pressed)
	else:
		push_error("QuitButton not found")

func _on_play_pressed() -> void:
	#print("HELLOOO")
	get_tree().change_scene_to_file("res://game/entity/dungeon_generator/dungeon_generator.tscn")

func _on_quit_pressed() -> void:
	get_tree().quit()
