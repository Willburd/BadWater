using Godot;
using System;
using System.Collections.Generic;

public partial class ChatController : DeligateController
{
    public const int chatmessage_max_length = 1024;

    public static List<string> chat_log = new List<string>();

    public enum ChatMode
    {
        Speak,
        Whisper,
        Emote,
        Subtle,
        Loooc,
        Goooc,
        Admin,
        VisibleMessage,
        AttackLog,
        Debug
    }
    public static string ModePrefix(ChatMode mode)
    {
        switch(mode)
        {
            case ChatMode.Speak:
                return "SPEAK";
            case ChatMode.Whisper:
                return "WHISPER";
            case ChatMode.Emote:
                return "EMOTE";
            case ChatMode.Subtle:
                return "SUBTLE";
            case ChatMode.Loooc:
                return "LOOC";
            case ChatMode.Goooc:
                return "OOC";
            case ChatMode.Admin:
                return "ADMIN";
            case ChatMode.VisibleMessage:
                return "VIS";
            case ChatMode.AttackLog:
                return "ATTACK";
            case ChatMode.Debug:
                return "DEBUG";
        }
        return "SPEAK";
    }


    public  static void DebugLog(string message)
    {
        SubmitMessage( null, null, message, ChatMode.Debug);
    }

    public  static void LogAttack(string message)
    {
        SubmitMessage( null, null, message, ChatMode.AttackLog);
    }

    public enum VisibleMessageFormatting
    {
        Nothing,
        Notice,
        Warning,
        Danger
    }

    public static void VisibleMessage(AbstractEntity speaking_ent, string message, VisibleMessageFormatting format = VisibleMessageFormatting.Nothing)
    {
        switch(format)
        {
            case VisibleMessageFormatting.Notice:
                message = "[color=cyan]" + message + "[/color]";
                break;
            case VisibleMessageFormatting.Warning:
                message = "[color=orange]" + message + "[/color]";
                break;
            case VisibleMessageFormatting.Danger:
                message = "[color=red]" + message + "[/color]";
                break;
        }
        SubmitMessage( null, speaking_ent, message, ChatMode.VisibleMessage);
    }

    public  static void SubmitMessage(NetworkClient client, AbstractEntity speaking_ent, string message, ChatMode mode)
    {
        // SANITIZE

        // Assemble message
        string output = "";
        if(mode == ChatMode.Debug || mode == ChatMode.AttackLog)
        {
            output += message;
        }
        else if(mode == ChatMode.Admin)
        {
            AccountController.Account acc = AccountController.ClientGetAccount(client);
            output += "[b][color=red]" + acc.id_name + "[/color][/b] : " + message;
        }
        else if(mode == ChatMode.Loooc || mode == ChatMode.Goooc)
        {
            AccountController.Account acc = AccountController.ClientGetAccount(client);
            output += "[b][color=blue]" + acc.id_name + "[/color][/b] : " + message;
        }
        else
        {
            if(speaking_ent != null)
            {
                switch(mode)
                {
                    case ChatMode.Speak:
                        output += "[b][color=green]" + speaking_ent.display_name + "[/color][/b] says : " + message;
                    break;
                    case ChatMode.Whisper:
                        output += "[b][color=green]" + speaking_ent.display_name + "[/color][/b] whispers : " + message;
                    break;
                    case ChatMode.Emote:
                        output += "[b][color=green]" + speaking_ent.display_name + "[/color][/b] " + message;
                    break;
                    case ChatMode.Subtle:
                        output += "[b][color=green]" + speaking_ent.display_name + "[/color][/b] " + message;
                    break;
                    // DIRECT
                    case ChatMode.VisibleMessage:
                        output += message;
                    break;
                }
            }
            else
            {
                output += "[b][color=red]The Abyss[/color][/b] whispers : " + message;
            }
        }

        // Add to chat log
        chat_log.Add(output);
        GD.Print(output);

        // server side only debugging stops here
        if(mode == ChatMode.Debug) return;

        // Determine the clients that recieve the message!
        for(int i = 0; i < MainController.controller.client_container.GetChildCount(); i++) 
        {
            NetworkClient scan_cli = (NetworkClient)MainController.controller.client_container.GetChild(i);
            // TODO Proper map adjacency, client visual distance, and any other status for sending messages to clients in visible range!
            if(scan_cli.focused_map_id == speaking_ent?.map_id_string)
            {
                scan_cli.BroadcastChatMessage(output);
            }
        }
    }

    public override bool CanInit()
    {
        return true;
    }

    public override bool Init()
    {
        tick_rate = -1; // NO TICK
        controller = this;
        return true;
    }

    public override void SetupTick()
    {
        FinishInit();
    }

    public override void Fire()
    {
        //GD.Print(Name + " Fired");
    }

    public override void Shutdown()
    {
        
    }
}
