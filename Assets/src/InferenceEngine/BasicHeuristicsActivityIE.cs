using System.Collections.Generic;
using System.Linq;

public class BasicHeuristicsActivityIE : InferenceEngine
{
    private List<ACTIVITY_TYPE> activityTypes = new List<ACTIVITY_TYPE> { ACTIVITY_TYPE.Bathroom, ACTIVITY_TYPE.Sleep, ACTIVITY_TYPE.Food, ACTIVITY_TYPE.Relax };

    // WOULD BE IN AGENT BUT TESTING HERE
    // TODO: Chage TrainingData to one of the good abstraction classes
    public override ACTIVITY_TYPE ChooseActivity(TrainingData currentStateValues)
    {
        return InferActivity(currentStateValues);
    }

    private ACTIVITY_TYPE InferActivity(TrainingData currentStateValues)
    {
        Dictionary<ACTIVITY_TYPE, float> posteriorProbabilities = new Dictionary<ACTIVITY_TYPE, float>();

        foreach(ACTIVITY_TYPE activity in activityTypes)
        {
            activitiesData.TryGetValue(activity, out ActivityData activityData);
            // TODO: Change this from currentStateValues.BathroomNeed and such to iterate through all the states and get the values, so it is not hardcoded

            float prior = activityData.Prior; // Retrieve stored prior
            float bathroomLikelihood = GaussianProbability(currentStateValues.BathroomNeed, activityData.BathroomNeedData.Mean, activityData.BathroomNeedData.Variance);
            float sleepLikelihood = GaussianProbability(currentStateValues.SleepNeed, activityData.SleepNeedData.Mean, activityData.SleepNeedData.Variance);
            float foodLikelihood = GaussianProbability(currentStateValues.FoodNeed, activityData.FoodNeedData.Mean, activityData.FoodNeedData.Variance);
            float crimeLikelihood = GaussianProbability(currentStateValues.CrimeRate, activityData.CrimeRateData.Mean, activityData.CrimeRateData.Variance);

            float posterior = prior * bathroomLikelihood * sleepLikelihood * foodLikelihood * crimeLikelihood;
            posteriorProbabilities[activity] = posterior;
        }

        // Gets the activity with the highest posterior probability
        return posteriorProbabilities.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
    }

    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.BASIC_HEURISTICS_ACTIVITY;
    }
}