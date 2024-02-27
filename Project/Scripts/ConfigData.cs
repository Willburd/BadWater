using Godot;
using System.IO; 

[GlobalClass] 
public partial class ConfigData : Resource
{
    public void Load(string file_path)
    {
        Godot.Collections.Dictionary data = TOOLS.ParseJson(file_path);
        string prefix = Path.GetFileNameWithoutExtension(file_path);
        if(data.ContainsKey("name"))        name = data["name"].AsString();
        if(data.ContainsKey("port"))        port = (int)data["port"].AsDouble();
        if(data.ContainsKey("max_clients")) max_clients = (int)data["max_clients"].AsDouble();
        if(data.ContainsKey("max_entities"))max_entities = (int)data["max_entities"].AsDouble();
        if(data.ContainsKey("password"))    password = data["password"].AsString();
        if(data.ContainsKey("loaded_maps")) loaded_maps = data["loaded_maps"].AsStringArray();
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
    public string password = "";
    [Export]
    public string[] loaded_maps;
}