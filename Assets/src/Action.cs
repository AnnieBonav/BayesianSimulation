using System.Collections.Generic;
using UnityEngine;

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