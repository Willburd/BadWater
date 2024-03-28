using Behaviors_BASE;
using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;

public static class ChatController
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
        Debug,
        Asset
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
            case ChatMode.Asset:
                return "ASSET";
        }
        return "SPEAK";
    }

    public static bool ProcessClientSideCommand(string message)
    {
        // Perform commands
        if(message.Substr(0,1) == "/")
        {
            // Perform it!
            switch(message.ToUpper())
            {
                case "/MESH":
                    WindowManager.controller.chat_window.RecieveChatMessage("Mesh count : " + MeshUpdater.mesh_count);
                    return true;
            }
        }
        return false;
    }

    public static void ProcessServerCommand(string message)
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

    public static void AssetLog(string message)
    {
        SubmitMessage( null, null, message, ChatMode.Asset);
    }

    public static void DebugLog(string message)
    {
        SubmitMessage( null, null, message, ChatMode.Debug);
    }

    public static void LogAttack(string message)
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

    public static void RuneMessage(AbstractEntity origin_entity, string runemessage = "👁", float range = MapController.screen_visible_range)
    {
        // Show visible text above characters
        bool makeEffect = false;
        for(int i = 0; i < MainController.controller.client_container.GetChildCount(); i++) 
        {
            NetworkClient client = MainController.controller.client_container.GetChild(i) as NetworkClient;
            if(MapController.OnSameMap(client.focused_map_id,origin_entity.GridPos.GetMapID()) && MapController.GetMapDistance(client.focused_position,origin_entity.GridPos.WorldPos()) <= range )
            {
                // Create network effect entity for the text, don't worry about doing this for all clients, the effect does that itself. We're just checking if any client is in range!
                makeEffect = true;
                break;
            }
        }
        // Drop out if not in range of any clients...
        if(!makeEffect) return;
        // Make effect!
        AbstractEntity.CreateEntity(MainController.DataType.Effect,"BASE:RUNE_TEXT",origin_entity.GridPos, data_string:runemessage );
    }

    public static void ActionMessage(AbstractEntity speaking_ent, string self_message, string seen_message, string heard_message, VisibleMessageFormatting format = VisibleMessageFormatting.Nothing, List<AbstractEntity> excludes = null)
    {
        InspectMessage(speaking_ent, self_message, format);
        if(excludes == null) excludes = new List<AbstractEntity>();
        excludes.Add(speaking_ent);
        VisibleMessage(speaking_ent, seen_message, format, true, false, excludes);
        if(heard_message != null) VisibleMessage(speaking_ent, seen_message, format, false, true, excludes);
    }

    public static void VisibleMessage(AbstractEntity speaking_ent, string message, VisibleMessageFormatting format = VisibleMessageFormatting.Nothing, bool send_to_visible = true, bool send_to_hidden = false, List<AbstractEntity> excludes = null)
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
        RuneMessage(speaking_ent);
        SubmitMessage( null, speaking_ent, message, ChatMode.VisibleMessage, send_to_visible, send_to_hidden, excludes);
    }

    public static void SubmitMessage(NetworkClient client, AbstractEntity speaking_ent, string message, ChatMode mode, bool send_to_visible = true, bool send_to_hidden = false, List<AbstractEntity> excludes = null)
    {
        if(excludes == null) excludes = new List<AbstractEntity>();
        // SANITIZE

        // Assemble message
        string output = "";
        if(mode == ChatMode.Asset)
        {
            output += "[b][color=cyan]ASSET[/color][/b] " + message;
        }
        else if(mode == ChatMode.Debug || mode == ChatMode.AttackLog)
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
                        output += "[b][color=green]" + speaking_ent.display_name.The(true) + "[/color][/b] says : " + message;
                        RuneMessage(speaking_ent,message);
                    break;
                    case ChatMode.Whisper:
                        output += "[b][color=green]" + speaking_ent.display_name.The(true) + "[/color][/b] whispers : " + message;
                        RuneMessage(speaking_ent,message,2);
                    break;
                    case ChatMode.Emote:
                        output += "[b][color=green]" + speaking_ent.display_name.The(true) + "[/color][/b] " + message;
                        RuneMessage(speaking_ent,message);
                    break;
                    case ChatMode.Subtle:
                        output += "[b][color=green]" + speaking_ent.display_name.The(true) + "[/color][/b] " + message;
                        RuneMessage(speaking_ent,message,2);
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
        if(mode == ChatMode.Asset || mode == ChatMode.Debug || mode == ChatMode.AttackLog) return;

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
                if(speaking_ent != null && MapController.OnSameMap(scan_cli.focused_map_id,speaking_ent.GridPos.GetMapID()))
                {
                    if(scan_cli.GetFocusedEntity() != null && excludes.Contains(scan_cli.GetFocusedEntity())) continue; // IGNORE!

                    // Check visibility for send_to_visible and send_to_hidden (so sound only visiblemessages proc!)
                    bool is_visible = MapController.GetMapVisibility(speaking_ent.GridPos.WorldPos(),scan_cli.focused_position);
                    if(is_visible && !send_to_visible) continue;
                    if(!is_visible && !send_to_hidden) continue;

                    // Handle whisper range
                    bool in_small_range_limit = MapController.Adjacent( speaking_ent.GridPos.WorldPos(), scan_cli.focused_position,true);
                    switch(mode)
                    {
                        case ChatMode.Speak:
                            AudioController.PlayAt("BASE/Talksounds/Speak", speaking_ent.GridPos, AudioController.screen_range, -10, scan_cli);
                            scan_cli.BroadcastChatMessage(output);
                        break;
                        case ChatMode.Whisper:
                            if(in_small_range_limit) 
                            {
                                AudioController.PlayAt("BASE/Talksounds/Speak", speaking_ent.GridPos, AudioController.short_range, -20, scan_cli);
                                scan_cli.BroadcastChatMessage(output);
                            }
                        break;
                        case ChatMode.Emote:
                            AudioController.PlayAt("BASE/Talksounds/Emote", speaking_ent.GridPos, AudioController.screen_range, -10, scan_cli);
                            scan_cli.BroadcastChatMessage(output);
                        break;
                        case ChatMode.Subtle:
                            if(in_small_range_limit) 
                            {
                                AudioController.PlayAt("BASE/Talksounds/Subtle", speaking_ent.GridPos, AudioController.short_range, -20, scan_cli);
                                scan_cli.BroadcastChatMessage(output);
                            }
                        break;

                        default:
                            scan_cli.BroadcastChatMessage(output);
                        break;
                    }
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
        if(to_entity.GetClientOwner() != null) DirectMessage(to_entity.GetClientOwner(), message);
    }
    public static void DirectMessage(NetworkClient to_client, string message)
    {
        to_client.BroadcastChatMessage(message);
    }
}
