using Godot;
using System;
using System.Collections.Generic;

public partial class AudioController : DeligateController
{
    public const float screen_range = 6;
    public const float short_range = 3;

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


    public static void PlayAt(string soundpack_id, MapController.GridPos pos, float range, float volume_mod, NetworkClient for_client = null)
    {
        if(soundpack_id == "") return;
        string soundpack = "res://Library/Sounds/" + soundpack_id;
        // If it has a key it's playable!
        if(AssetLoader.loaded_sounds.ContainsKey(soundpack))
        {
            List<string> packcontents = AssetLoader.loaded_sounds[soundpack];
            int rand = TOOLS.RandI(packcontents.Count);
            string path = packcontents[rand];
            if(for_client != null)
            {
                // Directly send to client
                float zlevel_dist = Mathf.Abs(pos.WorldPos().Z - for_client.focused_position.Z) * -5; // -5 per Z level!
                for_client.PlaySoundAt(soundpack + "/" + path,pos.WorldPos(),range,volume_mod + zlevel_dist);
            }
            else
            {
                // Scan for clients around us to hear
                for(int i = 0; i < MainController.controller.client_container.GetChildCount(); i++) 
                {
                    NetworkClient client = (NetworkClient)MainController.controller.client_container.GetChild(i);
                    if(MapController.OnSameMap(client.focused_map_id,pos.GetMapID()) && MapController.GetMapDistance(pos.WorldPos(), client.focused_position) < range)
                    {
                        float zlevel_dist = Mathf.Abs(pos.WorldPos().Z - client.focused_position.Z) * -5; // -5 per Z level!
                        client.PlaySoundAt(soundpack + "/" + path,pos.WorldPos(),range,volume_mod + zlevel_dist);
                    }
                }
            }
        }
    }

    public static void Loop()
    {
        
    }
}
