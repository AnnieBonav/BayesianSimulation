using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Having an abstract method inside of an abstract method is not allowed, and the fix will create a funky architecture, but its fine for now
public class StandardInference : InferenceEngine
{
    public override ACTIVITY_TYPE InferActivity(InferenceData currentStateValues)
    {
    Dictionary<ACTIVITY_TYPE, float> logPosteriorProbabilities = new Dictionary<ACTIVITY_TYPE, float>();

    foreach (ACTIVITY_TYPE activity in activityTypes)
    {
        if (performedActivitiesData.TryGetValue(activity, out PerformedActivityData performedActivityData))
        {
            float logPrior = Mathf.Log(performedActivityData.Prior); // Retrieve and log the prior
            float logPosterior = logPrior;

            foreach (StateData stateData in currentStateValues.StatesValues)
            {
                if (performedActivityData.StatesData.TryGetValue(stateData.StateType, out GaussianInfo stateStatistics))
                {
                    float stateLogLikelihood = Mathf.Log(GaussianProbability(stateData.Value, stateStatistics.Mean, stateStatistics.Variance));
                    logPosterior += stateLogLikelihood;
                }
            }

            logPosteriorProbabilities[activity] = logPosterior;
        }
    }

    // Gets the activity with the highest log posterior probability
    return logPosteriorProbabilities.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
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
