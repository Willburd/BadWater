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
    public TextEdit ip_entry;
    [Export]
    public TextEdit port_entry;
    [Export]
    public TextEdit pass_entry;

    [Export]
    public TextEdit account_entry;
    [Export]
    public TextEdit accpass_entry;

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
        BootController.controller.StartNetwork(false,false);
    }

    public void _on_server_pressed()
    {
        WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.ServerConfig);
        BootController.controller.StartNetwork(true,false);
    }
        
    public void _on_editor_pressed()
    {
        WindowManager.controller.SetGameWindowConfig(WindowManager.WindowStates.ServerConfig);
        BootController.controller.StartNetwork(true,true);
    }
}
