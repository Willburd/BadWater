using Godot;
using System;
using System.Collections.Generic;

public partial class AtmoController : DeligateController
{
    public static AtmoController controller;    // Singleton reference for each controller, mostly used during setup to check if controller has init.
    public AtmoController()
    {
        controller = this;
    }

    public const double MINIMUMPRESSUMEDELTA = 0.01;    // difference in pressure where updates are triggered
    public const int MOLEDELTATRIGGER = 10;             // difference in moles where updates are triggered
    public static List<AbstractTurf> pending_turfs = new List<AbstractTurf>();

    public override bool CanInit()
    {
        return IsSubControllerInit(MapController.controller) && IsSubControllerInit(ChemController.controller); // waiting on the map and chem controller first
    }

    public override bool Init()
    {
        display_name = "Atmos";
        tick_rate = 5;
        return true;
    }

    public override void SetupTick()
    {
        FinishInit();
    }

    public override bool Fire()
    {
        //GD.Print(Name + " Fired");
        if(MainController.server_state == MainController.ServerConfig.Editor) return false; // No life tick in edit mode

        while(pending_turfs.Count > 0)
        {
            AbstractTurf turf = pending_turfs[0];
            // PROCESS ATMO - TODO, big growing flood fill that triggers other atmo updates, repeatedly ripping out until satisfied.=================================================================================================================================
            pending_turfs.RemoveAt(0);
        }

        return true;
    }

    public override void Shutdown()
    {
        
    }

    public class AtmoCell
    {
        // TODO - replace fixed gas types with some kind of reagent chemical system with them being able to turn into gasses at certain temps?=================================================================================================================================
        enum GasType
        {
            oxygen,
            nitrogen,
            carbon_dioxide
        }

        // Moles of gasses
        int[] gasses = new int[Enum.GetNames(typeof(GasType)).Length];
        double temp = 0; // KELVIN

        public double Pressure
        {
            get 
            { 
                double total = 0;
                for(int i = 0; i < Enum.GetNames(typeof(GasType)).Length; i++) 
                {
                    total += gasses[i];
                }
                return total * 1; // TODO - ideal gas law=================================================================================================================================
            }
        }

        public bool CompareForUpdate(AtmoCell other_cell)   // Returns true if there is any substantial difference between this cell  and another... Flagging this one for an update!
        {
            // quick pressure check! Handles vacuum turfs!
            if(Mathf.Abs(Pressure - other_cell.Pressure) > MINIMUMPRESSUMEDELTA)
            {
                return true;
            }
            // temp, a lot more forgiving
            if(Mathf.Abs(temp - other_cell.temp) > 0.1)
            {
                return true;
            }
            // gas check!
            for(int i = 0; i < Enum.GetNames(typeof(GasType)).Length; i++) 
            {
                if(Mathf.Abs(gasses[i] - other_cell.gasses[i]) > MOLEDELTATRIGGER)
                {
                    return true;
                }
            }
            return false;
        }

        public double Temp_C
        {
            get {return temp - 273.15;}
        }

        public double Temp_F
        {
            get {return (Temp_C * 1.8) + 32;}
        }
    }
}

