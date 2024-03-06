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

[GlobalClass] 
public partial class AssetLoader : Node
{
    public const int tex_page_size = 512;

    public static Dictionary<string,AssetLoader.LoadedTexture> loaded_textures = new Dictionary<string,AssetLoader.LoadedTexture>();
    public static ShaderMaterial[] material_cache;
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
        GD.Print("LOADING ASSETS");
        string texture_path = "res://Library/Textures";
        string map_path = "res://Library/Maps";
        string area_path = "res://Library/Areas";
        string turf_path = "res://Library/Turfs";
        string struct_path = "res://Library/Struct";
        string item_path = "res://Library/Items";
        string effect_path = "res://Library/Effects";
        string mob_path = "res://Library/Mobs";

        GD.Print("-TEXTURES");
        DirAccess dir;
        Stack<string> scan_dirs = new Stack<string>();
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
                    if(!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".png")
                    {
                        LoadTextureAtlas(dir.GetCurrentDir() + "/" + fileName);
                    }
                    else if(dir.CurrentIsDir())
                    {
                        scan_dirs.Push(dir.GetCurrentDir() + "/" + fileName);
                    }
                    fileName = dir.GetNext();
                }
            }
        }
        ConstructTextureAtlas();


        GD.Print("-MAPS");
        dir = DirAccess.Open(map_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    GD.Print("-" + fileName);
                    ParseData(dir.GetCurrentDir() + "/" + fileName, MainController.DataType.Map);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-AREAS");
        dir = DirAccess.Open(area_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    GD.Print("-" + fileName);
                    ParseData(dir.GetCurrentDir() + "/" + fileName, MainController.DataType.Area);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-TURFS");
        dir = DirAccess.Open(turf_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    GD.Print("-" + fileName);
                    ParseData(dir.GetCurrentDir() + "/" + fileName, MainController.DataType.Turf);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-STRUCTURES");
        dir = DirAccess.Open(struct_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    GD.Print("-" + fileName);
                    ParseData(dir.GetCurrentDir() + "/" + fileName, MainController.DataType.Structure);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-ITEMS");
        dir = DirAccess.Open(item_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    GD.Print("-" + fileName);
                    ParseData(dir.GetCurrentDir() + "/" + fileName, MainController.DataType.Item);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-EFFECTS");
        dir = DirAccess.Open(effect_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    GD.Print("-" + fileName);
                    ParseData(dir.GetCurrentDir() + "/" + fileName, MainController.DataType.Effect);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-MOBS");
        dir = DirAccess.Open(mob_path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                
                if (!dir.CurrentIsDir() && Path.HasExtension(fileName) && Path.GetExtension(fileName) == ".json")
                {
                    GD.Print("-" + fileName);
                    ParseData(dir.GetCurrentDir() + "/" + fileName, MainController.DataType.Mob);
                }
                fileName = dir.GetNext();
            }
        }

        GD.Print("-PREFABS");
        {   // Base area
            AreaData area = new AreaData();
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data["name"] = "Unknown";
            data["is_space"] = 1.0;
            data["always_powered"] = 1.0;
            area.Init( "_", "_", "_", MainController.DataType.Area, data);
            loaded_areas[area.GetUniqueModID] = area;
            all_packs[AssetLoader.AllPackID(area.GetUniqueModID, MainController.DataType.Area)] = area;
        }
        {   // Space turf
            TurfData turf = new TurfData();
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data["name"] = "Space";
            data["density"] = 0.0;
            data["opaque"] = 0.0;
            turf.Init( "_", "_", "_", MainController.DataType.Turf, data);
            loaded_turfs[turf.GetUniqueModID] = turf;
            all_packs[AssetLoader.AllPackID(turf.GetUniqueModID, MainController.DataType.Turf)] = turf;
        }
        
        GD.Print("BUILDING INHERITANCE");
        BuildInheritance(loaded_maps.Values.ToList<PackData>()); // Could be possible, but why would you?
        BuildInheritance(loaded_areas.Values.ToList<PackData>());
        BuildInheritance(loaded_turfs.Values.ToList<PackData>());
        BuildInheritance(loaded_effects.Values.ToList<PackData>());
        BuildInheritance(loaded_items.Values.ToList<PackData>());
        BuildInheritance(loaded_structures.Values.ToList<PackData>());
        BuildInheritance(loaded_machines.Values.ToList<PackData>());
        BuildInheritance(loaded_mobs.Values.ToList<PackData>());

        GD.Print("CLEANUP");
        foreach( PackData data in all_packs.Values )
        {
            data.ClearTempData();
            data.ShowVars();
        }
    }

    public void BuildInheritance(List<PackData> todo)
    {
        List<PackData> next_todo = new List<PackData>();
        while(todo.Count > 0)
        {
            foreach(PackData data in todo)
            {
                // Check if this is a base type. It'll already have it's parent flag set!
                if(data.ParentFlag)
                {
                    data.SetVars();
                    data.ShowVars();
                    continue;
                }
                // IT HAS A PARENT, so time for GREAT FUN.
                string getID = AllPackID(data.GetDataParent,data.entity_type);
                PackData parent = all_packs[getID];
                if(!parent.ParentFlag)
                {
                    // Parent is not yet ready...
                    next_todo.Add(data); // lets wait for a future loop...
                    continue;
                }
                // Parent's data is set, loop through parent chain, and set data repeatedly...
                Stack<string> parent_chain = new Stack<string>();
                string parent_search = AllPackID(data.GetUniqueModID,data.entity_type);   
                while(true)
                {
                    PackData search_parent = all_packs[parent_search];
                    parent_chain.Push(parent_search);
                    if(search_parent.GetDataParent == "") break;
                    parent_search = AllPackID(search_parent.GetDataParent,data.entity_type);
                }
                // Go through all collected parents from the lowest to ourselves, and set values!
                while(parent_chain.Count > 0)
                {
                    PackData parent_step = all_packs[parent_chain.Pop()];
                    data.SetVars(parent_step.GetTempData()); // apply from parent
                }
                data.ParentFlag = true; // we're set!
                data.ShowVars();
            }
            // Next iteration
            todo = next_todo;
            next_todo = new List<PackData>();
        }
    }

    private void ParseData(string file_path, MainController.DataType type)
    {
        Godot.Collections.Dictionary data = TOOLS.ParseJsonFile(file_path);
        string prefix = Path.GetFileNameWithoutExtension(file_path);
        foreach( string key in data.Keys )
        {
            Godot.Collections.Dictionary dict_data = (Godot.Collections.Dictionary)data[key];
            switch(type)
            {
                case MainController.DataType.Map:
                    {
                        MapData data_pack = new MapData();
                        data_pack.Init( file_path, prefix, key, type, dict_data);
                        loaded_maps[data_pack.GetUniqueModID] = data_pack;
                        all_packs[AllPackID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    }
                break;

                case MainController.DataType.Area:
                    {
                        AreaData data_pack = new AreaData();
                        data_pack.Init( file_path, prefix, key, type, dict_data);
                        loaded_areas[data_pack.GetUniqueModID] = data_pack;
                        all_packs[AllPackID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    }
                break;

                case MainController.DataType.Turf:
                    {
                        TurfData data_pack = new TurfData();
                        data_pack.Init( file_path, prefix, key, type, dict_data);
                        loaded_turfs[data_pack.GetUniqueModID] = data_pack;
                        all_packs[AllPackID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    }
                break;

                case MainController.DataType.Effect:
                    {
                        EffectData data_pack = new EffectData();
                        data_pack.Init( file_path, prefix, key, type, dict_data);
                        loaded_effects[data_pack.GetUniqueModID] = data_pack;
                        all_packs[AllPackID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    }       
                break;

                case MainController.DataType.Item:
                    {
                        ItemData data_pack = new ItemData();
                        data_pack.Init( file_path, prefix, key, type, dict_data);
                        loaded_items[data_pack.GetUniqueModID] = data_pack;
                        all_packs[AllPackID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    }
                break;
                
                case MainController.DataType.Structure:
                    {
                        PackData data_pack = new PackData();
                        data_pack.Init( file_path, prefix, key, type, dict_data);
                        loaded_structures[data_pack.GetUniqueModID] = data_pack;
                        all_packs[AllPackID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    }
                break;
                
                case MainController.DataType.Machine:
                    {
                        PackData data_pack = new PackData();
                        data_pack.Init( file_path, prefix, key, type, dict_data);
                        loaded_machines[data_pack.GetUniqueModID] = data_pack;
                        all_packs[AllPackID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    }
                break;
                
                case MainController.DataType.Mob:
                    {
                        PackData data_pack = new PackData();
                        data_pack.Init( file_path, prefix, key, type, dict_data);
                        loaded_mobs[data_pack.GetUniqueModID] = data_pack;
                        all_packs[AllPackID(data_pack.GetUniqueModID,data_pack.entity_type)] = data_pack;
                    }
                break;
            }
        }
    }

    private void ParseMob(string file_path)
    {
        GD.Print(file_path);
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
    private SortedList<double,PreparingTexture> prepare_textures = new SortedList<double,PreparingTexture>();
    
    private void LoadTextureAtlas(string path)
    {
        Texture2D img = (Texture2D)GD.Load(path);
        int size = img.GetWidth() * img.GetHeight();
        prepare_textures.Add(size + tex_offset_stacker,new PreparingTexture(path,img.GetWidth(),img.GetHeight()));
        tex_offset_stacker += 0.0000001; // TODO - figure out a better way
    }
    private void ConstructTextureAtlas()
    {
        int tex_page_ind = 0; // incriments when we max out a page!
        int place_size = 32; // block size, powers of 2 please!
        List<LoadedTexture> placed_on_page = new List<LoadedTexture>();
        foreach(PreparingTexture prep_tex in prepare_textures.Values)
        {
            // Check each location 32x32 as a base block... Attempt to place the texture!
            Vector2I place_pos = Vector2I.Zero;
            bool placeable = false;
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
                    Rect2I new_rect = new Rect2I(place_pos.X,place_pos.Y,prep_tex.width,prep_tex.height);
                    Rect2I check_rect = new Rect2I(check_tex.u,check_tex.v,check_tex.width,check_tex.height);
                    if(new_rect.Intersects(check_rect))
                    {
                        placeable = false;
                        place_pos.X += place_size;
                        if(place_pos.X + prep_tex.width > tex_page_size)
                        {
                            place_pos.X = 0;
                            place_pos.Y += place_size;
                            if(place_pos.Y + prep_tex.height > tex_page_size)
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
                    LoadedTexture loaded_tex_data = new LoadedTexture(prep_tex.path,tex_page_ind,place_pos.X,place_pos.Y,prep_tex.width,prep_tex.height); 
                    GD.Print("-" + tex_page_ind + ">" + loaded_tex_data.u + "-" + loaded_tex_data.v + "(" + loaded_tex_data.width + "-" + loaded_tex_data.height + "): " + loaded_tex_data.path);
                    // Handle formatting
                    Image blit_img = ((Texture2D)GD.Load(prep_tex.path)).GetImage();
                    blit_img.Decompress(); // If format was compressed, decompress it...
                    blit_img.Convert(Image.Format.Rgba8);
                    // Blit image
                    tex_page.BlitRect(blit_img, new Rect2I(0,0,loaded_tex_data.width,loaded_tex_data.height), new Vector2I(loaded_tex_data.u,loaded_tex_data.v));
                    loaded_textures[prep_tex.path] = loaded_tex_data;
                    placed_on_page.Add(loaded_tex_data);
                    texture_pages[tex_page_ind] = tex_page;
                    break;
                }
            }
        }
        // Create material cache for each page!
        material_cache = new ShaderMaterial[tex_page_ind+1];
        for(int i = 0; i < tex_page_ind+1; i++) 
        {
            texture_pages[i].SavePng("res://Export/Page_" + i + ".png");
            material_cache[i] = (ShaderMaterial)GD.Load("res://Materials/Main.tres").Duplicate(true);
            material_cache[i].SetShaderParameter( "_MainTexture", ImageTexture.CreateFromImage(AssetLoader.texture_pages[i]));
        }
    }






    public static PackData GetPackFromRef(PackRef get_pack)
    {
        return all_packs[AllPackID(get_pack.modid,get_pack.data_type)];
    }


    public static string AllPackID( string modID, MainController.DataType type)
    {
        return type.ToString().ToUpper() + ":" + modID;
    }
}
