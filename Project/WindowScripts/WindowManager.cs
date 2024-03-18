using Godot;
using System;

[GlobalClass]
public partial class WindowManager : Node
{
    public static WindowManager controller;
    public override void _EnterTree()
    {
        controller = this;
    }

    [Export]
    public Window main_window;
    [Export]
    public JoinWindow join_window;
    [Export]
    public ChatWindow chat_window;


    public enum WindowStates
    {
        JoinMenu,
        MainGame,
        ServerConfig
    }

    public void SetGameWindowConfig(WindowStates state)
    {
        switch(state)
        {
            case WindowStates.JoinMenu:
                join_window.Show();
                main_window.Hide();
                chat_window.Hide();
            break;

            case WindowStates.MainGame:
                join_window.Hide();
                main_window.Show();
                chat_window.Show();
            break;

            case WindowStates.ServerConfig:
                join_window.Hide();
                main_window.Hide();
                chat_window.Hide();
            break;
        }
    }
}
