using Godot;
using System.IO; 

[GlobalClass] 
public partial class ConfigData : Resource
{
    public void Load(string file_path)
    {
        Godot.Collections.Dictionary data = TOOLS.ParseJsonFile(file_path);
        name                = TOOLS.ApplyExistingTag(data,"name",name);
        port                = TOOLS.ApplyExistingTag(data,"port",port);
        max_clients         = TOOLS.ApplyExistingTag(data,"max_clients",max_clients);
        max_entities        = TOOLS.ApplyExistingTag(data,"max_entities",max_entities);
        max_chunks          = TOOLS.ApplyExistingTag(data,"max_chunks",max_chunks);
        password            = TOOLS.ApplyExistingTag(data,"password",password);
        loaded_maps         = TOOLS.ApplyExistingTag(data,"loaded_maps",loaded_maps);
        allow_new_accounts  = TOOLS.ApplyExistingTag(data,"allow_new_accounts",allow_new_accounts);
        input_factor        = TOOLS.ApplyExistingTag(data,"movement_factor",input_factor);
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
    public double input_factor = 0.1; // Divide 0 to 1 inputs from game input by this. Makes mobs not move lightning fast
}