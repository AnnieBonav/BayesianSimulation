public class ManualTrainingIE : StandardInference
{
    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.MANUAL_TRAINING;
        newTrainingDataFileName = $"BasicHeuristicsActivity{fileCount}";
        existingTrainingDataFileName = $"BasicHeuristicsActivity{trainingDataFileNumber}";
        
        base.InitializeEngine();
    }
}
