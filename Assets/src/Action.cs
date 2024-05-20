using System;
using System.Collections.Generic;
using UnityEngine;

public enum Need
{
    BladderLevel,
    TirednessLevel,
    HungerLevel,
    RelaxationNeed,
    DetectiveNeed
}

// Moment of Day Tag
public enum MOD_TAG
{
    Morning,
    Afternoon,
    Evening,
    Night
}

public class Action
{
    public string name { get; private set; }
    public int value { get; private set; }
    public int timeInMin { get; private set; }

    public Action(string name, int value, int timeInMin)
    {
        this.name = name;
        this.value = value;
        this.timeInMin = timeInMin;
    }

    public string GetJsonInfo()
    {
        // return JsonConvert.SerializeObject(this);
        return "fake json string";
    }

    public override string ToString()
    {
        // return JsonConvert.SerializeObject(this);
        return "fake json string";
    }
}