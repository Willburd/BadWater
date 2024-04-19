using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

public class Reagent
{
    public Reagent(string new_id, Godot.Collections.Dictionary data)
    {
        id              = new_id;
        name            = JsonHandler.ApplyExistingTag(data,"name",name);
        formula         = JsonHandler.ApplyExistingTag(data,"formula",formula);
        // physics
        melting_point   = new PhasePoint( (float)JsonHandler.ApplyExistingTag(data,"melting_point" ,273.15)    , 101);
        boiling_point   = new PhasePoint( (float)JsonHandler.ApplyExistingTag(data,"boiling_point" ,373.15)    , 101);
        triple_point    = new PhasePoint( (float)JsonHandler.ApplyExistingTag(data,"triple_point_kelvin"  ,273.16) , (float)JsonHandler.ApplyExistingTag(data,"triple_point_kpa"  ,0.611657));
        critical_point  = new PhasePoint( (float)JsonHandler.ApplyExistingTag(data,"critical_point_kelvin",647.096), (float)JsonHandler.ApplyExistingTag(data,"critical_point_kpa",22064));
        molar_mass      = JsonHandler.ApplyExistingTag(data,"molar_mass",molar_mass);
        specific_heat   = JsonHandler.ApplyExistingTag(data,"specific_heat",specific_heat);
        // flags
        oxidizer        = JsonHandler.ApplyExistingTag(data,"oxidizer",oxidizer);
        fuel_gas        = JsonHandler.ApplyExistingTag(data,"is_fuel",fuel_gas);
    }

    private string id = "_:_";
    private string name = "Chemical X"; // Pretty name
    private string formula = "Z24";     // Chemical formula

    // physics
    struct PhasePoint
    {
        public PhasePoint(float temp, float pressure)
        {
            k = temp;
            kpa = pressure;
        }

        public float k;
        public float kpa;
    }
    private PhasePoint melting_point;   // expected at 101kpa
    private PhasePoint boiling_point;   // expected at 101kpa
    private PhasePoint triple_point;
    private PhasePoint critical_point;
    private float molar_mass = 0.01f;   // kg/mol
    private float specific_heat = 10f;  // J/(mol*K)

    // flags
    private bool oxidizer = false;      // Oxygen source for fires
    private bool fuel_gas = false;      // Fuel source for fires, flag to be used as a burnable fuel in machines


    public string Name
    {
        get {return name;}
    }

    public string Formula
    {
        get {return formula;}
    }


    public MathPhysics.MatterStates GetState(float temp, float kpa)
    {
        if(temp <= 0)
        {
            // If you somehow manage this, or exotic physics is taking place...
            // Void is treated however you want it to be treated.
            // It just means temp was at or beneath 0 kelvin.
            return MathPhysics.MatterStates.Void;
        }
        if(temp > critical_point.k && kpa > critical_point.kpa)
        {
            // Super critical state is reached, don't bother with the rest
            return MathPhysics.MatterStates.Critical;
        }

        PhasePoint current_point = new PhasePoint(temp,kpa);
        if(temp >= triple_point.k && kpa >= triple_point.kpa)
        {
            // top right of phase diagram
            // Get 0 to 1 value from triple point at the bottom of the slope, to critical point at the top!
            float slope_critical_prog_temp = (current_point.k - triple_point.k) / (critical_point.k - triple_point.k); 
            float slope_critical_prog_kpa  = (current_point.kpa - triple_point.kpa) / (critical_point.kpa - triple_point.kpa); 

            // If topleft of line, we are liquid(possibly solid)
            if( Mathf.InverseLerp(slope_critical_prog_temp,) )
            {
                // Check if melting point is larger than triple point, if so, if we are to the topleft of it, we are solid
                if(melting_point.k > triple_point.k)
                {
                    // Melting point above triple point, solve for if we are frozen!
                    float slope_frozen_prog_temp = (current_point.k - triple_point.k) / (melting_point.k - triple_point.k); 
                    float slope_frozen_prog_kpa  = (current_point.kpa - triple_point.kpa) / (melting_point.kpa - triple_point.kpa); 

                    if( )
                    {
                        return MathPhysics.MatterStates.Solid;
                    }
                }
                return MathPhysics.MatterStates.Liquid;
            }
            else
            {
                return MathPhysics.MatterStates.Gas;
            }
        }
        else if(temp < triple_point.k)
        {
            // Left side of phase diagram
            float slope_prog_temp = (current_point.k - triple_point.k) / (melting_point.k - triple_point.k); 
            float slope_prog_kpa  = (current_point.kpa - triple_point.kpa) / (melting_point.kpa - triple_point.kpa); 

            // Check if we are to the top left of the line from 0 to triple point, if we are, we are solid(possibly liquid)
            if( )
            {
                // Check if melting point is smaller or equal to triple point, if so, if we are to the topright of it, we are liquid
                if(melting_point.k <= triple_point.k)
                {
                    if( )
                    {
                        return MathPhysics.MatterStates.Liquid;
                    }
                }
                return MathPhysics.MatterStates.Solid;
            }
            else
            {
                return MathPhysics.MatterStates.Gas;
            }
        }
        else
        {
            // Bottom right of phase diagram. Pressure too low to be in any other state but a gas!
            return MathPhysics.MatterStates.Gas;
        }
    }
}
