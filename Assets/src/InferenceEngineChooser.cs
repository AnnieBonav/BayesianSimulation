using UnityEngine;

public class InferenceEngineChooser : MonoBehaviour
{
    [SerializeField] private bool overrideSelectedIESettings;
    [SerializeField] private RUN_TYPE runType;
    [SerializeField] private bool saveTrainingData;
    [SerializeField] private string trainingDataFileNumber;
    [SerializeField] private INFERENCE_ENGINE_TYPE ChosenInferenceEngineType;
    [SerializeField] private PredefinedGaussiansIE PredefinedGaussiansIE;
    [SerializeField] private RandomActivityIE RandomActivityIE;
    [SerializeField] private BasicHeuristicsIE BasicHeuristicsIE;
    [SerializeField] private CombinedActivityIE CombinedActivityIE;
    [SerializeField] private ActiveInferenceEngine ActiveInferenceEngine;
    [SerializeField] private ManualTrainingIE ManualTrainingIE;

    public InferenceEngine GetSelectedEngine()
    {
        InferenceEngine selectedInferenceEngine;
        switch (ChosenInferenceEngineType)
        {
            // case INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS:
            //     return PredefinedGaussiansIE;

            case INFERENCE_ENGINE_TYPE.RANDOM_ACTIVITY:
                selectedInferenceEngine = RandomActivityIE;
                break;

            case INFERENCE_ENGINE_TYPE.BASIC_HEURISTICS_ACTIVITY:
                selectedInferenceEngine = BasicHeuristicsIE;
                break;

            case INFERENCE_ENGINE_TYPE.COMBINED_ACTIVITY:
                selectedInferenceEngine = CombinedActivityIE;
                break;
            
            case INFERENCE_ENGINE_TYPE.ACTIVE_INFERENCE:
                selectedInferenceEngine = ActiveInferenceEngine;
                break;

            case INFERENCE_ENGINE_TYPE.MANUAL_TRAINING:
                selectedInferenceEngine = ManualTrainingIE;
                break;
                
            default:
                Debug.LogError("Inference Engine not found");
                return null;
        }

        if(overrideSelectedIESettings)
        {
            selectedInferenceEngine.RunType = runType;
            selectedInferenceEngine.SaveTrainingData = saveTrainingData;
            selectedInferenceEngine.TrainingDataFileNumber = trainingDataFileNumber;
        }

        return selectedInferenceEngine;
    }
}
