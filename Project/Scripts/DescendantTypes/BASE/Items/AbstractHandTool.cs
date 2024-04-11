using Godot;
using System;

namespace Behaviors
{
    public class AbstractHandTool : AbstractItem
    {
        public DAT.ToolTag tool_tag;

        public AbstractHandTool(DAT.ToolTag tool)
        {
            tool_tag = tool;
            entity_type = MainController.DataType.Item;
        }
    }
}