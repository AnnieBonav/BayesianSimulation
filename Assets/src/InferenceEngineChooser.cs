using UnityEngine;

public class InferenceEngineChooser : MonoBehaviour
{
    [SerializeField] private bool overrideSelectedIESettings;
    [SerializeField] private RUN_TYPE runType;
    [SerializeField] private bool saveRunData;
    [SerializeReference] private bool trainSingleState;
    [SerializeField] private string trainingDataFileNumber;
    [SerializeField] private bool verbose = false;
    [SerializeField] private bool verboseInitialData = false;
    [SerializeField] private DATA_TRAINER_TYPE dataTrainerType;
    [SerializeField] private INFERENCE_ENGINE_TYPE ChosenInferenceEngineType;
    [SerializeField] private PredefinedGaussiansIE PredefinedGaussiansIE;
    [SerializeField] private StandardInference StandardInferenceEngine;
    [SerializeField] private ActiveInferenceEngine ActiveInferenceEngine;

    public InferenceEngine GetSelectedEngine()
    {
        InferenceEngine selectedInferenceEngine;
        switch (ChosenInferenceEngineType)
        {
            case INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS:
                selectedInferenceEngine = PredefinedGaussiansIE;
                break;

            case INFERENCE_ENGINE_TYPE.STANDARD_INFERENCE:
                selectedInferenceEngine = StandardInferenceEngine;
                break;
            
            case INFERENCE_ENGINE_TYPE.ACTIVE_INFERENCE:
                selectedInferenceEngine = ActiveInferenceEngine;
                break;
                
            default:
                Debug.LogError("Inference Engine not found");
                return null;
        }

        if(overrideSelectedIESettings)
        {
            selectedInferenceEngine.RunType = runType;
            selectedInferenceEngine.TrainSingleState = trainSingleState;
            selectedInferenceEngine.SaveRunData = saveRunData;
            selectedInferenceEngine.TrainingDataFileNumber = trainingDataFileNumber;
            selectedInferenceEngine.Verbose = verbose;
            selectedInferenceEngine.VerboseInitialData = verboseInitialData;
            selectedInferenceEngine.DataTrainerType = dataTrainerType;
        }

        return selectedInferenceEngine;
    }
}
