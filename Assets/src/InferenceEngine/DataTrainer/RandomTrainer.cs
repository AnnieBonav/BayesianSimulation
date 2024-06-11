using System.Collections.Generic;
using UnityEngine;

public class RandomTrainer : DataTrainer
{
    public override ACTIVITY_TYPE ChooseTrainingActivity(List<ACTIVITY_TYPE> availableActivities, InferenceData trainingStateValues)
    {
        ACTIVITY_TYPE activityType = availableActivities[Random.Range(0, availableActivities.Count)];
        Debug.Log("IN RANDOM TRAINER " + JsonUtility.ToJson(trainingStateValues) + $"  Chosen Activity: {activityType}");
        return activityType;
    }

    public RandomTrainer()
    {
        dataTrainerType = DATA_TRAINER_TYPE.RANDOM;
    }
}
