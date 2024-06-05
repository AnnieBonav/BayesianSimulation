using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InferenceEngineChooser : MonoBehaviour
{
    [SerializeField] public INFERENCE_ENGINE_TYPE ChosenInferenceEngineType;
    [SerializeField] private PredefinedGaussiansIE PredefinedGaussiansIE;
    [SerializeField] private RandomActivityIE RandomActivityIE;
    [SerializeField] private BasicHeuristicsIE BasicHeuristicsIE;
    [SerializeField] private ActiveInferenceEngine ActiveInferenceEngine;

    private void Start() {
        switch (ChosenInferenceEngineType)
        {
            // case INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS:
            //     PredefinedGaussiansIE.InitializeEngine();
            //     break;
            case INFERENCE_ENGINE_TYPE.BASIC_HEURISTICS_ACTIVITY:
                BasicHeuristicsIE.InitializeEngine();
                break;
            case INFERENCE_ENGINE_TYPE.RANDOM_ACTIVITY:
                RandomActivityIE.InitializeEngine();
                break;
            default:
                Debug.LogError("Inference Engine not found");
                break;
        }
    }

    public InferenceEngine GetSelectedEngine()
    {
        switch (ChosenInferenceEngineType)
        {
            case INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS:
                return PredefinedGaussiansIE;

            case INFERENCE_ENGINE_TYPE.RANDOM_ACTIVITY:
                return RandomActivityIE;

            case INFERENCE_ENGINE_TYPE.BASIC_HEURISTICS_ACTIVITY:
                return BasicHeuristicsIE;
            
            case INFERENCE_ENGINE_TYPE.ACTIVE_INFERENCE:
                return ActiveInferenceEngine;
                
            default:
                Debug.LogError("Inference Engine not found");
                return null;
        }
    }
}
