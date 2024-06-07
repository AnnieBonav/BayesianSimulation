using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

public enum ACTIVITY_TYPE
{
    [EnumMember(Value = "None")]
    NONE,
    [EnumMember(Value = "Bathroom")]
    BATHROOM,
    [EnumMember(Value = "Sleep")]
    SLEEP,
    [EnumMember(Value = "Food")]
    FOOD,
    [EnumMember(Value = "Relax")]
    RELAX,
    [EnumMember(Value = "Detective")]
    DETECTIVE
}

// Activities are the different classes the classifier will choose from
public class Activity : MonoBehaviour
{
    [SerializeField] private ACTIVITY_TYPE activityType;
    public ACTIVITY_TYPE ActivityType => activityType;
    [SerializeField] private List<Action> possibleActions;
    public List<Action> PossibleActions => possibleActions;
}
