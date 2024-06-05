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
        if (trainingStateValues.GetStateValue(STATE_TYPE.BATHROOM_NEED) > 70f) return ACTIVITY_TYPE.BATHROOM;
        if (trainingStateValues.GetStateValue(STATE_TYPE.SLEEP_NEED) > 80f) return ACTIVITY_TYPE.SLEEP;
        if (trainingStateValues.GetStateValue(STATE_TYPE.FOOD_NEED) > 50f) return ACTIVITY_TYPE.FOOD;
        return ACTIVITY_TYPE.RELAX;
    }
}
