using System.Collections.Generic;
using UnityEngine;
using System;

// Moment of Day Tag
public enum MOD_TAG
{
    Morning,
    Afternoon,
    Evening,
    Night
}

[Serializable]
public class AffectedState
{
    public STATE_TYPE StateType;
    public float Value;
    public override string ToString()
    {
        return $"State Type: {StateType}, Value: {Value}";
    }
}


