using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActiveInferenceEngine : InferenceEngine
{
    [SerializeField] private float explorationRate = 0.8f;
    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.ACTIVE_INFERENCE;
        newTrainingDataFileName = $"ActiveInference_{dataTrainer.DataTrainerType}_{fileCount}";
        existingTrainingDataFileName = $"ActiveInference_{dataTrainer.DataTrainerType}_{trainingDataFileNumber}";
        base.InitializeEngine();
    }

    // Will be used only in Active Inference (for now) WAS IN ENGINE
    public void UpdateModel(List<InferenceData> data)
    {
        CalculatePriors();
        CalculateLikelihoods();
    }

    public override ACTIVITY_TYPE InferActivity(InferenceData currentStateValues)
    {
        ACTIVITY_TYPE chosenActivityType;
        if (Random.value < explorationRate)
        {
            // Random exploration
            chosenActivityType = (ACTIVITY_TYPE)Random.Range(1, activityTypes.Count + 1);
        }
        else
        {
            // Using base inference
            chosenActivityType = InferActivityBase(currentStateValues);
        }
            return chosenActivityType;
    }

    protected override void RunInference()
    {
        print("Called active inference in Inference Engine");
        // Is repeated code from RunInference but want to keep it like this until I make it work
        foreach (STATE_TYPE state in agent.States)
        {
            agentsPerformedActivities[state] = new Dictionary<ACTIVITY_TYPE, List<float>>();
        }

        CalculatePriors();
        CalculateLikelihoods();
        
        agent.StartActiveInfering(verbose);
    }

    public void UpdateModel()
    {
        CalculatePriors();
        CalculateLikelihoods();
    }

    protected override void RunAutomaticTraining()
    {
        throw new System.NotImplementedException();
    }
}

