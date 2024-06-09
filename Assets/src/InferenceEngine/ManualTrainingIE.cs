using Unity.Serialization.Json;

public class ManualTrainingIE : StandardInference
{
    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.MANUAL_TRAINING;
        newTrainingDataFileName = $"ManualTraining{fileCount}";
        existingTrainingDataFileName = $"ManualTraining{trainingDataFileNumber}";
        
        base.InitializeEngine();
    }

    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        ACTIVITY_TYPE activityType = ACTIVITY_TYPE.NONE;
        print("IN MANUAL " + JsonSerialization.ToJson(trainingStateValues));
        // TODO: Need to change this to be dynamic, do not know how at the moment :)
        // SEE I had an issea because this was callinhg relax and that didnt exist anymore **angry** need to change
        if (trainingStateValues.GetStateValue(STATE_TYPE.CRIME_RATE) > 50f)
        {
            activityType = ACTIVITY_TYPE.DETECTIVE;
        }
        else if (trainingStateValues.GetStateValue(STATE_TYPE.BATHROOM_NEED) > 70f)
        {
            activityType = ACTIVITY_TYPE.BATHROOM;
        }
        else if (trainingStateValues.GetStateValue(STATE_TYPE.SLEEP_NEED) > 60f)
        {
            activityType = ACTIVITY_TYPE.SLEEP;
        }
        else if (trainingStateValues.GetStateValue(STATE_TYPE.FOOD_NEED) > 50f)
        {
            activityType = ACTIVITY_TYPE.FOOD;
        }

        if (activityType == ACTIVITY_TYPE.NONE)
        {
            activityType = ACTIVITY_TYPE.RELAX;
        }

        print($"IN MANUAL " + $"Chosen Activity: {activityType}");

        return activityType;
    }
}
