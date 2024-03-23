using Godot;
using System;

public partial class TurfClickHandler : Node3D
{
    public bool ClickInput(Camera3D camera, InputEvent evnt, Vector3 pos, StaticBody3D collider)
    {
        if(Multiplayer == null) return false;
        if(evnt is InputEventMouseButton mouse_button)
        {
            Godot.Collections.Dictionary new_inputs = TOOLS.AssembleStandardClick(pos);
            new_inputs["button"] = (int)mouse_button.ButtonIndex;
            new_inputs["state"] = mouse_button.Pressed;
            NetworkClient.peer_active_client.ClientTurfClick(Json.Stringify(new_inputs));
            return true;
        }
        return false;
    }
}
