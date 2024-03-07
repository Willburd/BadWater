extends Node
class_name BootController

@export var entity_container : Node
@export var client_container : Node

@export var client_prefab : PackedScene
@export var client_spawner : MultiplayerSpawner
@export var entity_spawner : MultiplayerSpawner
@export var chunk_spawner : MultiplayerSpawner

@export var ip_entry : TextEdit
@export var port_entry : TextEdit
@export var pass_entry : TextEdit

@export var account_entry : TextEdit
@export var accpass_entry : TextEdit

var max_players : int = -1		# Set from the ClientSpawner's data
var max_entities : int = -1		# Set from the EntitySpawners's data
var max_chunks : int = -1		# Set from the EntitySpawners's data
var asset_library : AssetLoader

var config : ConfigData

@export var join_menu : CanvasLayer # TEMP

func _ready():
	var args = Array(OS.get_cmdline_args())
	# Load config
	config = ConfigData.new()
	config.Load("res://Config/Setup.json")
	# Load asset library
	asset_library = AssetLoader.new()
	asset_library.Load()
	# Spawn server from launch options
	if args.has("-s") || args.has("-e") || args.has("--headless"):
		StartNetwork(true,args.has("-e"))
		join_menu.hide()
	
# Called when the node enters the scene tree for the first time.
func _enter_tree():
	pass

func StartNetwork(server: bool, edit_mode: bool) -> void:
	print("Start networking")
	var peer = ENetMultiplayerPeer.new()
	if(server):
		# Start controller
		var server_scene: MainController = preload("res://Scenes/Server.tscn").instantiate()
		add_child.call_deferred(server_scene)
		server_scene.client_container = client_container
		server_scene.entity_container = entity_container
		server_scene.config = config
		server_scene.Init(edit_mode);
		client_spawner.set_spawn_limit(config.max_clients)
		entity_spawner.set_spawn_limit(config.max_entities)
		chunk_spawner.set_spawn_limit(config.max_chunks)
		# Set limits
		max_players = client_spawner.get_spawn_limit()
		max_entities = entity_spawner.get_spawn_limit()
		max_chunks = chunk_spawner.get_spawn_limit()
		# Link signals
		multiplayer.peer_connected.connect(self._PeerJoin)
		multiplayer.peer_disconnected.connect(self._PeerLeave)
		# Create godot network server
		peer.create_server(config.port,max_players)
	else:
		# Create godot client connection to server
		peer.create_client(ip_entry.text,port_entry.text.to_int())
	multiplayer.set_multiplayer_peer(peer)
	
func _PeerJoin(id: int):
	print(str("Peer join: ",id))
	var c : NetworkClient = client_prefab.instantiate()
	c.name = str(id)
	if !is_multiplayer_authority():
		return
	# Only run on server
	c.Init(account_entry.text,accpass_entry.text)
	
func _PeerLeave(id: int):
	print(str("Peer Leave: ",id))
	var c : NetworkClient = client_container.get_node(str(id))
	if c != null && c.multiplayer.multiplayer_peer && is_multiplayer_authority():
		c.DisconnectClient()
	# Removal of the client is done in the NetworkClient it self!
	if is_multiplayer_authority(): # Only run on DC client
		return
	# Reset the client...
	while(entity_container.get_child_count() > 0):
		entity_container.get_child(0).queue_free();
	while(client_container.get_child_count() > 0):
		client_container.get_child(0).queue_free();
	print("LEFT GAME")
	join_menu.show();
	

# TEMPS
func _on_client_pressed():
	if account_entry.text.length() <= 0:
		return
	join_menu.hide()
	StartNetwork(false,false)

func _on_server_pressed():
	join_menu.hide()
	StartNetwork(true,false)
	
func _on_editor_pressed():
	join_menu.hide()
	StartNetwork(true,true)
