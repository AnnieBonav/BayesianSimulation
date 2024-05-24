using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



// Moment of Day Tag
public enum MOD_TAG
{
    Morning,
    Afternoon,
    Evening,
    Night
}

public class Action : MonoBehaviour
{
    [SerializeField] private string actionName;
    public string ActionName => actionName;
    [SerializeField] private int value;
    public int Value => value;
    [SerializeField] private int timeInMin;
    public int TimeInMin => timeInMin;
    [SerializeField] private Material material;

    [SerializeField] private ACTIVITY_TYPE activityType;
    public ACTIVITY_TYPE ActivityType => activityType;

    [SerializeField] private STATE_TYPE affectedState;
    public STATE_TYPE AffectedState => affectedState;
    public void Awake()
    {
        // Renderer renderer = this.GetComponent<Renderer>();
        // if (renderer != null)
        // {
        //     renderer.material = material;
        // }
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