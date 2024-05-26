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
    // TODO: Ask if it is better to get it on awake of have people drag it :) I prefer dragging
    [SerializeField] private Transform actionTransform;
    public Transform ActionTransform => actionTransform;
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

    public override string ToString()
    {
        return $"Action Name: {actionName}, Value: {value}, Time in Min: {timeInMin}, Activity Type: {activityType}, Affected State: {affectedState}";
    }
}