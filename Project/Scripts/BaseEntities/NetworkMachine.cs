using Godot;
using System;
using System.Collections.Generic;

// Machine entities are objects on a map that perform a regular update, are not living things, and often interact directly with the map. Rarely some objects that are not machines may use this type.
[GlobalClass] 
public partial class NetworkMachine : NetworkEntity
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
