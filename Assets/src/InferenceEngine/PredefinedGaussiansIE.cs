public class PredefinedGaussiansIE : StandardInference
{
    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS;
        base.InitializeEngine();
    }

    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        // TODO: Actually implement getting it from gaussians lol
        return ACTIVITY_TYPE.None;
    }
}
