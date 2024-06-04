public enum INFERENCE_ENGINE_TYPE
{
    PREDEFINED_GAUSSIANS,
    RANDOM_ACTIVITY,
    BASIC_HEURISTICS_ACTIVITY
}

// TODO: Would need to create a gaussianScriptableObject or something like that for the activities to store this mean, variance and prior, but for now will make simple data type
public class ActivityOneStateData
{
    public float Mean;
    public float Variance;
    public STATE_TYPE StateType;

    public ActivityOneStateData(float mean, float variance, STATE_TYPE stateType)
    {
        Mean = mean;
        Variance = variance;
        StateType = stateType;
    }
}

public class ActivityData
{
    public float Prior;
    public ActivityOneStateData BathroomNeedData;
    public ActivityOneStateData SleepNeedData;
    public ActivityOneStateData FoodNeedData;
    public ActivityOneStateData CrimeRateData;
    public ACTIVITY_TYPE ActivityType;

    public ActivityData(float prior, ActivityOneStateData bathroomNeedData, ActivityOneStateData sleepNeedData, ActivityOneStateData foodNeedData, ActivityOneStateData crimeRateData, ACTIVITY_TYPE activityType)
    {
        Prior = prior;
        BathroomNeedData = bathroomNeedData;
        SleepNeedData = sleepNeedData;
        FoodNeedData = foodNeedData;
        CrimeRateData = crimeRateData;
        ActivityType = activityType;
    }
}
