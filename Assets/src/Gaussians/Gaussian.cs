using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GaussianInfo
{
    public STATE_TYPE state;
    public float mean;
    public float standardDeviation;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GaussianScriptableObject", order = 1)]
public class GaussianScriptableObject : ScriptableObject
{
    public List<GaussianInfo> gaussians;
}