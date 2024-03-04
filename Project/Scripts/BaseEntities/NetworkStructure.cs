using Godot;
using System;
using System.Collections.Generic;

// Structure entites are simple map objects that do not require any kind of automated update, but are not turfs. Things such as signs.
[GlobalClass] 
public partial class NetworkStructure : NetworkEntity
{
    // Beginning of template data

    // End of template data

    public void Sync(AbstractItem abs)
    {
        // sync data
        base.Sync(abs);
    }
    
    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
