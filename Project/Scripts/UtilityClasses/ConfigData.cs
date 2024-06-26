using Godot;
using System.IO; 

[GlobalClass] 
public partial class ConfigData : Resource
{
    public void Load(string file_path)
    {
        Godot.Collections.Dictionary data = JsonHandler.ParseJsonFile(file_path);
        name                = JsonHandler.ApplyExistingTag(data,"name",name);
        port                = JsonHandler.ApplyExistingTag(data,"port",port);
        max_clients         = JsonHandler.ApplyExistingTag(data,"max_clients",max_clients);
        max_entities        = JsonHandler.ApplyExistingTag(data,"max_entities",max_entities);
        max_chunks          = JsonHandler.ApplyExistingTag(data,"max_chunks",max_chunks);
        password            = JsonHandler.ApplyExistingTag(data,"password",password);
        loaded_maps         = JsonHandler.ApplyExistingTag(data,"loaded_maps",loaded_maps);
        allow_new_accounts  = JsonHandler.ApplyExistingTag(data,"allow_new_accounts",allow_new_accounts);
        input_factor        = JsonHandler.ApplyExistingTag(data,"movement_factor",input_factor);
    }

    [Export]
    public string name = "Server";
    [Export]
    public int port = 2532;
    [Export]
    public int max_clients = 64;
    [Export]
    public int max_entities = 65535;
    [Export]
    public int max_chunks = 4096;
    [Export]
    public string password = "";
    [Export]
    public string[] loaded_maps;
    [Export]
    public bool allow_new_accounts;
    [Export]
    public float input_factor = 0.1f; // Divide 0 to 1 inputs from game input by this. Makes mobs not move lightning fast
}