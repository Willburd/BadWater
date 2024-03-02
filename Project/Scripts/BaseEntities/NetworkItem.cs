using Godot;
using System;
using System.Collections.Generic;

// Item entities are entities that can be picked up by players upon click, and have behaviors when used in hands.
[GlobalClass] 
public partial class NetworkItem : NetworkEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        PackRef = new PackRef(data);
        ItemData temp = AssetLoader.GetPackFromModID(PackRef) as ItemData;
        SetTag(temp.tag);
        model = temp.model;
        texture = temp.texture;
        size_category = temp.size_category;
        SetBehavior(Behavior.CreateBehavior(temp));
    }
    public override PackData TemplateWrite()
    {
        ItemData data = new ItemData();
        data.tag = tag;
        data.model = model;
        data.texture = texture;
        data.size_category = size_category;
        return data;
    }
    [Export]
    public bool density;                    // blocks movement
    [Export]
    public bool opaque;                     // blocks vision
    [Export]
    public ItemData.SizeCategory size_category = ItemData.SizeCategory.Tiny;   // Size of item in world and bags
    // End of template data

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(1); // Server
    }
}
