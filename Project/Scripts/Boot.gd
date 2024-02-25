extends Node
class_name BootController

@export var server_forced : bool = false
@export var entity_container : Node
@export var client_container : Node

@export var client_prefab : PackedScene
@export var client_spawner : MultiplayerSpawner
@export var entity_spawner : MultiplayerSpawner

@export var join_address : String = "localhost"
@export var port : int	= 2532
var max_players : int = -1		# Set from the ClientSpawner's data
var max_entities : int = -1		# Set from the EntitySpawners's data

# Called when the node enters the scene tree for the first time.
func _ready():
	var args = Array(OS.get_cmdline_args())
	if server_forced || args.has("-s"):
		var server_scene = preload("res://Scenes/Server.tscn").instantiate()
		add_child.call_deferred(server_scene)
		StartNetwork(true)
	if !args.has("--headless"):
		StartNetwork(false)

func StartNetwork(server: bool) -> void:
	var peer = ENetMultiplayerPeer.new()
	if(server):
		multiplayer.peer_connected.connect(self.PeerJoin)
		multiplayer.peer_disconnected.connect(self.PeerLeave)
		max_players = client_spawner.get_spawn_limit()
		max_entities = entity_spawner.get_spawn_limit()
		peer.create_server(port,max_players)
		print(str("Server Init, Port: ",port," Max players: ",max_players))
	else:
		peer.create_client(join_address,port)
		print(str("Client Init, Address: ",join_address," : ",port))
	multiplayer.set_multiplayer_peer(peer)
	
func PeerJoin(id : int):
	var p = client_prefab.instantiate()
	p.name = id
	print(str("Client join: ",id))
	client_container.add_child(p)

func PeerLeave(id : int):
	print(str("Client Leave: ",id))
	client_container.get_node(str(id)).queue_free()
