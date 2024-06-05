using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GaussianInfo
{
    public STATE_TYPE StateType;
    public float Mean;
    public float StandardDeviation;
    public int MinValue;
    public int MaxValue;
    public float Variance;

    public GaussianInfo(STATE_TYPE stateType, float mean, float standardDeviation, int minValue, int maxValue)
    {
        StateType = stateType;
        Mean = mean;
        StandardDeviation = standardDeviation;
        MinValue = minValue;
        MaxValue = maxValue;
        Variance = -1;
    }

    public GaussianInfo(STATE_TYPE stateType, float mean, int minValue, int maxValue, float variance)
    {
        StateType = stateType;
        Mean = mean;
        StandardDeviation = -1;
        MinValue = minValue;
        MaxValue = maxValue;
        Variance = variance;
    }

    public GaussianInfo(STATE_TYPE stateType, float mean, float standardDeviation, float variance, int minValue = 0, int maxValue = 100)
    {
        StateType = stateType;
        Mean = mean;
        StandardDeviation = standardDeviation;
        MinValue = minValue;
        MaxValue = maxValue;
        Variance = variance;
    }
}

// Manually add the GaussianInfo (which is the State that the gaussian represents with all of the math data) to a list that can be modified in the inspector, so this asset can then be dragged to an object that wants to use this data (Like an activity). This way, the activity can get the GaussianInfo for each state (while getting to know which states affect it), and use it to do its Activity Thing.
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GaussianScriptableObject", order = 1)]
public class GaussianScriptableObject : ScriptableObject
{
    public List<GaussianInfo> gaussians;
}