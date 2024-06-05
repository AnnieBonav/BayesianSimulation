using System.Collections.Generic;
using System.Linq;

public class StandardInference : InferenceEngine
{
    private List<ACTIVITY_TYPE> activityTypes = new List<ACTIVITY_TYPE> { ACTIVITY_TYPE.Bathroom, ACTIVITY_TYPE.Sleep, ACTIVITY_TYPE.Food, ACTIVITY_TYPE.Relax };

    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        // Initial logic for choosing activity, this will be updated later with Naive Bayes inference. Will start with basic "importance" (bathroom > sleep > food > relax)
        // TODO: Will be changed for integers?
        // TODO: Need to change this to be dynamic, do not know how at the moment :)
        if (trainingStateValues.GetStateValue(STATE_TYPE.BathroomNeed) > 70f) return ACTIVITY_TYPE.Bathroom;
        if (trainingStateValues.GetStateValue(STATE_TYPE.SleepNeed) > 80f) return ACTIVITY_TYPE.Sleep;
        if (trainingStateValues.GetStateValue(STATE_TYPE.FoodNeed) > 50f) return ACTIVITY_TYPE.Food;
        return ACTIVITY_TYPE.Relax;
    }

    public override ACTIVITY_TYPE InferActivity(InferenceData currentStateValues)
    {
        Dictionary<ACTIVITY_TYPE, float> posteriorProbabilities = new Dictionary<ACTIVITY_TYPE, float>();

        foreach(ACTIVITY_TYPE activity in activityTypes)
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

    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.BASIC_HEURISTICS_ACTIVITY;
    }
}
