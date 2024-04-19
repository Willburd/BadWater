using Godot;
using System;

// Massive credit to SS13 polaris repo for all of these calculations, some have been modified for our own engine.
// Comments have been preserved, we've no reason to hide that this is a straight up copypaste.

// https://github.com/PolarisSS13/Polaris
// Combination of:
// code\__defines\math_physics.dm
// code\__defines\atmos.dm


public static class MathPhysics
{
    // Math constants.
    public const double R_IDEAL_GAS_EQUATION       = 8.31;    // kPa*L/(K*mol).
    public const double ONE_ATMOSPHERE             = 101.325; // kPa.
    public const double IDEAL_GAS_ENTROPY_CONSTANT = 1164;    // (mol^3 * s^3) / (kg^3 * L).
    public const double ADIABATIC_EXPONENT         = 0.667; //Actually adiabatic exponent - 1.

    public const double T0C    = 273.15;  //    0.0 degrees celcius
    public const double T20C   = 293.15;  //   20.0 degrees celcius
    public const double TCMB   = 2.7;     // -270.3 degrees celcius
    public const double TN60C  = 213.15; //    -60 degrees celcius

    // Radiation constants.
    public const double STEFAN_BOLTZMANN_CONSTANT    = 5.6704e-8; // W/(m^2*K^4).
    public const double COSMIC_RADIATION_TEMPERATURE = 3.15;      // K.
    public const double AVERAGE_SOLAR_RADIATION      = 200;       // W/m^2. Kind of arbitrary. Really this should depend on the sun position much like solars.
    public const double RADIATOR_OPTIMUM_PRESSURE    = 3771;      // kPa at 20 C. This should be higher as gases aren't great conductors until they are dense. Used the critical pressure for air.
    public const double GAS_CRITICAL_TEMPERATURE     = 132.65;    // K. The critical point temperature for air.

    public const double RADIATOR_EXPOSED_SURFACE_AREA_RATIO = 0.04; // (3 cm + 100 cm * sin(3deg))/(2*(3+100 cm)). Unitless ratio.
    public const double HUMAN_EXPOSED_SURFACE_AREA          = 5.2; //m^2, surface area of 1.7m (H) x 0.46m (D) cylinder


    // Atmo constants
    const double CELL_VOLUME = 2500; // Liters in a cell.
    const double MOLES_CELLSTANDARD = ONE_ATMOSPHERE*CELL_VOLUME/(T20C*R_IDEAL_GAS_EQUATION); // Moles in a 2.5 m^3 cell at 101.325 kPa and 20 C.

    public const double O2STANDARD = 0.21; // Percentage.
    public const double N2STANDARD = 0.79;

    public const double MOLES_O2STANDARD = (MOLES_CELLSTANDARD * O2STANDARD); // O2 standard value (21%)
    public const double MOLES_N2STANDARD = (MOLES_CELLSTANDARD * N2STANDARD); // N2 standard value (79%)
    public const double MOLES_O2ATMOS = MOLES_O2STANDARD*50;
    public const double MOLES_N2ATMOS = MOLES_N2STANDARD*50;

    public const double BREATH_VOLUME = 0.5; // Liters in a normal breath.
    public const double BREATH_MOLES = (ONE_ATMOSPHERE * BREATH_VOLUME / (T20C * R_IDEAL_GAS_EQUATION)); // Amount of air to take a from a tile
    public const double BREATH_PERCENTAGE = (BREATH_VOLUME / CELL_VOLUME);                               // Amount of air needed before pass out/suffocation commences.
    public const double HUMAN_NEEDED_OXYGEN = MOLES_CELLSTANDARD * BREATH_PERCENTAGE * 0.16;
    public const double HUMAN_HEAT_CAPACITY = 280000; //J/K For 80kg person

    public const double MINIMUM_AIR_RATIO_TO_SUSPEND = 0.05; // Minimum ratio of air that must move to/from a tile to suspend group processing
    public const double MINIMUM_AIR_TO_SUSPEND       = MOLES_CELLSTANDARD * MINIMUM_AIR_RATIO_TO_SUSPEND; // Minimum amount of air that has to move before a group processing can be suspended
    public const double MINIMUM_MOLES_DELTA_TO_MOVE  = MOLES_CELLSTANDARD * MINIMUM_AIR_RATIO_TO_SUSPEND; // Either this must be active
    public const double MINIMUM_TEMPERATURE_TO_MOVE  = T20C + 100;                                        // or this (or both, obviously)
    public const double MINIMUM_PRESSURE_DIFFERENCE_TO_SUSPEND = (MINIMUM_AIR_TO_SUSPEND*R_IDEAL_GAS_EQUATION*T20C)/CELL_VOLUME;			// Minimum pressure difference between zones to suspend

    public const double MINIMUM_TEMPERATURE_RATIO_TO_SUSPEND      = 0.012;        // Minimum temperature difference before group processing is suspended.
    public const double MINIMUM_TEMPERATURE_DELTA_TO_SUSPEND      = 4;
    public const double MINIMUM_TEMPERATURE_DELTA_TO_CONSIDER     = 0.5;          // Minimum temperature difference before the gas temperatures are just set to be equal.
    public const double MINIMUM_TEMPERATURE_FOR_SUPERCONDUCTION   = T20C + 10;
    public const double MINIMUM_TEMPERATURE_START_SUPERCONDUCTION = T20C + 200;


    //These control the mole ratio of oxidizer and fuel used in the combustion reaction
    public const double FIRE_REACTION_OXIDIZER_AMOUNT	= 3; //should be greater than the fuel amount if fires are going to spread much
    public const double FIRE_REACTION_FUEL_AMOUNT		= 2;

    //These control the speed at which fire burns
    public const double FIRE_GAS_BURNRATE_MULT			= 1;
    public const double FIRE_LIQUID_BURNRATE_MULT		= 0.225;

    //If the fire is burning slower than this rate then the reaction is going too slow to be self sustaining and the fire burns itself out.
    //This ensures that fires don't grind to a near-halt while still remaining active forever.
    public const double FIRE_GAS_MIN_BURNRATE			= 0.01;
    public const double FIRE_LIQUD_MIN_BURNRATE		= 0.0025;

    //How many moles of fuel are contained within one solid/liquid fuel volume unit
    public const double LIQUIDFUEL_AMOUNT_TO_MOL		= 0.45;  //mol/volume unit

    public enum MatterStates
    {
        Void,
        Solid,
        Liquid,
        Gas,
        Critical
    }
}
