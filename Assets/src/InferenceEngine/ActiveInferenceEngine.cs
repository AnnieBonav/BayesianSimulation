using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActiveInferenceEngine : InferenceEngine
{
    [SerializeField] private float explorationRate = 0.8f;
    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.ACTIVE_INFERENCE;
        newTrainingDataFileName = $"ActiveInference{fileCount}";
        existingTrainingDataFileName = $"ActiveInference{trainingDataFileNumber}";
        base.InitializeEngine();
    }

    // It is actually training and inferring
    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        if (Random.value < explorationRate)
        {
            // Random exploration
            return (ACTIVITY_TYPE)Random.Range(1, activityTypes.Count + 1);
        }
        else
        {
            return InferActivity(trainingStateValues);
        }
    }

    public override ACTIVITY_TYPE InferActivity(InferenceData currentStateValues)
    {
        Dictionary<ACTIVITY_TYPE, float> posteriorProbabilities = new Dictionary<ACTIVITY_TYPE, float>();

        foreach (ACTIVITY_TYPE activity in activityTypes)
        {
            performedActivitiesData.TryGetValue(activity, out PerformedActivityData performedActivityData);
            
            float prior = performedActivityData.Prior; // Retrieve stored prior
            float posterior = prior;

            foreach (StateData stateData in currentStateValues.StatesValues)
            {
                float stateLikelihood = GaussianProbability(stateData.Value, performedActivityData.StatesData[stateData.StateType].Mean, performedActivityData.StatesData[stateData.StateType].Variance);
                posterior *= stateLikelihood;
            }

            posteriorProbabilities[activity] = posterior;
        }

        // Gets the activity with the highest posterior probability
        return posteriorProbabilities.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
    }
}

