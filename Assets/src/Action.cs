using System;
using System.Collections.Generic;
using UnityEngine;

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

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ActionScriptableObject", order = 2)]
public class ActionScriptableObject : ScriptableObject
{
    public string ActionName;
    public ACTIVITY_TYPE ActivityType;
    public int TimeInMin;
    public List<AffectedState> AffectedStates;
    public Sprite Sprite;
}

public class Action : MonoBehaviour
{
    [SerializeField] private ActionScriptableObject actionInfo;
    public ActionScriptableObject ActionInfo => actionInfo;

    // TODO: Ask if it is better to get it on awake of have people drag it :) I prefer dragging
    [SerializeField] private Transform actionTransform;
    public Transform ActionTransform => actionTransform;
    [SerializeField] private MeshRenderer actionMeshRenderer;
    private Dictionary<STATE_TYPE, float> affectedStates;
    public Dictionary<STATE_TYPE, float> AffectedStates => affectedStates;
    private string affectedStatesString;
    private void Awake() {
        affectedStates = new Dictionary<STATE_TYPE, float>();
        CacheAffectedStates();
        actionMeshRenderer.material.mainTexture = actionInfo.Sprite.texture;
    }

    // Hvaing a dictionary might be over repeating having the AffectedState class? But maybe it is useful cause A) I can see the states as a dict and B) Then the dictionary helps me easily go through it on other scripts? Also not access priavate stuff?
    private void CacheAffectedStates()
    {
        foreach (AffectedState affectedState in actionInfo.AffectedStates)
        {
            affectedStates.Add(affectedState.StateType, affectedState.Value);
            affectedStatesString += $"{affectedState}\n";
        }
    }

    public override string ToString()
    {
        return $"Action Name: {actionInfo.ActionName}, Time in Min: {actionInfo.TimeInMin}, Activity Type: {actionInfo.ActivityType}, Affected States: {affectedStatesString}";
    }
}