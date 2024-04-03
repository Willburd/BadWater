using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;


public struct PackRef
{
    public PackRef(PackData data,MainController.DataType set_data_type)
    {
        modid = data.GetUniqueModID;
        data_type = set_data_type;
    }
    public string modid;
    public MainController.DataType data_type;
}

public static class ShaderConfig
{
    public enum Library
    {
        Main,
        AboveAll,
        Last
    }
    public static string[] lib = new string[(int)Library.Last]
    {
        "res://Materials/Main.tres",
        "res://Materials/NoticeEffects.tres"
    };
}

[GlobalClass] 
public partial class AssetLoader : Node
{
    public const int tex_page_size = 1024;
    // Assets
    public static Dictionary<string,AssetLoader.LoadedTexture> loaded_textures = new Dictionary<string,AssetLoader.LoadedTexture>();
    public static ShaderMaterial[][] material_cache = new ShaderMaterial[(int)ShaderConfig.Library.Last][]; // dictionary of shader IDs, with shadermaterials stored inside, each entry is a texture page assigned to that material.
    public static Dictionary<string,List<string>> loaded_sounds = new Dictionary<string,List<string>>();
    public static Dictionary<string,PackedScene> loaded_models = new Dictionary<string,PackedScene>();
    public static Dictionary<string,MapData> loaded_maps = new Dictionary<string,MapData>();
    public static Dictionary<string,AreaData> loaded_areas = new Dictionary<string,AreaData>();
    public static Dictionary<string,TurfData> loaded_turfs = new Dictionary<string,TurfData>();
    public static Dictionary<string,EffectData> loaded_effects = new Dictionary<string,EffectData>();
    public static Dictionary<string,PackData> loaded_items = new Dictionary<string,PackData>();
    public static Dictionary<string,PackData> loaded_structures = new Dictionary<string,PackData>();
    public static Dictionary<string,PackData> loaded_machines = new Dictionary<string,PackData>();
    public static Dictionary<string,PackData> loaded_mobs = new Dictionary<string,PackData>();
    public static Dictionary<string,PackData> all_packs = new Dictionary<string,PackData>();

    public void Load()
    {
        ChatController.AssetLog("LOADING ASSETS");
        string sound_path = "res://Library/Sounds";
        string model_path = "res://Library/Models";
        string texture_path = "res://Library/Textures";
        string map_path = "res://Library/Maps";
        string area_path = "res://Library/Areas";
        string turf_path = "res://Library/Turfs";
        string struct_path = "res://Library/Struct";
        string item_path = "res://Library/Items";
        string effect_path = "res://Library/Effects";
        string mob_path = "res://Library/Mobs";

        ChatController.AssetLog("-SOUND");
        DirAccess dir;
        Stack<string> scan_dirs = new Stack<string>();
        scan_dirs.Push(sound_path);
        while(scan_dirs.Count > 0)
        {
            string scanName = scan_dirs.Pop();
            dir = DirAccess.Open(scanName);
            if (dir != null)
            {
                // Add directory for random sound selection
                loaded_sounds.Add(dir.GetCurrentDir(),new List<string>()); // Each of these lists stores all audio files inside the folder we are currently scanning. For random picking!
                
                int found_sounds = 0;
                dir.ListDirBegin();
                string fileName = dir.GetNext();
                while (fileName != "")
                {
                    if(!dir.CurrentIsDir())
                    {
                        if(fileName.EndsWith("ogg.import"))
                        {
                            found_sounds += 1;
                            loaded_sounds[dir.GetCurrentDir()].Add(fileName.Replace("ogg.import", "ogg"));
                        }
                    }
                    
                    else
                    {
                        scan_dirs.Push(dir.GetCurrentDir() + "/" + fileName);
                    }
                    fileName = dir.GetNext();
                }

                if(found_sounds > 0) ChatController.AssetLog("--AUDIOPACK: " + dir.GetCurrentDir() + " : " + found_sounds);
            }
        }
        scan_dirs.Clear();



        ChatController.AssetLog("-MODELS");
        scan_dirs = new Stack<string>();
        scan_dirs.Push(model_path);
        while(scan_dirs.Count > 0)
        {
            string scanName = scan_dirs.Pop();
            dir = DirAccess.Open(scanName);
            if (dir != null)
            {
                // Add directory for random sound selection
                dir.ListDirBegin();
                string fileName = dir.GetNext();
                while (fileName != "")
                {
                    if(!dir.CurrentIsDir())
                    {
                        if(fileName.EndsWith("tscn") || fileName.EndsWith("tscn.remap"))
                        {   
                            string path = dir.GetCurrentDir() + "/" + fileName.Replace("tscn.remap", "tscn");
                            ChatController.AssetLog("--MODEL: " + path);
                            loaded_models.Add(path, (PackedScene)GD.Load(path));
                        }
                    }
                    else
                    {
                        scan_dirs.Push(dir.GetCurrentDir() + "/" + fileName);
                    }
                    fileName = dir.GetNext();
                }
            }
        }
        scan_dirs.Clear();



        ChatController.AssetLog("-TEXTURES");
        scan_dirs = new Stack<string>();
        scan_dirs.Push(texture_path);
        while(scan_dirs.Count > 0)
        {
            string scanName = scan_dirs.Pop();
            dir = DirAccess.Open(scanName);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();
                while (fileName != "")
                {
                    if(!dir.CurrentIsDir())
                    {
                        if(fileName.EndsWith("png.import"))
                        {
                            LoadTextureAtlas(dir.GetCurrentDir() + "/" + fileName.Replace("png.import", "png"));
                        }
                    }
                    
                    else
                    {
                        scan_dirs.Push(dir.GetCurrentDir() + "/" + fileName);
                    }
                    fileName = dir.GetNext();
                }
            }
        }
        ConstructTextureAtlas();
        scan_dirs.Clear();


        // Load main library entities
        ChatController.AssetLog("BUILDING LIBRARY DATA");
        LoadLibraryWithType( map_path, MainController.DataType.Map);
        LoadLibraryWithType( area_path, MainController.DataType.Area);
        LoadLibraryWithType( turf_path, MainController.DataType.Turf);
        LoadLibraryWithType( struct_path, MainController.DataType.Structure);
        LoadLibraryWithType( item_path, MainController.DataType.Item);
        LoadLibraryWithType( effect_path, MainController.DataType.Effect);
        LoadLibraryWithType( mob_path, MainController.DataType.Mob);


        // Construct dependant entities
        ChatController.AssetLog("BUILDING SECONDARY DATA");
        // TODO - circuitboards etc 


        // Load internal REQUIRED types
        ChatController.AssetLog("FINALIZING DATA");
        ChatController.AssetLog("-PREFABS");
        {   // Base area
            AreaData area = new AreaData();
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data["name"] = "Unknown";
            data["is_space"] = 1.0;
            data["always_powered"] = 1.0;
            area.Init( "_", "_", "_", MainController.DataType.Area, data);
            loaded_areas[area.GetUniqueModID] = area;
            all_packs[PackTypeID(area.GetUniqueModID, MainController.DataType.Area)] = area;
        }
        {   // Space turf
            TurfData turf = new TurfData();
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data["name"] = "Space";
            data["density"] = 0.0;
            data["opaque"] = 0.0;
            turf.Init( "_", "_", "_", MainController.DataType.Turf, data);
            loaded_turfs[turf.GetUniqueModID] = turf;
            all_packs[PackTypeID(turf.GetUniqueModID, MainController.DataType.Turf)] = turf;
        }
        
        ChatController.AssetLog("BUILDING INHERITANCE");
        BuildInheritance(loaded_maps.Values.ToList<PackData>()); // Could be possible, but why would you?
        BuildInheritance(loaded_areas.Values.ToList<PackData>());
        BuildInheritance(loaded_turfs.Values.ToList<PackData>());
        BuildInheritance(loaded_effects.Values.ToList<PackData>());
        BuildInheritance(loaded_items.Values.ToList<PackData>());
        BuildInheritance(loaded_structures.Values.ToList<PackData>());
        BuildInheritance(loaded_machines.Values.ToList<PackData>());
        BuildInheritance(loaded_mobs.Values.ToList<PackData>());

        ChatController.AssetLog("CLEANUP");
        foreach( PackData data in all_packs.Values )
        {
            data.ClearTempData();
            data.ShowVars();
        }
    }

    public void BuildInheritance(List<PackData> assemble)
    {
        List<PackData> next_assemble = new List<PackData>();
        while(assemble.Count > 0)
        {
            foreach(PackData data in assemble)
            {
                // Check if this is a base type. If so it'll already have it's parent flag set! Skip...
                if(data.ParentFlag)
                {
                    data.SetVars();
                    data.ShowVars();
                    continue;
                }
                // Check to see if the parent of the current data has it's parentflag set, if so it's ready for inheretance.
                string getID = PackTypeID(data.GetDataParent,data.entity_type);
                PackData parent = all_packs[getID];
                if(!parent.ParentFlag)
                {
                    // Parent is not yet ready...
                    next_assemble.Add(data); // lets wait for a future loop...
                    continue;
                }
                // Parent has parentflag set, so we can inheret its data, loop through parent chain, and set data repeatedly from topmost parent to us...
                Stack<string> parent_chain = new Stack<string>();
                string parent_search = PackTypeID(data.GetUniqueModID,data.entity_type);   
                while(true)
                {
                    PackData search_parent = all_packs[parent_search];
                    parent_chain.Push(parent_search);
                    if(search_parent.GetDataParent == "") break;
                    parent_search = PackTypeID(search_parent.GetDataParent,data.entity_type);
                }
                while(parent_chain.Count > 0)
                {
                    PackData parent_step = all_packs[parent_chain.Pop()];
                    data.SetVars(parent_step.GetTempData()); // apply from parent
                }
                data.ParentFlag = true; // we're set!
                data.ShowVars();
            }
            // Next iteration
            assemble = next_assemble;
            next_assemble = new List<PackData>();
        }
    }



    private void LoadLibraryWithType(string dir_path, MainController.DataType type)
    {
        ChatController.AssetLog("-" + type.ToString().ToUpper());
        DirAccess base_dir = DirAccess.Open(dir_path);
        if (base_dir != null)
        {
            // Scan each mod directory
            base_dir.ListDirBegin();
            string dirName = base_dir.GetNext();
            while (dirName != "")
            {
                if(base_dir.CurrentIsDir())
                {
                    // Then scan each json file in each mod directory! 
                    DirAccess dir = DirAccess.Open(dir_path + "/" + dirName);
                    if(dir != null)
                    {
                        dir.ListDirBegin();
                        string fileName = dir.GetNext();
                        while (fileName != "" && fileName.GetExtension() == "json")
                        {
                            if (!dir.CurrentIsDir())
                            {
                                ParseData(dir.GetCurrentDir() + "/" + fileName, type, fileName.ToUpper().Replace(".JSON", ""), dirName.ToUpper());
                            }
                            fileName = dir.GetNext();
                        }
                    }
                }
                dirName = base_dir.GetNext();
            }
        }
    }

    private void ParseData(string file_path, MainController.DataType type, string key, string prefix)
    {
        Godot.Collections.Dictionary dict_data = JsonHandler.ParseJsonFile(file_path);
        if(dict_data == null || dict_data.Keys.Count <= 0) 
        {
            ChatController.AssetLog("--Invalid or empty asset json in " + file_path);
            return;
        }

        switch(type)
        {
            case MainController.DataType.Map:
                {
                    MapData data_pack = new MapData();
                    data_pack.Init( file_path, prefix, key, type, dict_data);
                    loaded_maps[data_pack.GetUniqueModID] = data_pack;
                    all_packs[PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    ChatController.AssetLog("--Loaded: " + PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type));
                }
            break;

            case MainController.DataType.Area:
                {
                    AreaData data_pack = new AreaData();
                    data_pack.Init( file_path, prefix, key, type, dict_data);
                    loaded_areas[data_pack.GetUniqueModID] = data_pack;
                    all_packs[PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    ChatController.AssetLog("--Loaded: " + PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type));
                }
            break;

            case MainController.DataType.Turf:
                {
                    TurfData data_pack = new TurfData();
                    data_pack.Init( file_path, prefix, key, type, dict_data);
                    loaded_turfs[data_pack.GetUniqueModID] = data_pack;
                    all_packs[PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    ChatController.AssetLog("--Loaded: " + PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type));
                }
            break;

            case MainController.DataType.Effect:
                {
                    EffectData data_pack = new EffectData();
                    data_pack.Init( file_path, prefix, key, type, dict_data);
                    loaded_effects[data_pack.GetUniqueModID] = data_pack;
                    all_packs[PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    ChatController.AssetLog("--Loaded: " + PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type));
                }       
            break;

            case MainController.DataType.Item:
                {
                    ItemData data_pack = new ItemData();
                    data_pack.Init( file_path, prefix, key, type, dict_data);
                    loaded_items[data_pack.GetUniqueModID] = data_pack;
                    all_packs[PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    ChatController.AssetLog("--Loaded: " + PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type));
                }
            break;
            
            case MainController.DataType.Structure:
                {
                    PackData data_pack = new StructureData();
                    data_pack.Init( file_path, prefix, key, type, dict_data);
                    loaded_structures[data_pack.GetUniqueModID] = data_pack;
                    all_packs[PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    ChatController.AssetLog("--Loaded: " + PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type));
                }
            break;
            
            case MainController.DataType.Machine:
                {
                    PackData data_pack = new MachineData();
                    data_pack.Init( file_path, prefix, key, type, dict_data);
                    loaded_machines[data_pack.GetUniqueModID] = data_pack;
                    all_packs[PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    ChatController.AssetLog("--Loaded: " + PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type));
                }
            break;
            
            case MainController.DataType.Mob:
                {
                    MobData data_pack = new MobData();
                    data_pack.Init( file_path, prefix, key, type, dict_data);
                    loaded_mobs[data_pack.GetUniqueModID] = data_pack;
                    all_packs[PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    ChatController.AssetLog("--Loaded: " + PackTypeID(data_pack.GetUniqueModID,data_pack.entity_type));
                }
            break;
        }
    }


    public readonly struct LoadedTexture
    {
        public LoadedTexture(string set_path, int set_tex_page, int set_u, int set_v, int set_width, int set_height)
        {
            path = set_path;
            tex_page = set_tex_page;
            u = set_u;
            v = set_v;
            width = set_width;
            height = set_height;
        }
        public readonly string path;
        public readonly int tex_page;
        public readonly int u;
        public readonly int v;
        public readonly int width;
        public readonly int height;
    }
    public struct PreparingTexture
    {
        public PreparingTexture(string set_path, int set_width, int set_height)
        {
            path = set_path;
            width = set_width;
            height = set_height;
        }
        public readonly string path;
        public int width;
        public int height;
    }

    public static List<Image> texture_pages = new List<Image>();
    private double tex_offset_stacker = 0;
    private TupleList<double,PreparingTexture> prepare_textures = new TupleList<double,PreparingTexture>();
    
    private void LoadTextureAtlas(string path)
    {
        ChatController.AssetLog("--TEXTURE: " + path);
        Texture2D tex = (Texture2D)ResourceLoader.Load(path);
        int size = tex.GetWidth() * tex.GetHeight();
        prepare_textures.Add(size + tex_offset_stacker,new PreparingTexture(path,tex.GetWidth(),tex.GetHeight()));
    }
    private void ConstructTextureAtlas()
    {
        int tex_page_ind = 0; // incriments when we max out a page!
        List<LoadedTexture> placed_on_page = new List<LoadedTexture>();
        prepare_textures.ReverseSort();
        foreach(var prep_tex in prepare_textures)
        {
            // Check each location 32x32 as a base block... Attempt to place the texture!
            Vector2I place_pos = Vector2I.Zero;
            bool placeable = false;
            int place_size = Mathf.FloorToInt(Mathf.Min(prep_tex.Item2.width,prep_tex.Item2.height));
            while(!placeable)
            {
                if(texture_pages.Count <= tex_page_ind) 
                {
                    Image new_page = Image.Create(tex_page_size,tex_page_size,false,Image.Format.Rgba8);
                    texture_pages.Add(new_page);
                }
                placeable = true;
                foreach(LoadedTexture check_tex in placed_on_page)
                {
                    Rect2I new_rect = new Rect2I(place_pos.X,place_pos.Y,prep_tex.Item2.width,prep_tex.Item2.height);
                    Rect2I check_rect = new Rect2I(check_tex.u,check_tex.v,check_tex.width,check_tex.height);
                    if(new_rect.Intersects(check_rect))
                    {
                        placeable = false;
                        place_pos.X += place_size;
                        if(place_pos.X + prep_tex.Item2.width > tex_page_size)
                        {
                            place_pos.X = 0;
                            place_pos.Y += place_size;
                            if(place_pos.Y + prep_tex.Item2.height > tex_page_size)
                            {
                                // Failover to next page!
                                place_pos.X = 0;
                                place_pos.Y = 0;
                                tex_page_ind += 1;
                                placed_on_page.Clear();
                            }
                        }
                        break;
                    }
                }
                // If the spot is clear of any other texture on the page!
                if(placeable)
                {
                    Image tex_page = texture_pages[tex_page_ind];
                    // Blit image to page
                    LoadedTexture loaded_tex_data = new LoadedTexture(prep_tex.Item2.path,tex_page_ind,place_pos.X,place_pos.Y,prep_tex.Item2.width,prep_tex.Item2.height); 
                    ChatController.AssetLog("-" + tex_page_ind + ">" + loaded_tex_data.u + "-" + loaded_tex_data.v + "(" + loaded_tex_data.width + "-" + loaded_tex_data.height + "): " + loaded_tex_data.path);
                    // Handle formatting
                    Image blit_img = ((Texture2D)ResourceLoader.Load(loaded_tex_data.path)).GetImage();
                    blit_img.Decompress(); // If format was compressed, decompress it...
                    blit_img.Convert(Image.Format.Rgba8);
                    // Blit image
                    tex_page.BlitRect(blit_img, new Rect2I(0,0,loaded_tex_data.width,loaded_tex_data.height), new Vector2I(loaded_tex_data.u,loaded_tex_data.v));
                    loaded_textures[prep_tex.Item2.path] = loaded_tex_data;
                    placed_on_page.Add(loaded_tex_data);
                    texture_pages[tex_page_ind] = tex_page;
                    break;
                }
            }
        }
        if(texture_pages.Count > 0)
        {
            ChatController.AssetLog("-Creating texture pages. Count: " + texture_pages.Count);
            // Create an entry for each sahder path
            for(int s = 0; s < (int)ShaderConfig.Library.Last; s++) 
            {
                // Create material cache for each page!
                material_cache[s] = new ShaderMaterial[tex_page_ind+1];
                for(int i = 0; i < texture_pages.Count; i++) 
                {
                    material_cache[s][i] = (ShaderMaterial)ResourceLoader.Load(ShaderConfig.lib[s]).Duplicate(true);
                    material_cache[s][i].SetShaderParameter( "_MainTexture", ImageTexture.CreateFromImage(AssetLoader.texture_pages[i]));
                }
            }
            if(OS.HasFeature("editor")) 
            {
                for(int i = 0; i < texture_pages.Count; i++) 
                {
                    texture_pages[i].SavePng("res://Export/Page_" + i + ".png");
                }
            }
        }
        else
        {
            ChatController.AssetLog("============TEXTURE PAGE ERROR, NO PAGES============");
        }
    }






    public static PackData GetPackFromRef(PackRef get_pack)
    {
        return all_packs[PackTypeID(get_pack.modid,get_pack.data_type)];
    }


    public static string PackTypeID( string modID, MainController.DataType type)
    {
        return type.ToString().ToUpper() + ":" + modID;
    }
}
