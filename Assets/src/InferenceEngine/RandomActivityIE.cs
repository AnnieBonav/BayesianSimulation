using UnityEngine;

public class RandomActivityIE : StandardInference
{
    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.RANDOM_ACTIVITY;
        newTrainingDataFileName = $"RandomActivity{fileCount}";
        existingTrainingDataFileName = $"RandomActivity{trainingDataFileNumber}";
        base.InitializeEngine();
    }

    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        return (ACTIVITY_TYPE)Random.Range(1, activityTypes.Count+1);
    }
}
