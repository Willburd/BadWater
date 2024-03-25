using Godot;
using System;

public class AIHolder
{
    // TODO Everything foundational to mob AI processing and all its hooks...

    public virtual void Alive()
    {

    }
    public virtual void Dead()
    {

    }
    public virtual void ReactToAttack(AbstractEntity attacker)
    {

    }
    public virtual void ReceiveRaunt(AbstractEntity taunter, bool force_target_switch = false)
    {
        
    }


    bool autopilot_flag;
    public bool Autopilot
    {
        get
        {
            return autopilot_flag;
        }
        set
        {
            autopilot_flag = value;
        }
    }


    // todo - remove magic number in favor of enum ==========================================================================================
    int stance;
    public int GetStance
    {
        get 
        {
            return stance;
        }
    }


    bool busy_flag = false;
    public bool IsBusy
    {
        get
        {
            return busy_flag;
        }
        set
        {
            busy_flag = value;
        }
    }
}