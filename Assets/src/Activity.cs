using System.Collections.Generic;
using UnityEngine;

public enum ACTIVITY_TYPE
{
    NONE,
    BATHROOM,
    SLEEP,
    FOOD,
    RELAX,
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
