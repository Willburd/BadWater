using Godot;
using System;
using System.Collections.Generic;

public class GasMix
{
    public GasMix(Godot.Collections.Dictionary data)
    {
        foreach(string key in data.Keys )
        {
            if(key != "temp")
            {
                MixData mix = new MixData
                {
                    reagent_id = key,
                    moles = data[key].AsDouble()
                };
                mixes.Add(mix);
            }
        } 
        temp = JsonHandler.ApplyExistingTag(data,"name",temp);
    }

    public GasMix(GasMix original)
    {
        foreach(MixData orgmix in original.mixes )
        {
                MixData mix = new MixData
                {
                    reagent_id = orgmix.reagent_id,
                    moles = orgmix.moles
                };
                mixes.Add(mix);
        }
        temp = original.temp;
    }


    double temp = MathPhysics.T20C;
    List<MixData> mixes = new List<MixData>();

    private struct MixData
    {
        public string reagent_id;
        public double moles;
    }


    public List<string> ReagentsInMix()
    {
        List<string> dat = new List<string>();
        foreach(MixData mix in mixes )
        {
            dat.Add(mix.reagent_id);
        }
        return dat;
    }
}