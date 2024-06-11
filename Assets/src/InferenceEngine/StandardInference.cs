using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Having an abstract method inside of an abstract method is not allowed, and the fix will create a funky architecture, but its fine for now
public class StandardInference : InferenceEngine
{
    public override ACTIVITY_TYPE InferActivity(InferenceData currentStateValues)
    {
        ACTIVITY_TYPE chosenActivityType = InferActivityBase(currentStateValues);
        return chosenActivityType;
    }

    protected override void RunInference()
    {
        print("Called inference in Standard Inference Engine");

        // Saves only the states that affect the current agent
        foreach (STATE_TYPE state in agent.States)
        {
            agentsPerformedActivities[state] = new Dictionary<ACTIVITY_TYPE, List<float>>();
        }

        CacheTrainingData();
        CalculatePriors();
        CalculateLikelihoods();

        agent.StartInfering(verbose);
    }

    protected override void RunAutomaticTraining()
    {
        print("Called Automatic Training in Standard Inference Engine");
        agent.StartTraining(verbose);
    }

    public override void InitializeEngine()
    {
        newTrainingDataFileName = $"StandardInference_{dataTrainer.DataTrainerType}_{fileCount}";
        existingTrainingDataFileName = $"StandardInference_{dataTrainer.DataTrainerType}_{trainingDataFileNumber}";
        base.InitializeEngine();
    }
}
