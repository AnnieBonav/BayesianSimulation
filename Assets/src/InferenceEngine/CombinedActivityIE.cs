using UnityEngine;

public class CombinedActivityIE : StandardInference
{
    [SerializeField] private float explorationRate = 0.2f;

    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.COMBINED_ACTIVITY;
        base.InitializeEngine();
    }

    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        if (Random.value < explorationRate)
        {
            // Random exploration
            return (ACTIVITY_TYPE)Random.Range(1, activityTypes.Count + 1);
        }
        else
        {
            // Heuristic exploitation
            if (trainingStateValues.GetStateValue(STATE_TYPE.BathroomNeed) > 70f) return ACTIVITY_TYPE.Bathroom;
            if (trainingStateValues.GetStateValue(STATE_TYPE.SleepNeed) > 80f) return ACTIVITY_TYPE.Sleep;
            if (trainingStateValues.GetStateValue(STATE_TYPE.FoodNeed) > 50f) return ACTIVITY_TYPE.Food;
            return ACTIVITY_TYPE.Relax;
        }
    }
}
