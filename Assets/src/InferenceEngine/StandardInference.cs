using System.Collections.Generic;
using System.Linq;

// Having an abstract method inside of an abstract method is not allowed, and the fix will create a funky architecture, but its fine for now
public class StandardInference : InferenceEngine
{
    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        return ACTIVITY_TYPE.NONE;
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
}
