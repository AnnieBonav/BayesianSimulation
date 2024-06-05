using System.Collections.Generic;

// A single PerformedActovity has different gaussians of the states it is affected by, and also stores the prior and the Actovity that was performed
public class PerformedActivityData
{
    public float Prior;
    private Dictionary<STATE_TYPE, GaussianInfo> statesData;
    public Dictionary<STATE_TYPE, GaussianInfo> StatesData => statesData;
    public ACTIVITY_TYPE ActivityType;

    public PerformedActivityData(float prior, List<GaussianInfo> gaussiansInfo, ACTIVITY_TYPE activityType)
    {
        statesData = new Dictionary<STATE_TYPE, GaussianInfo>();
        
        Prior = prior;
        foreach(GaussianInfo gaussianInfo in gaussiansInfo)
        {
            statesData.Add(gaussianInfo.StateType, gaussianInfo);
        }

        ActivityType = activityType;
    }
}
