// TODO: Somehow mark tha this cannot be used to train (architecture could also be better)
using UnityEngine;

public class PredefinedGaussiansIE : InferenceEngine
{
    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS;
        base.InitializeEngine();
    }

    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        print("Predefined Gaussians cannot be used to train");
        throw new System.NotImplementedException();
    }

    public override ACTIVITY_TYPE InferActivity(InferenceData currentStateValues)
    {
        float highestLogSum = Mathf.NegativeInfinity;
        Activity chosenActivity = null;

        // foreach (Activity activity in activities)
        // {
        //     float logSum = activity.GetLogsSum(states, verbose);
        //     if (verbose) Debug.Log($"Activity: {activity.ActivityType}, Log Sum: {logSum}");
        //     if (logSum > highestLogSum)
        //     {
        //         highestLogSum = logSum;
        //         chosenActivity = activity;
        //     }
        // }
        // if (verbose) print("Chosen Activity: " + JsonSerialization.ToJson(chosenActivity));
        // return chosenActivity;
        return ACTIVITY_TYPE.NONE;
    }
}
