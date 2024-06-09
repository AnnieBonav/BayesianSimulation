using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Serialization.Json;
using UnityEngine;

public enum RUN_TYPE
{
    AUTOMATIC_TRAINING,
    MANUAL_TRAINING,
    INFERENCE,
    ACTIVE_INFERENCE
}

public enum INFERENCE_ENGINE_TYPE
{
    NONE,
    PREDEFINED_GAUSSIANS,
    RANDOM_ACTIVITY,
    BASIC_HEURISTICS_ACTIVITY,
    COMBINED_ACTIVITY,
    ACTIVE_INFERENCE,
    MANUAL_TRAINING
}

// Inference Engine both handles the training (based on which Engine it is), and then the inference (based on that training.) It calls the functions on the Agent and (...)
public abstract class InferenceEngine : MonoBehaviour
{
    [SerializeField] protected RUN_TYPE runType;
    // This setter and getter is used in case the Inference Engine Chooser is overriding the action
    public RUN_TYPE RunType
    {
        get { return runType; }
        set { runType = value; }
    }

    // Change it to "Run Data" and not "Training Data" because Inference can save the data too, should maybe change the name of the file based on the run type
    [SerializeField] protected bool saveRunData;
    public bool SaveRunData
    {
        get { return saveRunData; }
        set { saveRunData = value; }
    }

    [SerializeField] protected string trainingDataFileNumber;
    public string TrainingDataFileNumber
    {
        get { return trainingDataFileNumber; }
        set { trainingDataFileNumber = value; }
    }
    [SerializeField] protected bool verbose = false;
    public bool Verbose
    {
        get { return verbose; }
        set { verbose = value; }
    }
    
    // Based on the agents, its states will be used to create the training data
    [SerializeField] protected Agent agent;
    // CONSIDER: Need to use the TYPE for checking so the actual states and activities...do not go outside of the Agent
    protected Dictionary<ACTIVITY_TYPE, int> activityCounts;

    protected Dictionary<ACTIVITY_TYPE, PerformedActivityData> performedActivitiesData; // Used in run inference
    protected Dictionary<STATE_TYPE, Dictionary<ACTIVITY_TYPE, List<float>> > agentsPerformedActivities; // Also used in run inference. Per state that affects the Agent, the different actiivities that were tested are saved with a list of the values 

    protected int totalData = -1;
    protected List<InferenceData> trainingData;
    protected List<ACTIVITY_TYPE> activityTypes;

    protected string newTrainingDataFileName;
    protected string existingTrainingDataFileName;
    protected int fileCount;

    protected void Awake() {
        activityCounts = new Dictionary<ACTIVITY_TYPE, int>();
        performedActivitiesData = new Dictionary<ACTIVITY_TYPE, PerformedActivityData>();
        agentsPerformedActivities = new Dictionary<STATE_TYPE, Dictionary<ACTIVITY_TYPE, List<float>>>();
        trainingData = new List<InferenceData>();
        activityTypes = new List<ACTIVITY_TYPE>();
        fileCount = Directory.GetFiles("Assets/src/Data/TrainingData", "*.json").Length;
        newTrainingDataFileName = $"TrainingData{fileCount}";
    }

    // TODO: Change save data to the engine? Engine should hear what the agent is doing and based on that save the data
    // TODO: Save scriptable object in the future, rn will be a JSON that works because gets serialized from the trainingDataScriptableObject so the list is respected, can just open it later
    private void SaveData(InferenceDataWrapper trainingDataWrapper)
    {
        string trainingDataJSON = JsonSerialization.ToJson(trainingDataWrapper);
        print("Final Training Data JSON" + trainingDataJSON);
        string filePath = $"Assets/src/Data/TrainingData/{newTrainingDataFileName}.json";

        File.WriteAllText(filePath, trainingDataJSON);
    }

    public virtual void InitializeEngine()
    {
        StartEngine();
    }

    protected void StartEngine()
    {
        // Caches the activities from the agent
        activityTypes = agent.Activities;

        // Do in Start and not awake cause Agent needs to be initialized first. Could probably use some better architecture
        switch(runType){
            case RUN_TYPE.AUTOMATIC_TRAINING:
                print("Automatic training running");
                RunTraining();
                break;

            case RUN_TYPE.MANUAL_TRAINING:
                print("Manual training not implemented yet...what?");
                break;

            case RUN_TYPE.INFERENCE:
                print("Inference running");
                RunInference();
                break;

            case RUN_TYPE.ACTIVE_INFERENCE:
                print("Active Inference running");
                RunActiveInference();
                break;

            default:
                Debug.LogError("Run type not found");
                break;
        }
    }

    /*
    MAIN RUNS
    */

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

        agent.StartInfering(verbose);
    }

    protected void RunActiveInference()
    {
        print("Called active inference in Inference Engine");
        // Is repeated code from RunInference but want to keep it like this until I make it work
        foreach (STATE_TYPE state in agent.States)
        {
            agentsPerformedActivities[state] = new Dictionary<ACTIVITY_TYPE, List<float>>();
        }

        CalculatePriors();
        CalculateLikelihoods();

        agent.StartActiveInfering(verbose);
    }

    // Will be used only in Active Inference (for now)
    public void UpdateModel(List<InferenceData> data)
    {
        CalculatePriors();
        CalculateLikelihoods();
    }

    // Reads the data from a presaved JSON file
    protected void CacheTrainingData()
    {
        string filePath = $"Assets/src/Data/TrainingData/{existingTrainingDataFileName}.json";

        string jsonText = File.ReadAllText(filePath);
        print("JsonText: " + jsonText);

        InferenceDataWrapper trainingDataObject = JsonSerialization.FromJson<InferenceDataWrapper>(jsonText.ToString());

        print("TrainingDataObject: " + trainingDataObject.InferenceData.Count);
        trainingData = trainingDataObject.InferenceData;

        if (verbose)
        {
            // foreach (InferenceData data in trainingData)
            // {
            //     Debug.Log(JsonSerialization.ToJson(data));
            // }
        }
    }

    // protected void CalculatePriors()
    // {

    //     // Initialize dictionaries with all the known ACTIVITY_TYPE values
    //     foreach (ACTIVITY_TYPE activity in agent.Activities)
    //     {
    //         activityCounts[activity] = 0;
    //         // Per every state, it will create a list of every activivity that was tested and the values that were gotten (from that state and activity)
    //         foreach (STATE_TYPE state in agentsPerformedActivities.Keys)
    //         {
    //             agentsPerformedActivities[state][activity] = new List<float>();
    //         }
    //     }

    //     // Count occurrences the ocurrances of each actovoty (in activityCounts) and store the values of the states in the corresponding lists (in bathroomNeeds, sleepNeeds, foodNeeds, crimeRates
    //     foreach (InferenceData data in trainingData)
    //     {
    //         activityCounts[data.ChosenActivity]++;
    //         List<StateData> statesValues = data.StatesValues;

    //         // Will iterate over the States that are related with that InferenceData instance, should probably only be one for now (because Agent State mainAffectedState = statesDict[affectedStates[0]]; in ManuallyPerformActionForTraining)
    //         foreach (StateData state in statesValues)
    //         {
    //             STATE_TYPE stateType = state.StateType;
    //             agentsPerformedActivities[stateType][data.ChosenActivity].Add(data.GetStateValue(stateType));
    //         }
    //     }

    //     totalData = trainingData.Count;
    // }

    protected void CalculatePriors()
    {
        // Initialize dictionaries
        foreach (ACTIVITY_TYPE activity in agent.Activities)
        {
            activityCounts[activity] = 0;
            foreach (STATE_TYPE state in agent.States)
            {
                if (!agentsPerformedActivities.ContainsKey(state))
                {
                    agentsPerformedActivities[state] = new Dictionary<ACTIVITY_TYPE, List<float>>();
                }
                agentsPerformedActivities[state][activity] = new List<float>();
            }
        }

        // Count occurrences and store state values
        foreach (InferenceData data in trainingData)
        {
            activityCounts[data.ChosenActivity]++;
            foreach (StateData state in data.StatesValues)
            {
                agentsPerformedActivities[state.StateType][data.ChosenActivity].Add(state.Value);
            }
        }

        totalData = trainingData.Count;

        // Debug statements to check priors
        foreach (var kvp in activityCounts)
        {
            Debug.Log($"Activity: {kvp.Key}, Count: {kvp.Value}");
        }
    }

    // To really be dynamic, the activities list could be gotten from the InferenceData JSON itself. However, getting it from the current agent ensures that the agent has the Activities and the states. And it is important to ensure that the current agent with its current information is the one whos Training Data is being used, as no mixing should be done (maybe saving the agent and checking its the same would be great!)
    // protected void CalculateLikelihoods()
    // {
    //     // Calculate priors and likelihoods (mean and variance)
    //     foreach (ACTIVITY_TYPE activityType in agent.Activities)
    //     {
    //         float prior = (float)activityCounts[activityType] / totalData;
            
    //         List<GaussianInfo> statesData = new List<GaussianInfo>();

    //         foreach (STATE_TYPE stateType in agentsPerformedActivities.Keys)
    //         {
    //             float stateMean = CalculateAverage(agentsPerformedActivities[stateType][activityType]);
    //             float stateVariance = Variance(agentsPerformedActivities[stateType][activityType], stateMean);
    //             // Create a gaussian with the variance constructor, standard deviation will be -1
    //             GaussianInfo stateData = new GaussianInfo(stateType, stateMean, 0, 100, stateVariance);
    //             statesData.Add(stateData);
    //         }

    //         PerformedActivityData activityData = new PerformedActivityData(prior, statesData, activityType);
            
    //         performedActivitiesData[activityType] = activityData;
    //     }
    // }

    protected void CalculateLikelihoods()
    {
        foreach (ACTIVITY_TYPE activityType in agent.Activities)
        {
            float prior = (float)activityCounts[activityType] / totalData;
            List<GaussianInfo> statesData = new List<GaussianInfo>();

            foreach (STATE_TYPE stateType in agentsPerformedActivities.Keys)
            {
                List<float> values = agentsPerformedActivities[stateType][activityType];
                if (values.Count == 0) continue;

                float stateMean = CalculateAverage(values);
                float stateVariance = Variance(values, stateMean);

                GaussianInfo stateData = new GaussianInfo(stateType, stateMean, 0, 100, stateVariance);
                statesData.Add(stateData);

                // Debug statements to check means and variances
                Debug.Log($"Activity: {activityType}, State: {stateType}, Mean: {stateMean}, Variance: {stateVariance}");
            }

            PerformedActivityData activityData = new PerformedActivityData(prior, statesData, activityType);
            performedActivitiesData[activityType] = activityData;
        }
    }
    
    // protected float Variance(List<float> values, float mean)
    // {
    //     return values.Select(v => (v - mean) * (v - mean)).Sum() / values.Count;
    // }

    private float Variance(List<float> values, float mean)
    {
        if (values.Count == 0) return 0f;
        return values.Select(v => (v - mean) * (v - mean)).Sum() / values.Count;
    }

    // protected float CalculateAverage(List<float> values)
    // {
    //     if (values.Count == 0)
    //     {
    //         return 0f;
    //     }

    //     float sum = 0f;
    //     foreach (float value in values)
    //     {
    //         sum += value;
    //     }

    //     return sum / values.Count;
    // }

    private float CalculateAverage(List<float> values)
    {
        if (values.Count == 0) return 0f;
        float sum = values.Sum();
        return sum / values.Count;
    }

    // protected float GaussianProbability(float x, float mean, float variance)
    // {
    //     return (1 / Mathf.Sqrt(2 * Mathf.PI * variance)) * Mathf.Exp(-((x - mean) * (x - mean)) / (2 * variance));
    // }

    protected float GaussianProbability(float x, float mean, float variance)
    {
        // Add a small epsilon to variance to avoid division by zero
        float epsilon = 1e-6f;
        variance = Math.Max(variance, epsilon);

        float exponent = Mathf.Exp(-Mathf.Pow(x - mean, 2) / (2 * variance));
        return (1 / Mathf.Sqrt(2 * Mathf.PI * variance)) * exponent;
    }

    private void OnDisable()
    {
        if(saveRunData)
        {
            print("Saved Training Data not saved");
            InferenceDataWrapper trainingDataWrapper = new InferenceDataWrapper(agent.Activities, agent.States, agent.PerformedActivitiesData);
            SaveData(trainingDataWrapper);
        }
    }

    public abstract ACTIVITY_TYPE InferActivity(InferenceData currentStateValues);
    public abstract ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues);
    protected INFERENCE_ENGINE_TYPE inferenceEngineType;
}
