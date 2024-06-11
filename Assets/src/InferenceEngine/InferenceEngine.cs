using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Serialization.Json;
using UnityEngine;

public enum RUN_TYPE
{
    AUTOMATIC_TRAINING,
    MANUAL_TRAINING,
    // If I do inference with Active Inference Engine, then it is active inference
    INFERENCE
}

public enum DATA_TRAINER_TYPE
{
    RANDOM,
    HEURISTICS,
    COMBINED
}

public enum INFERENCE_ENGINE_TYPE
{
    NONE,
    STANDARD_INFERENCE,
    ACTIVE_INFERENCE,
    PREDEFINED_GAUSSIANS
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

    // Train single state would be that only bathroom need is taken into consideration for the bathroom activity, sleep need for sleep activity, etc. This is to train one activity at a time, except the idle activity (relax) which is trained with all the states
    [SerializeField] protected bool trainSingleState = false;
    public bool TrainSingleState
    {
        get { return trainSingleState; }
        set { trainSingleState = value; }
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

    [SerializeField] protected bool verboseInitialData = false;
    public bool VerboseInitialData
    {
        get { return verboseInitialData; }
        set { verboseInitialData = value; }
    }

    [SerializeField] protected DATA_TRAINER_TYPE dataTrainerType;
    public DATA_TRAINER_TYPE DataTrainerType
    {
        get { return dataTrainerType; }
        set { dataTrainerType = value; }
    }

    protected DataTrainer dataTrainer;
    public DataTrainer DataTrainer
    {
        get { return dataTrainer; }
        set { dataTrainer = value; }
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
        // Creates a Data Trainer based on the type, using a factory method
        dataTrainer = DataTrainer.CreateDataTrainer(dataTrainerType);

        // Initializes the dictionaries
        activityCounts = new Dictionary<ACTIVITY_TYPE, int>();
        performedActivitiesData = new Dictionary<ACTIVITY_TYPE, PerformedActivityData>();
        agentsPerformedActivities = new Dictionary<STATE_TYPE, Dictionary<ACTIVITY_TYPE, List<float>>>();
        trainingData = new List<InferenceData>();
        activityTypes = new List<ACTIVITY_TYPE>();

        // Handles file names
        fileCount = Directory.GetFiles("Assets/src/Data/TrainingData", "*.json").Length;
        newTrainingDataFileName = $"TrainingData{fileCount}";
    }

    // Created so that the inherited classes can do their own thing, set everything they need and then call the base method.
    public virtual void InitializeEngine()
    {
        StartEngine();
    }

    protected void StartEngine()
    {
        // Caches the activities from the agent
        activityTypes = agent.Activities;

        // Do in Start and not awake cause Agent needs to be initialized first. Could probably use some better architecture
        // Calls the training method that will use what is implemented in each Inference Engine.
        switch(runType){
            case RUN_TYPE.AUTOMATIC_TRAINING:
                print("Automatic training running");
                RunAutomaticTraining();
                break;

            case RUN_TYPE.MANUAL_TRAINING:
                // TODO: Make the world not change states to avoid confusion
                print("Manual training not implemented yet...what?");
                break;

            case RUN_TYPE.INFERENCE:
                print("Inference running");
                RunInference();
                break;

            default:
                Debug.LogError("Run type not found");
                break;
        }
    }

    /*
    MAIN RUNS
    */

    // Reads the data from a presaved JSON file
    protected void CacheTrainingData()
    {
        string filePath = $"Assets/src/Data/TrainingData/{existingTrainingDataFileName}.json";
        string jsonText = File.ReadAllText(filePath);
        print("JsonText: " + jsonText);

        InferenceDataWrapper trainingDataObject = JsonSerialization.FromJson<InferenceDataWrapper>(jsonText.ToString());

        print("TrainingDataObject: " + trainingDataObject.InferenceData.Count);
        trainingData = trainingDataObject.InferenceData;

        if (verboseInitialData)
        {
            foreach (InferenceData data in trainingData)
            {
                print(JsonSerialization.ToJson(data));
            }
        }
    }

    protected void CalculatePriors()
    {
        // Initialize dictionaries with the data in the agent
        foreach (ACTIVITY_TYPE activity in agent.Activities)
        {
            activityCounts[activity] = 0;
            // Creates a list of every activity that was tested and the values that were gotten (from that state and activity)
            foreach (STATE_TYPE state in agent.States)
            {
                // TODO: Check why it does this
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
                print($"Activity: {activityType}, State: {stateType}, Mean: {stateMean}, Variance: {stateVariance}");
            }

            PerformedActivityData activityData = new PerformedActivityData(prior, statesData, activityType);
            performedActivitiesData[activityType] = activityData;
        }
    }

    private float Variance(List<float> values, float mean)
    {
        if (values.Count == 0) return 0f;
        return values.Select(v => (v - mean) * (v - mean)).Sum() / values.Count;
    }

    private float CalculateAverage(List<float> values)
    {
        if (values.Count == 0) return 0f;
        float sum = values.Sum();
        return sum / values.Count;
    }

    protected float GaussianProbability(float x, float mean, float variance)
    {
        // Add a small epsilon to variance to avoid division by zero
        float epsilon = 1e-6f;
        variance = Math.Max(variance, epsilon);

        float exponent = Mathf.Exp(-Mathf.Pow(x - mean, 2) / (2 * variance));
        return (1 / Mathf.Sqrt(2 * Mathf.PI * variance)) * exponent;
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
    private void OnDisable()
    {
        if(saveRunData)
        {
            print("Saved Training Data not saved");
            InferenceDataWrapper trainingDataWrapper = new InferenceDataWrapper(agent.Activities, agent.States, agent.PerformedActivitiesData);
            SaveData(trainingDataWrapper);
        }
    }

    public virtual ACTIVITY_TYPE InferActivityBase(InferenceData currentStateValues)
    {
        Dictionary<ACTIVITY_TYPE, float> logPosteriorProbabilities = new Dictionary<ACTIVITY_TYPE, float>();

        foreach (ACTIVITY_TYPE activity in activityTypes)
        {
            if (performedActivitiesData.TryGetValue(activity, out PerformedActivityData performedActivityData))
            {
                float logPrior = Mathf.Log(performedActivityData.Prior); // Retrieve and log the prior
                float logPosterior = logPrior;

                foreach (StateData stateData in currentStateValues.StatesValues)
                {
                    if (performedActivityData.StatesData.TryGetValue(stateData.StateType, out GaussianInfo stateStatistics))
                    {
                        float stateLogLikelihood = Mathf.Log(GaussianProbability(stateData.Value, stateStatistics.Mean, stateStatistics.Variance));
                        logPosterior += stateLogLikelihood;
                    }
                }

                logPosteriorProbabilities[activity] = logPosterior;
            }
        }

        // Gets the activity with the highest log posterior probability
        return logPosteriorProbabilities.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
    }

    protected abstract void RunAutomaticTraining();
    protected abstract void RunInference();
    public abstract ACTIVITY_TYPE InferActivity(InferenceData currentStateValues);
    protected INFERENCE_ENGINE_TYPE inferenceEngineType;
}
