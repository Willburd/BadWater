extends Node
class_name BootController

@export var server_forced : bool = false
@export var entity_container : Node
@export var client_container : Node

@export var client_prefab : PackedScene
@export var client_spawner : MultiplayerSpawner
@export var entity_spawner : MultiplayerSpawner

@export var join_address : String = "localhost"
var max_players : int = -1		# Set from the ClientSpawner's data
var max_entities : int = -1		# Set from the EntitySpawners's data
var asset_library : AssetLoader

var config : ConfigData

@export var join_menu : CanvasLayer # TEMP

# Called when the node enters the scene tree for the first time.
func _ready():
	# Load config
	config = ConfigData.new()
	config.Load("res://Config/Setup.json")
	# Load asset library
	asset_library = AssetLoader.new()
	asset_library.Load()
	# Spawn server from launch options
	var args = Array(OS.get_cmdline_args())
	if args.has("-s") || args.has("-e") || args.has("--headless"):
		StartNetwork(true,args.has("-e"))
		join_menu.hide()

func StartNetwork(server: bool, edit_mode: bool) -> void:
	print("Start networking")
	var peer = ENetMultiplayerPeer.new()
	if(server):
		# Start controller
		var server_scene: MainController = preload("res://Scenes/Server.tscn").instantiate()
		add_child.call_deferred(server_scene)
		server_scene.Init(edit_mode);
		server_scene.entity_container = entity_container
		server_scene.config = config
		client_spawner.set_spawn_limit(config.max_clients)
		entity_spawner.set_spawn_limit(config.max_entities)
		# Set limits
		max_players = client_spawner.get_spawn_limit()
		max_entities = entity_spawner.get_spawn_limit()
		# Link signals
		multiplayer.peer_connected.connect(self._PeerJoin)
		multiplayer.peer_disconnected.connect(self._PeerLeave)
		# Create godot network server
		peer.create_server(config.port,max_players)
	else:
		# Create godot client connection to server
		peer.create_client(join_address,config.port)
	multiplayer.set_multiplayer_peer(peer)
	
func _PeerJoin(id: int):
	print(str("Peer join: ",id))
	var c : NetworkClient = client_prefab.instantiate()
	c.name = str(id)
	c.Spawn(c.name)
	client_container.add_child(c)

func _PeerLeave(id: int):
	print(str("Peer Leave: ",id))
	var c : NetworkClient = client_container.get_node(str(id))
	c.Kill()
	c.queue_free()

# TEMPS
func _on_client_pressed():
	join_menu.hide()
	StartNetwork(false,false)

func _on_server_pressed():
	join_menu.hide()
	StartNetwork(true,false)
	
func _on_editor_pressed():
	join_menu.hide()
	StartNetwork(true,true)
