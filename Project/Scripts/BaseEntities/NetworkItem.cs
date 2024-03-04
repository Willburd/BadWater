using Godot;
using System;
using System.Collections.Generic;

// Item entities are entities that can be picked up by players upon click, and have behaviors when used in hands.
[GlobalClass] 
public partial class NetworkItem : NetworkEntity
{
    // Beginning of template data
    [Export]
    public ItemData.SizeCategory size_category = ItemData.SizeCategory.Tiny;   // Size of item in world and bags
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
