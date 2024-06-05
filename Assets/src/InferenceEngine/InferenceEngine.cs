using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Serialization.Json;
using UnityEngine;

public enum RUN_TYPE
{
    Training,
    Inference
}

// Could be both a data trainer AND getting the inferences, but might be duplicating the getting the data code, will need to check. BUT NO because getting the data depends on which way of getting the data we want. Will implement both for now.
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

    // To remove all of these, will have a dictionary of dictionaries of the data that get dinamically generated based on which needa affect the agent. The dictionary is a dictionary of states, and a keyvalue pair of the activity type and the list of values.
    // private Dictionary<ACTIVITY_TYPE, int> activityCounts;
    // private Dictionary<ACTIVITY_TYPE, List<float>> bathroomNeeds;
    // private Dictionary<ACTIVITY_TYPE, List<float>> sleepNeeds;
    // private Dictionary<ACTIVITY_TYPE, List<float>> crimeRates;
    // private Dictionary<ACTIVITY_TYPE, List<float>> foodNeeds;
    protected int totalData = -1;

    // Will need to change naming because one will be the stored training adat (trainingData) and the other the actively got trainedData (will be gotten from the agent for now)
    protected List<InferenceData> trainingData;
    // private List<InferenceData> trainedData;

    protected void Awake() {
        activityCounts = new Dictionary<ACTIVITY_TYPE, int>();
        performedActivitiesData = new Dictionary<ACTIVITY_TYPE, PerformedActivityData>();
        agentsPerformedActivities = new Dictionary<STATE_TYPE, Dictionary<ACTIVITY_TYPE, List<float>>>();
        trainingData = new List<InferenceData>();
        InitializeEngine();
    }

    private void OnDisable() {
        SaveTrainingData();
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

        int fileCount = Directory.GetFiles("Assets/src/Data/TrainingData").Length;
        string filePath = $"Assets/src/Data/TrainingData/TrainedData{fileCount}.json";

        File.WriteAllText(filePath, trainingDataJSON);
    }

    protected void Start()
    {
        // Do in Start and not awake cause Agent needs to be initialized first. Could probably use some better architecture
        if (runType == RUN_TYPE.Training)
        {
            RunTraining();
        }
        else
        {
            RunInference();
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
        string jsonPath = $"Assets/src/Data/TrainingData/TrainedData{trainingDataFileNumber}.json";
        string jsonText = File.ReadAllText(jsonPath);
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
            // Per every state, it will create a list of every actovivity that was tested and the values that were gotten (from that state and activity)
            foreach (STATE_TYPE state in agentsPerformedActivities.Keys)
            {
                agentsPerformedActivities[state][activity] = new List<float>();
            }
            // bathroomNeeds[activity] = new List<float>();
            // sleepNeeds[activity] = new List<float>();
            // foodNeeds[activity] = new List<float>();
            // crimeRates[activity] = new List<float>();
        }

        // Count occurrences the ocurrances of each actovoty (in activityCounts) and store the values of the states in the corresponding lists (in bathroomNeeds, sleepNeeds, foodNeeds, crimeRates
        foreach (InferenceData data in trainingData)
        {
            activityCounts[data.ChosenActivity]++;
            foreach (STATE_TYPE state in agentsPerformedActivities.Keys)
            {
                agentsPerformedActivities[state][data.ChosenActivity].Add(data.GetStateValue(state));
            }
            // bathroomNeeds[data.ChosenActivity].Add(data.BathroomNeed);
            // sleepNeeds[data.ChosenActivity].Add(data.SleepNeed);
            // foodNeeds[data.ChosenActivity].Add(data.FoodNeed);
            // crimeRates[data.ChosenActivity].Add(data.CrimeRate);
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

    // To really be dynamic, the activities list should be gotten from the json itself, not from the current agent, as a prior agent could have different activities (but also no cause a dataset trained with different activities would not be useful for the current agent)
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
    
    public abstract ACTIVITY_TYPE ChooseActivity(InferenceData currentStateValues);
    public abstract void InitializeEngine();
    protected INFERENCE_ENGINE_TYPE inferenceEngineType;
}
