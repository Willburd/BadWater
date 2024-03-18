using Godot;
using System;

[GlobalClass] 
public partial class MobData : PackData
{
    public override void SetVars(Godot.Collections.Dictionary data_override = null)
    {
        base.SetVars(data_override);
        Godot.Collections.Dictionary data = temp_file_data;
        if(data_override != null) data = data_override;
        // Gameplay
        max_health      = TOOLS.ApplyExistingTag(data,"max_health",max_health);
        walk_speed      = TOOLS.ApplyExistingTag(data,"move_speed",walk_speed);
        run_speed       = TOOLS.ApplyExistingTag(data,"run_speed",run_speed);
        has_hands       = TOOLS.ApplyExistingTag(data,"has_hands",has_hands);
        extra_hands     = TOOLS.ApplyExistingTag(data,"extra_hands",extra_hands);
        complex_tools   = TOOLS.ApplyExistingTag(data,"complex_tools",complex_tools);
        // invslots 
        wears_hats      = TOOLS.ApplyExistingTag(data,"wears_hats",wears_hats);
        wears_mask      = TOOLS.ApplyExistingTag(data,"wears_mask",wears_mask);
        wears_eyes      = TOOLS.ApplyExistingTag(data,"wears_eyes",wears_eyes);
        wears_uniform   = TOOLS.ApplyExistingTag(data,"wears_uniform",wears_uniform);
        wears_suit      = TOOLS.ApplyExistingTag(data,"wears_suit",wears_suit);
        wears_shoe      = TOOLS.ApplyExistingTag(data,"wears_shoe",wears_shoe);
        wears_ears      = TOOLS.ApplyExistingTag(data,"wears_ears",wears_ears);
        wears_glove     = TOOLS.ApplyExistingTag(data,"wears_glove",wears_glove);
        wears_belt      = TOOLS.ApplyExistingTag(data,"wears_belt",wears_belt);
    }

    protected override string GetVarString()
    {
        // Print variables of loaded data for debugging
        return " name: " + display_name + " description: " + description + " tag: " + tag + " maxhealth: "  + max_health;
    }
    
    public override void Clone(PackData source)
    {
        MobData temp = source as MobData;
        base.Clone(temp);
        // Gameplay
        max_health = temp.max_health;
        walk_speed      = temp.walk_speed;
        run_speed       = temp.run_speed;
        has_hands       = temp.has_hands;
        extra_hands     = temp.extra_hands;
        complex_tools   = temp.complex_tools;
        // Inv slots
        wears_hats      = temp.wears_hats;
        wears_mask      = temp.wears_mask;
        wears_eyes      = temp.wears_eyes;
        wears_uniform   = temp.wears_uniform;
        wears_suit      = temp.wears_suit;
        wears_shoe      = temp.wears_shoe;
        wears_ears      = temp.wears_ears;
        wears_glove     = temp.wears_glove;
        wears_belt      = temp.wears_belt;
    }
    // Unique data
    public int max_health;
    public float walk_speed = (float)0.25;
    public float run_speed = 1;
    public bool has_hands;
    public bool extra_hands;
    public bool complex_tools;
    // Inventory slots
    public bool wears_hats;
    public bool wears_mask;
    public bool wears_eyes;
    public bool wears_uniform; // Controls more than uniform slot, allows pockets, id, back, etc
    public bool wears_suit;
    public bool wears_shoe;
    public bool wears_ears;
    public bool wears_glove;
    public bool wears_belt;
}
