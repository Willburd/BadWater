using Godot;
using System;

namespace MapLoading
{
    public class MapOperator
    {
        protected MapController controller;
        protected MapContainer output_map;
        public int max_steps
        {
            get {return output_map.Depth * output_map.Width * output_map.Height;}
        } 

        protected string map_id;
        protected int steps = 0; // for logging
        protected int current_x = 0;
        protected int current_y = 0;
        protected int current_z = 0;

        protected virtual void HandleLoop()
        {
            // Next loop!
            steps += 1;
            current_x += 1;
            if(current_x >= output_map.Width)
            {
                current_x = 0;
                current_y += 1;
            }
            if(current_y >= output_map.Height)
            {
                current_y = 0;
                current_z += 1;
            }
            if(current_z >= output_map.Depth)
            {
                finished = true;
            }
            TOOLS.PrintProgress(steps,max_steps);
        }

        public virtual void Process()
        {
            // replace with controlled functions!
            HandleLoop();
        }

        protected bool finished = false;
        public bool Finished()
        {
            return finished;
        }


        public string GetMapID()
        {
            return map_id;
        }
        public MapContainer GetMap()
        {
            return output_map;
        }
    }

    public class MapLoader : MapOperator
    {
        Godot.Collections.Dictionary area_data;
        Godot.Collections.Dictionary turf_data;

        public MapLoader(MapController owner, string set_map_id,int set_width, int set_height,int set_depth)
        {
            controller = owner;
            map_id = set_map_id;
            MapData map_data = AssetLoader.loaded_maps[set_map_id];
            output_map = new MapContainer(set_map_id,set_width, set_height,set_depth);

            Godot.Collections.Dictionary map_list = JsonHandler.ParseJsonFile(map_data.GetFilePath);
            Godot.Collections.Dictionary map_json = (Godot.Collections.Dictionary)map_list[map_data.GetUniqueID];
            area_data = (Godot.Collections.Dictionary)map_json["area_data"];
            turf_data = (Godot.Collections.Dictionary)map_json["turf_data"];
            ChatController.DebugLog("LOADING MAP" + map_id + " =========================");
        }

        public override void Process()
        {
            if(finished) return;

            Godot.Collections.Dictionary area_depth = null;
            if(area_data.ContainsKey(current_z.ToString())) area_depth = (Godot.Collections.Dictionary)area_data[current_z.ToString()];
            Godot.Collections.Dictionary turf_depth = null;
            if(turf_data.ContainsKey(current_z.ToString())) turf_depth = (Godot.Collections.Dictionary)turf_data[current_z.ToString()];

            int repeats = 100;
            while(repeats-- > 0 && !finished)
            {
                string make_area_id = "_:_";
                string make_turf_id = "_:_";
                string turf_json = "";
                if(area_depth != null && turf_depth != null)
                {
                    // MUST not be an empty Z level, these should NEVER be invalid on a real map file. So it's probably an empty one... Paint a fresh map!
                    // Assume data will overrun the buffer, and provide dummy lists
                    string[] area_ylist = new string[]{"_:_"};
                    if(area_depth.ContainsKey(current_x.ToString())) 
                    {
                        area_ylist = area_depth[current_x.ToString()].AsStringArray();
                    }
                    Godot.Collections.Array<string[]> turf_ylist = new Godot.Collections.Array<string[]>{ new string[] { "_:_", "" }};
                    if(area_depth.ContainsKey(current_x.ToString())) 
                    {
                        turf_ylist = (Godot.Collections.Array<string[]>)turf_depth[current_x.ToString()]; // array of string[TurfID,CustomData]
                    }

                    if(current_y < area_ylist.Length)
                    {
                        make_area_id = area_ylist[current_y];
                    }
                    if(current_y < turf_ylist.Count)
                    {
                        string[] construct_strings = turf_ylist[current_y];
                        make_turf_id = construct_strings[0]; // Set ID
                        turf_json = construct_strings[1];
                    }
                }

                // It's turfin time... How awful.
                AbstractTurf turf = output_map.AddTurf(make_turf_id, new GridPos(map_id,current_x,current_y,current_z), controller.areas[make_area_id], false, false);
                if(turf_json.Length > 0) turf.ApplyMapCustomData(JsonHandler.ParseJson(turf_json)); // Set this object's flags using an embedded string of json!
                HandleLoop();
            }
        }
    }


    public class MapInitilizer : MapOperator
    {
        public MapInitilizer(MapController owner, MapContainer input_map)
        {
            controller = owner;
            map_id = input_map.MapID;
            output_map = input_map;
            ChatController.DebugLog("INITING MAP" + map_id + " =========================");
        }

        public override void Process()
        {
            int repeats = 200;
            while(repeats-- > 0 && !finished)
            {
                GetTurfAtPosition(map_id,current_x, current_y, current_z).Init();
                HandleLoop();
            }
        }

        public AbstractTurf GetTurfAtPosition(string mapid, int x, int y, int z)
        {
            return output_map.GetTurfAtPosition(new GridPos(mapid,x,y,z),false);
        }
    }


    public class MapLateInitilizer : MapOperator
    {
        public MapLateInitilizer(MapController owner, MapContainer input_map)
        {
            controller = owner;
            map_id = input_map.MapID;
            output_map = input_map;
            ChatController.DebugLog("UPDATING MAP" + map_id + " =========================");
        }

        public override void Process()
        {
            int repeats = 100;
            while(repeats-- > 0 && !finished)
            {
                AbstractTurf turf = GetTurfAtPosition(map_id,current_x, current_y, current_z);
                turf.LateInit();
                turf.UpdateIcon();
                HandleLoop();
            }
        }

        public AbstractTurf GetTurfAtPosition(string mapid,int x, int y, int z)
        {
            return output_map.GetTurfAtPosition(new GridPos(mapid,x,y,z),false);
        }
    }



    public class MapEntityCreator : MapOperator
    {
        Godot.Collections.Array<string[]> item_data;
        Godot.Collections.Array<string[]> effect_data;
        Godot.Collections.Array<string[]> structure_data;
        Godot.Collections.Array<string[]> machine_data;
        Godot.Collections.Array<string[]> mob_data;

        public new int max_steps
        {
            get {return item_data.Count + effect_data.Count + structure_data.Count + machine_data.Count + mob_data.Count;}
        } 


        int phase = 0;
        public MapEntityCreator(MapController owner, MapContainer input_map)
        {
            controller = owner;
            map_id = input_map.MapID;
            output_map = input_map;

            MapData map_data = AssetLoader.loaded_maps[map_id];
            Godot.Collections.Dictionary map_list = JsonHandler.ParseJsonFile(map_data.GetFilePath);
            Godot.Collections.Dictionary map_json = (Godot.Collections.Dictionary)map_list[map_data.GetUniqueID];
            item_data       = (Godot.Collections.Array<string[]>)map_json["items"];     // array of string[EntityID,X,Y,Z,CustomData]
            effect_data     = (Godot.Collections.Array<string[]>)map_json["effects"];   // array of string[EntityID,X,Y,Z,CustomData]
            structure_data  = (Godot.Collections.Array<string[]>)map_json["structures"];// array of string[EntityID,X,Y,Z,CustomData]
            machine_data    = (Godot.Collections.Array<string[]>)map_json["machines"];  // array of string[EntityID,X,Y,Z,CustomData]
            mob_data        = (Godot.Collections.Array<string[]>)map_json["mobs"];      // array of string[EntityID,X,Y,Z,CustomData]
            ChatController.DebugLog("CREATING ENTITIES " + map_id + " =========================");
        }

        public override void Process()
        {
            int repeats = 10;
            while(repeats-- > 0 && !finished)
            {
                // Get entity data!
                string[] entity_pack = null;
                AbstractEntity ent = null;
                switch(phase)
                {
                    case 0: // Item
                        if(item_data.Count > 0)
                        {
                            entity_pack = item_data[current_x];
                            ent = AbstractTools.CreateEntity(MainController.DataType.Item,entity_pack[0],new GridPos(map_id,float.Parse(entity_pack[1]),float.Parse(entity_pack[2]),float.Parse(entity_pack[3])), true);
                            if(entity_pack[4].Length > 0) ent.ApplyMapCustomData(JsonHandler.ParseJson(entity_pack[4])); // Set this object's flags using an embedded string of json!
                        }
                    break;
                    case 1: // Effect
                        if(effect_data.Count > 0)
                        {
                            entity_pack = effect_data[current_x];
                            ent = AbstractTools.CreateEntity(MainController.DataType.Effect,entity_pack[0],new GridPos(map_id,float.Parse(entity_pack[1]),float.Parse(entity_pack[2]),float.Parse(entity_pack[3])), true);
                            if(entity_pack[4].Length > 0) ent.ApplyMapCustomData(JsonHandler.ParseJson(entity_pack[4])); // Set this object's flags using an embedded string of json!
                        }
                    break;
                    case 2: // Structure
                        if(structure_data.Count > 0)
                        {
                            entity_pack = structure_data[current_x];
                            ent = AbstractTools.CreateEntity(MainController.DataType.Structure,entity_pack[0],new GridPos(map_id,float.Parse(entity_pack[1]),float.Parse(entity_pack[2]),float.Parse(entity_pack[3])), true);
                            if(entity_pack[4].Length > 0) ent.ApplyMapCustomData(JsonHandler.ParseJson(entity_pack[4])); // Set this object's flags using an embedded string of json!
                        }
                    break;
                    case 3: // Machine
                        if(machine_data.Count > 0)
                        {
                            entity_pack = machine_data[current_x];
                            ent = AbstractTools.CreateEntity(MainController.DataType.Machine,entity_pack[0],new GridPos(map_id,float.Parse(entity_pack[1]),float.Parse(entity_pack[2]),float.Parse(entity_pack[3])), true);
                            if(entity_pack[4].Length > 0) ent.ApplyMapCustomData(JsonHandler.ParseJson(entity_pack[4])); // Set this object's flags using an embedded string of json!
                        }
                    break;
                    case 4: // Mobs
                        if(mob_data.Count > 0)
                        {
                            entity_pack = mob_data[current_x];
                            ent = AbstractTools.CreateEntity(MainController.DataType.Mob,entity_pack[0],new GridPos(map_id,float.Parse(entity_pack[1]),float.Parse(entity_pack[2]),float.Parse(entity_pack[3])), true);
                            if(entity_pack[4].Length > 0) ent.ApplyMapCustomData(JsonHandler.ParseJson(entity_pack[4])); // Set this object's flags using an embedded string of json!
                        }
                    break;
                }
                // LOOP!
                HandleLoop();
            }
        }
        protected override void HandleLoop()
        {
            // Next loop!
            steps += 1;
            current_x += 1;
            switch(phase)
            {
                case 0: // Item
                    if(current_x >= item_data.Count)
                    {
                        current_x = 0;
                        phase += 1;
                    }
                break;

                case 1: // Effect
                    if(current_x >= effect_data.Count)
                    {
                        current_x = 0;
                        phase += 1;
                    }
                break;

                case 2: // Structure
                    if(current_x >= structure_data.Count)
                    {
                        current_x = 0;
                        phase += 1;
                    }
                break;

                case 3: // Machine
                    if(current_x >= machine_data.Count)
                    {
                        current_x = 0;
                        phase += 1;
                    }
                break;

                case 4: // Mobs
                    if(current_x >= mob_data.Count)
                    {
                        current_x = 0;
                        phase += 1;
                        finished = true;
                    }
                break;
            }
            TOOLS.PrintProgress(steps,max_steps);
        }
    }
}