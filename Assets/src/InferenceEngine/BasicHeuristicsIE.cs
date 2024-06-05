public class BasicHeuristicsIE : StandardInference
{
    public override void InitializeEngine()
    {
        // Could add here the getting the file name and stuff to avoid switches on the Engine Chooser/Parent Inference Engine class
        // Also probably this assignment could be done in the parent using typeof(this) or something like that
        inferenceEngineType = INFERENCE_ENGINE_TYPE.BASIC_HEURISTICS_ACTIVITY;
        base.InitializeEngine();
    }

    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        // TODO: Need to change this to be dynamic, do not know how at the moment :)
        if (trainingStateValues.GetStateValue(STATE_TYPE.BathroomNeed) > 70f) return ACTIVITY_TYPE.Bathroom;
        if (trainingStateValues.GetStateValue(STATE_TYPE.SleepNeed) > 80f) return ACTIVITY_TYPE.Sleep;
        if (trainingStateValues.GetStateValue(STATE_TYPE.FoodNeed) > 50f) return ACTIVITY_TYPE.Food;
        return ACTIVITY_TYPE.Relax;
    }
}
