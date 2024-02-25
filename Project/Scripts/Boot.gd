extends Node
class_name BootController

@export var server_forced : bool = false

# Called when the node enters the scene tree for the first time.
func _ready():
	var args = Array(OS.get_cmdline_args())
	if server_forced || args.has("-s"):
		print("starting server...")
		var server_scene = preload("res://Scenes/Server.tscn").instantiate()
		get_tree().root.add_child.call_deferred(server_scene)
	if !args.has("--headless"):
		print("starting client...")
		var client_scene = preload("res://Scenes/Client.tscn").instantiate()
		get_tree().root.add_child.call_deferred(client_scene)
