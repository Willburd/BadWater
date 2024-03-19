using Godot;
using GodotPlugins.Game;
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
        Looc,
        Gooc,
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
            case ChatMode.Looc:
                return "LOOC";
            case ChatMode.Gooc:
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


    public  static void ServerCommand(string message)
    {
        // Perform commands
        if(message.Substr(0,1) == "/")
        {
            // Log it
            SubmitMessage( null, null, message, ChatMode.Debug);
            // Perform it!
        }
        else
        {
            // Announcements
            SubmitMessage( null, null, message, ChatMode.Admin);
        }
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
            output += "[b][color=orange]LOG[/color][/b] " + message;
        }
        else if(mode == ChatMode.Admin)
        {
            if(client == null && MainController.controller != null)
            {
                // We're the server
                output += "[b][color=red]SERVER[/color][/b] : " + message;
            }
            else
            {
                AccountController.Account acc = AccountController.ClientGetAccount(client);
                output += "[b][color=red]" + acc.id_name + "[/color][/b] : " + message;
            }
        }
        else if(mode == ChatMode.Looc || mode == ChatMode.Gooc)
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

        // Add to chat log and server logging
        chat_log.Add(output);
        GD.Print(output);

        // server side only debugging stops here
        if(mode == ChatMode.Speak || mode == ChatMode.Whisper || mode == ChatMode.Looc || mode == ChatMode.Gooc || mode == ChatMode.Debug || mode == ChatMode.AttackLog || mode == ChatMode.Admin)
        {
            WindowManager.controller.logging_window.RecieveLogMessage(output);
        }
        if(mode == ChatMode.Debug || mode == ChatMode.AttackLog) return;

        // Determine the clients that recieve the message!
        for(int i = 0; i < MainController.controller.client_container.GetChildCount(); i++) 
        {
            NetworkClient scan_cli = (NetworkClient)MainController.controller.client_container.GetChild(i);
            if(mode == ChatMode.Looc || mode == ChatMode.Gooc || mode == ChatMode.Admin)
            {
                scan_cli.BroadcastChatMessage(output);
            }
            else
            {
                // TODO Proper map adjacency, client visual distance, and any other status for sending messages to clients in visible range!
                if(scan_cli.focused_map_id == speaking_ent?.map_id_string)
                {
                    scan_cli.BroadcastChatMessage(output);
                }
            }
        }
    }

    // Send message to only one client
    public static void InspectMessage(AbstractEntity to_entity, string message, VisibleMessageFormatting format = VisibleMessageFormatting.Nothing)
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
        DirectMessage( to_entity, message);
    }

    public static void DirectMessage(AbstractEntity to_entity, string message)
    {
        // Determine the clients that recieve the message!
        for(int i = 0; i < MainController.controller.client_container.GetChildCount(); i++) 
        {
            NetworkClient scan_cli = (NetworkClient)MainController.controller.client_container.GetChild(i);
            if(scan_cli.GetFocusedEntity() == to_entity)
            {
                DirectMessage(scan_cli, message);
                break;
            }
        }
    }
    public static void DirectMessage(NetworkClient to_client, string message)
    {
        to_client.BroadcastChatMessage(message);
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
