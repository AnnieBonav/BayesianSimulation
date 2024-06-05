using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Serialization.Json;
using UnityEngine;

public enum RUN_TYPE
{
    Training,
    Inference
}

public enum INFERENCE_ENGINE_TYPE
{
    NONE,
    PREDEFINED_GAUSSIANS,
    RANDOM_ACTIVITY,
    BASIC_HEURISTICS_ACTIVITY,
    ACTIVE_INFERENCE
}

// Inference Engine both handles the training (based on which Engine it is), and then the inference (based on that training.) It calls the functions on the Agent and (...)
public abstract class InferenceEngine : MonoBehaviour
{
    [SerializeField] protected RUN_TYPE runType;
    [SerializeField] protected bool saveTrainingData;
    [SerializeField] protected string trainingDataFileNumber;
    [SerializeField] protected bool verbose = false;
    // Based on the agents, its states will be used to create the training data
    [SerializeField] protected Agent agent;
    // CONSIDER: Need to use the TYPE for checking so the actual states and activities...do not go outside of the Agent
    protected Dictionary<ACTIVITY_TYPE, int> activityCounts;

    protected Dictionary<ACTIVITY_TYPE, PerformedActivityData> performedActivitiesData; // Used in run inference
    protected Dictionary<STATE_TYPE, Dictionary<ACTIVITY_TYPE, List<float>> > agentsPerformedActivities; // Also used in run inference. Per state that affects the Agent, the different actiivities that were tested are saved with a list of the values 

    protected int totalData = -1;
    protected List<InferenceData> trainingData;
    protected List<ACTIVITY_TYPE> activityTypes;

    protected void Awake() {
        activityCounts = new Dictionary<ACTIVITY_TYPE, int>();
        performedActivitiesData = new Dictionary<ACTIVITY_TYPE, PerformedActivityData>();
        agentsPerformedActivities = new Dictionary<STATE_TYPE, Dictionary<ACTIVITY_TYPE, List<float>>>();
        trainingData = new List<InferenceData>();
        activityTypes = new List<ACTIVITY_TYPE>();
    }

    private void OnDisable()
    {
        SaveTrainingData();
    }

    private string NewTrainingDataFileName()
    {
        int fileCount = Directory.GetFiles("Assets/src/Data/TrainingData", "*.asset").Length;
        string fileName;
        switch(inferenceEngineType)
        {
            case INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS:
                fileName = $"PredefinedGaussians{fileCount}";
                break;

            case INFERENCE_ENGINE_TYPE.RANDOM_ACTIVITY:
                fileName = $"RandomActivity{fileCount}";
                break;

            case INFERENCE_ENGINE_TYPE.BASIC_HEURISTICS_ACTIVITY:
                fileName = $"BasicHeuristicsActivity{fileCount}";
                break;

            case INFERENCE_ENGINE_TYPE.ACTIVE_INFERENCE:
                fileName = $"ActiveInference{fileCount}";
                break;

            default: // Stupid that it makes me add default (unasigeable variable fileName) cause I am adding all the cases, but it is what it is
                fileName = $"TrainingData{fileCount}";
                break;
        }
        return fileName;
    }

    private string ExistingTrainingDataFileName()
    {
        string fileName;
        switch(inferenceEngineType)
        {
            case INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS:
                fileName = $"PredefinedGaussians{trainingDataFileNumber}";
                break;

            case INFERENCE_ENGINE_TYPE.RANDOM_ACTIVITY:
                fileName = $"RandomActivity{trainingDataFileNumber}";
                break;

            case INFERENCE_ENGINE_TYPE.BASIC_HEURISTICS_ACTIVITY:
                fileName = $"BasicHeuristicsActivity{trainingDataFileNumber}";
                break;

            case INFERENCE_ENGINE_TYPE.ACTIVE_INFERENCE:
                fileName = $"ActiveInference{trainingDataFileNumber}";
                break;

            default:
                fileName = $"TrainingData{trainingDataFileNumber}";
                break;
        }

        return fileName;
    }

    protected void SaveTrainingData()
    {
        if(saveTrainingData)
        {
            TrainingDataWrapper trainingDataWrapper = new TrainingDataWrapper(agent.Activities, agent.States, agent.PerformedActivitiesData);
            SaveTrainingData(trainingDataWrapper);
        }else
        {
            print("Training Data not saved");
        }
    }

    // TODO: Change save data to the engine? Engine should hear what the agent is doing and based on that save the data
    // TODO: Save scriptable object in the future, rn will be a JSON that works because gets serialized from the trainingDataScriptableObject so the list is respected, can just open it later
    private void SaveTrainingData(TrainingDataWrapper trainingDataWrapper)
    {
        string trainingDataJSON = JsonSerialization.ToJson(trainingDataWrapper);
        print("Final Training Data JSON" + trainingDataJSON);

        string fileName = NewTrainingDataFileName();
        string filePath = $"Assets/src/Data/TrainingData/{fileName}.json";

        File.WriteAllText(filePath, trainingDataJSON);
    }

    protected void StartEngine()
    {
        // Caches the activities from the agent
        activityTypes = agent.Activities;

        // Do in Start and not awake cause Agent needs to be initialized first. Could probably use some better architecture
        switch(runType){
            case RUN_TYPE.Training:
                RunTraining();
                break;
            case RUN_TYPE.Inference:
                RunInference();
                break;
            default:
                Debug.LogError("Run type not found");
                break;
        }
    }

    protected void RunTraining()
    {
        print("Called training in Inference Engine");
        agent.StartTraining(verbose);
    }

    protected void RunInference()
    {
        print("Called inference in Inference Engine");

        // Saves only the states that affect the current agent
        foreach (STATE_TYPE state in agent.States)
        {
            agentsPerformedActivities[state] = new Dictionary<ACTIVITY_TYPE, List<float>>();
        }
        CacheTrainingData();
        CalculatePriors();
        CalculateLikelihoods();

        agent.StartInfering();
    }

    // Reads the data from a presaved JSON file
    protected void CacheTrainingData()
    {
        string fileName = ExistingTrainingDataFileName();
        string filePath = $"Assets/src/Data/TrainingData/{fileName}.json";

        string jsonText = File.ReadAllText(filePath);
        print("JsonText: " + jsonText);

        TrainingDataWrapper trainingDataObject = JsonSerialization.FromJson<TrainingDataWrapper>(jsonText.ToString());

        print("TrainingDataObject: " + trainingDataObject.InferenceData.Count);
        trainingData = trainingDataObject.InferenceData;

        if (verbose)
        {
            foreach (InferenceData data in trainingData)
            {
                Debug.Log(JsonSerialization.ToJson(data));
            }
        }
    }

    protected void CalculatePriors()
    {

        // Initialize dictionaries with all the known ACTIVITY_TYPE values
        // TODO would need to make it dynamic if I need to control which activities affect it
        foreach (ACTIVITY_TYPE activity in Enum.GetValues(typeof(ACTIVITY_TYPE)))
        {
            activityCounts[activity] = 0;
            // Per every state, it will create a list of every activivity that was tested and the values that were gotten (from that state and activity)
            foreach (STATE_TYPE state in agentsPerformedActivities.Keys)
            {
                agentsPerformedActivities[state][activity] = new List<float>();
            }
        }

        // Count occurrences the ocurrances of each actovoty (in activityCounts) and store the values of the states in the corresponding lists (in bathroomNeeds, sleepNeeds, foodNeeds, crimeRates
        foreach (InferenceData data in trainingData)
        {
            activityCounts[data.ChosenActivity]++;
            foreach (STATE_TYPE state in agentsPerformedActivities.Keys)
            {
                agentsPerformedActivities[state][data.ChosenActivity].Add(data.GetStateValue(state));
            }
        }

        totalData = trainingData.Count;
    }

    protected float CalculateAverage(List<float> values)
    {
        if (values.Count == 0)
        {
            return 0f;
        }

        float sum = 0f;
        foreach (float value in values)
        {
            sum += value;
        }

        return sum / values.Count;
    }

    // To really be dynamic, the activities list could be gotten from the InferenceData JSON itself. However, getting it from the current agent ensures that the agent has the Activities and the states. And it is important to ensure that the current agent with its current information is the one whos Training Data is being used, as no mixing should be done (maybe saving the agent and checking its the same would be great!)
    protected void CalculateLikelihoods()
    {
        // Calculate priors and likelihoods (mean and variance)
        foreach (ACTIVITY_TYPE activityType in agent.Activities)
        {
            float prior = (float)activityCounts[activityType] / totalData;
            
            List<GaussianInfo> statesData = new List<GaussianInfo>();

            foreach (STATE_TYPE stateType in agentsPerformedActivities.Keys)
            {
                float stateMean = CalculateAverage(agentsPerformedActivities[stateType][activityType]);
                float stateVariance = Variance(agentsPerformedActivities[stateType][activityType], stateMean);
                // Create a gaussian with the variance constructor, standard deviation will be -1
                GaussianInfo stateData = new GaussianInfo(stateType, stateMean, 0, 100, stateVariance);
                statesData.Add(stateData);
            }

            PerformedActivityData activityData = new PerformedActivityData(prior, statesData, activityType);
            
            performedActivitiesData[activityType] = activityData;
        }
    }
    
    protected float Variance(List<float> values, float mean)
    {
        return values.Select(v => (v - mean) * (v - mean)).Sum() / values.Count;
    }

    protected float GaussianProbability(float x, float mean, float variance)
    {
        return (1 / Mathf.Sqrt(2 * Mathf.PI * variance)) * Mathf.Exp(-((x - mean) * (x - mean)) / (2 * variance));
    }
    
    public virtual void InitializeEngine()
    {
        StartEngine();
    }
    
    public abstract ACTIVITY_TYPE InferActivity(InferenceData currentStateValues);
    public abstract ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues);
    protected INFERENCE_ENGINE_TYPE inferenceEngineType;
}
