extends CanvasLayer

@export var controller : Node

func _ready():
	controller.join_menu.show()

func _on_client_pressed():
	if controller.account_entry.text.length() <= 0:
		return
	controller.join_menu.hide()
	controller.StartNetwork(false,false)

func _on_server_pressed():
	controller.join_menu.hide()
	controller.StartNetwork(true,false)
	
func _on_editor_pressed():
	controller.join_menu.hide()
	controller.StartNetwork(true,true)
