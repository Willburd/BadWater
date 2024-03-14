extends StaticBody3D
class_name ClickReciever

@export var mesh_updater : Node3D

func _on_input_event(camera, event, pos, normal, shape_idx):
	mesh_updater.ClickInput(camera, event, pos, normal, self, shape_idx)
