using Godot;
using System;

public partial class TurfClickHandler : Node3D
{
    public bool ClickInput(Camera3D camera, InputEvent evnt, Vector3 pos, StaticBody3D collider)
    {
        if(Multiplayer == null) return false;
        if(evnt is InputEventMouseButton mouse_button)
        {
            bool click = false;
            Godot.Collections.Dictionary new_inputs = TOOLS.AssembleStandardClick(pos);
            if(mouse_button.ButtonIndex == MouseButton.Left)
            {
                new_inputs["button"] = (int)MouseButton.Left;
                new_inputs["state"] = mouse_button.Pressed;
                click = true;
            }
            if(mouse_button.ButtonIndex == MouseButton.Right)
            {
                new_inputs["button"] = (int)MouseButton.Right;
                new_inputs["state"] = mouse_button.Pressed;
                click = true;
            }
            if(click) NetworkClient.peer_active_client.ClientTurfClick(Json.Stringify(new_inputs));
            return true;
        }
        return false;
    }
}
