using UnityEngine;

public class ActivityDebugBtn : MonoBehaviour
{
    [SerializeField] private Agent agent;
    [SerializeField] private ACTIVITY_TYPE activityType;

    public void PerformAction()
    {
        StartCoroutine(agent.ManuallyPerformActionForTraining(activityType));
        // agent.ManuallyPerformActionForTraining(activityType);
    }

    public void Detective()
    {
        StartCoroutine(agent.SolveCrime());
    }
}
