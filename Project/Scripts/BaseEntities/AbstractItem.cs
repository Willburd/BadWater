using Godot;
using System;
using System.Collections.Generic;

// Item entities are entities that can be picked up by players upon click, and have behaviors when used in hands.

public partial class AbstractItem : AbstractEntity
{
    // Beginning of template data
    public override void TemplateRead(PackData data)
    {
        base.TemplateRead(data);
        ItemData temp = data as ItemData;
        size_category = temp.size_category;
    }
    [Export]
    public ItemData.SizeCategory size_category = ItemData.SizeCategory.Tiny;   // Size of item in world and bags
    // End of template data
}
