using Godot;
using System;

namespace Behaviors_BASE
{
    public class AbstractTool : AbstractItem
    {
        public DAT.ToolTag tool_tag;

        public AbstractTool(DAT.ToolTag tool)
        {
            tool_tag = tool;
        }
    }
}