using Godot;
using System;

[GlobalClass]
public partial class JoinWindow : GameWindows
{
    [Export]
    public Button button_client;
    [Export]
    public Button button_server;
    [Export]
    public Button button_edit;

    [Export]
    public LineEdit ip_entry;
    [Export]
    public LineEdit port_entry;
    [Export]
    public LineEdit pass_entry;

    [Export]
    public LineEdit account_entry;
    [Export]
    public LineEdit accpass_entry;

    public override void _Ready()
    {
        base._Ready();
        // Signal connect
        button_client.Pressed   += _on_client_pressed;
        button_server.Pressed   += _on_server_pressed;
        button_edit.Pressed     += _on_editor_pressed;
        // Show it!
        WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.JoinMenu);
    }

    public void _on_client_pressed()
    {
        if(account_entry.Text.Length <= 0) return;
        WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.MainGame);
        WindowManager.controller.main_window.GrabFocus(); // demand attention
        BootController.controller.StartNetwork(false,false);
        DisplayServer.WindowSetTitle("Badwater - Client");
    }

    public void _on_server_pressed()
    {
        WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.ServerConfig);
        BootController.controller.StartNetwork(true,false);
        DisplayServer.WindowSetTitle("Badwater - Server");
    }
        
    public void _on_editor_pressed()
    {
        WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.ServerConfig);
        BootController.controller.StartNetwork(true,true);
        DisplayServer.WindowSetTitle("Badwater - Editing");
    }
}
