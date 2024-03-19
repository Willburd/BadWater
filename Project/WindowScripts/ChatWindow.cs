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
    public Button chat_marker;

    public override void _Ready()
    {
        chat_entry.MaxLength = ChatController.chatmessage_max_length; // ensure max
        chat_entry.TextSubmitted += TextSubmit;
        chat_marker.Pressed += CycleMode;
        SetMode(ChatController.ChatMode.Speak);
    }


    public static ChatController.ChatMode chat_mode;

    public static void ChatFocus(bool whisper, bool emoting, bool ooc)
    {
        ChatController.ChatMode mode = ChatController.ChatMode.Speak;
        WindowManager.controller.chat_window.GrabFocus();
        WindowManager.controller.chat_window.chat_entry.GrabFocus();
        if(ooc)
        {
            if(whisper)
            {
                mode = ChatController.ChatMode.Looc;
            }
            else
            {
                mode = ChatController.ChatMode.Gooc;
            }
        }
        else if(emoting)
        {
            if(whisper)
            {
                mode = ChatController.ChatMode.Subtle;
            }
            else
            {
                mode = ChatController.ChatMode.Emote;
            }
        }
        else
        {
            if(whisper)
            {
                mode = ChatController.ChatMode.Whisper;
            }
            else
            {
                mode = ChatController.ChatMode.Speak;
            }
        }
        // Update chat mode
        WindowManager.controller.chat_window.SetMode(mode);
    }

    public void CycleMode()
    {
        chat_mode += 1;
        if(chat_mode > ChatController.ChatMode.Gooc) chat_mode = ChatController.ChatMode.Speak;
        SetMode(chat_mode);
    }

    public void SetMode(ChatController.ChatMode mode)
    {
        chat_mode = mode;
        WindowManager.controller.chat_window.chat_marker.Text = " " + ChatController.ModePrefix(chat_mode) + ": ";
    }

    private void TextSubmit(string text)
    {
        // Bwoop
        chat_entry.MaxLength = ChatController.chatmessage_max_length; // ensure max
        if(chat_entry.Text.Length > 0) NetworkClient.peer_active_client.SendChatMessage( chat_entry.Text, chat_mode);
        // clear
        chat_entry.ReleaseFocus();
        WindowManager.controller.main_window.GrabFocus(); // Return to main window
        chat_entry.Text = "";
    }

    public void RecieveChatMessage(string message)
    {
        // Get a new message from the server!
        chat_history.AppendText(message + "\n");
    }
}
