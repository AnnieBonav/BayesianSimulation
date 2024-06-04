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
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GaussianScriptableObject", order = 1)]
public class GaussianScriptableObject : ScriptableObject
{
    public List<GaussianInfo> gaussians;
}