using Godot;
using System;

[GlobalClass]
public partial class LoggingWindow : GameWindows
{
    [Export]
    public RichTextLabel chat_history;
    [Export]
    public LineEdit chat_entry;

    public void ReloadLog()
    {
        chat_history.Text = "";
        foreach(string tx in ChatController.chat_log)
        {
            chat_history.Text += tx + "\n"; 
        }
    }

    public override void _Ready()
    {
        chat_entry.MaxLength = ChatController.chatmessage_max_length; // ensure max
        chat_entry.TextSubmitted += TextSubmit;
    }

    private void TextSubmit(string text)
    {
        // Bwoop
        chat_entry.MaxLength = ChatController.chatmessage_max_length; // ensure max
        if(chat_entry.Text.Length > 0) ChatController.ProcessServerCommand(chat_entry.Text);
        // clear
        chat_entry.Text = "";
    }
    public void RecieveLogMessage(string message)
    {
        // Get a new message from the server!
        chat_history.AppendText(message + "\n");
    }
}
