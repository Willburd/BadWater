using Godot;
using System;

public class DisplayName
{
    public DisplayName(string name)
    {
        if(name == "") 
        {
            raw_name = "";
            return;
        }
        raw_name = name;
        if(CheckFlag("/Proper ")) proper = true;
        if(CheckFlag("/Plural ")) plural = true;
        if(CheckFlag("/Proper ")) proper = true; // check reverse order too
    }

    public bool CheckFlag(string check)
    {
        if(raw_name.Length >= check.Length)
        {
            if(raw_name.Substring(0,check.Length).ToUpper() == check.ToUpper()) // may as well allow any capitalization tbh
            {
                raw_name = raw_name.Substring(check.Length,raw_name.Length - check.Length);
                return true;
            }
        }
        return false;
    }

    string raw_name = "";
    bool proper = false;
    bool plural = false;
    public override string ToString()
    {
        return raw_name;
    }

    public int Length
    {
        get {return raw_name.Length;}
    }

    public string The(bool capitalized = false)
    {
        if(raw_name == "") return "";
        string the = capitalized ? "The " : "the ";
        return (proper ? "" : the) + raw_name;
    }

    public string A(bool capitalized = false)
    {
        if(raw_name == "") return "";
        string an = "";
        if(!proper && VowelCheck())
        {
            an = capitalized ? "An " : "an ";
        }
        else
        {
            an = capitalized ? "A " : "a ";
        }
        return (proper ? "" : an) + raw_name;
    }

    public string Some(bool capitalized = false)
    {
        if(raw_name == "") return "";
        string some = capitalized ? "Some " : "some ";
        return (proper ? some : "") + raw_name;
    }

    public string AutoPlural(bool capitalized = false)
    {
        if(IsPlural) {return Some(capitalized);} else {return A(capitalized);};
    }

    private bool VowelCheck()
    {
        string first = raw_name.Substr(0,1);
        string firsttwo = first;
        if(raw_name.Length > 1)
        {
            firsttwo = raw_name.Substr(0,2);
        } 
        if(firsttwo.ToUpper() == "HE" || firsttwo.ToUpper() == "HO") return true;
        return first.ToUpper() == "A" || first.ToUpper() == "E" || first.ToUpper() == "I" || first.ToUpper() == "O" || first.ToUpper() == "U";
    }


    public bool IsProper
    {
        get {return proper;}
    }
    public bool IsPlural
    {
        get {return plural;}
    }
}
