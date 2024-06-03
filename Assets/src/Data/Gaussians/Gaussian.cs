using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GaussianInfo
{
    public STATE_TYPE stateType;
    public float mean;
    public float standardDeviation;
    public int minValue;
    public int maxValue;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GaussianScriptableObject", order = 1)]
public class GaussianScriptableObject : ScriptableObject
{
    public List<GaussianInfo> gaussians;
}