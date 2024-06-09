using System.Collections.Generic;
using UnityEngine;

// public class RandomActivityIE : StandardInference
// {
//     public override void InitializeEngine()
//     {
//         inferenceEngineType = INFERENCE_ENGINE_TYPE.RANDOM_ACTIVITY;
//         newTrainingDataFileName = $"RandomActivity{fileCount}";
//         existingTrainingDataFileName = $"RandomActivity{trainingDataFileNumber}";
//         base.InitializeEngine();
//     }

//     public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
//     {
//         return (ACTIVITY_TYPE)Random.Range(1, activityTypes.Count+1);
//     }
// }

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
