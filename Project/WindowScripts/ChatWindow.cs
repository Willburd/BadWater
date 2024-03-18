using Godot;
using System;

[GlobalClass]
public partial class ChatWindow : GameWindows
{
    [Export]
    public RichTextLabel chat_history;
    [Export]
    public LineEdit chat_entry;
    [Export]
    public Label chat_marker;

    public override void _Ready()
    {
        chat_entry.TextSubmitted += TextSubmit;
    }

    public static void ChatFocus(bool whisper, bool emoting)
    {
        WindowManager.controller.chat_window.GrabFocus();
        WindowManager.controller.chat_window.chat_entry.GrabFocus();
        if(emoting)
        {
            if(whisper)
            {
                WindowManager.controller.chat_window.chat_marker.Text = " SUBTLE: ";
            }
            else
            {
                WindowManager.controller.chat_window.chat_marker.Text = " EMOTE: ";
            }
        }
        else
        {
            if(whisper)
            {
                WindowManager.controller.chat_window.chat_marker.Text = " WHISPER: ";
            }
            else
            {
                WindowManager.controller.chat_window.chat_marker.Text = " SPEAK: ";
            }
        }
    }

    private void TextSubmit(string text)
    {
        // Bwoop
        if(chat_entry.Text.Length > 0)
        {
            if(chat_history.Text.Length > 0) chat_history.Text += "\n";
            chat_history.Text += chat_entry.Text;
        }
        
        // clear
        chat_entry.ReleaseFocus();
        WindowManager.controller.main_window.GrabFocus(); // Return to main window
        chat_entry.Text = "";
    }
}
