using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ACTIVITY_TYPE
{
    Bathroom,
    Sleep,
    Food,
    Relax,
}

// Basically the Activities are the different classses the classifier will choose from, they all have different gaussians on how the States affect them
public class Activity : MonoBehaviour
{
    [SerializeField] private ACTIVITY_TYPE activityType;
    public ACTIVITY_TYPE ActivityType => activityType;

    [SerializeField] private List<GaussianScriptableObject> stateGaussians;
}
