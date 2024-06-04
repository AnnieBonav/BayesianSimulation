using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    protected Dictionary<ACTIVITY_TYPE, ActivityData> activitiesData;
    protected Dictionary<STATE_TYPE, Dictionary<ACTIVITY_TYPE, List<float>> > statesOfAgent;

    // To remove all of these, will have a dictionary of dictionaries of the data that get dinamically generated based on which needa affect the agent. The dictionary is a dictionary of states, and a keyvalue pair of the activity type and the list of values.
    // private Dictionary<ACTIVITY_TYPE, int> activityCounts;
    // private Dictionary<ACTIVITY_TYPE, List<float>> bathroomNeeds;
    // private Dictionary<ACTIVITY_TYPE, List<float>> sleepNeeds;
    // private Dictionary<ACTIVITY_TYPE, List<float>> crimeRates;
    // private Dictionary<ACTIVITY_TYPE, List<float>> foodNeeds;
    protected int totalData = -1;

    // Will need to change naming because one will be the stored training adat (trainingData) and the other the actively got trainedData (will be gotten from the agent for now)
    protected List<TrainingData> trainingData;
    // private List<TrainingData> trainedData;

    protected void Awake() {
        statesOfAgent = new Dictionary<STATE_TYPE, Dictionary<ACTIVITY_TYPE, List<float>>>();
        activityCounts = new Dictionary<ACTIVITY_TYPE, int>();
        activitiesData = new Dictionary<ACTIVITY_TYPE, ActivityData>();
        trainingData = new List<TrainingData>();
        InitializeEngine();
    }

    private void OnDisable() {
        SaveTrainingData();
    }

    protected void SaveTrainingData()
    {
        if(saveTrainingData)
        {
            TrainingDataWrapper trainingDataWrapper = new TrainingDataWrapper(agent.PerformedActivitiesData);
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
        string trainingDataJSON = JsonUtility.ToJson(trainingDataWrapper);
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
        agent.StartTraining();
    }

    protected void RunInference()
    {
        print("Called inference in Inference Engine");

        foreach (STATE_TYPE state in agent.States)
        {
            statesOfAgent[state] = new Dictionary<ACTIVITY_TYPE, List<float>>();
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
        string jsonText = System.IO.File.ReadAllText(jsonPath);
        print("JsonText: " + jsonText);

        TrainingDataWrapper trainingDataObject = JsonUtility.FromJson<TrainingDataWrapper>(jsonText.ToString());

        print("TrainingDataObject: " + trainingDataObject.TrainingData.Count);
        trainingData = trainingDataObject.TrainingData;

        if (verbose)
        {
            foreach (TrainingData data in trainingData)
            {
                Debug.Log(data.ToJson());
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
            foreach (STATE_TYPE state in statesOfAgent.Keys)
            {
                statesOfAgent[state][activity] = new List<float>();
            }
            // bathroomNeeds[activity] = new List<float>();
            // sleepNeeds[activity] = new List<float>();
            // foodNeeds[activity] = new List<float>();
            // crimeRates[activity] = new List<float>();
        }

        // Count occurrences the ocurrances of each actovoty (in activityCounts) and store the values of the states in the corresponding lists (in bathroomNeeds, sleepNeeds, foodNeeds, crimeRates
        foreach (TrainingData data in trainingData)
        {
            activityCounts[data.ChosenActivity]++;
            foreach (STATE_TYPE state in statesOfAgent.Keys)
            {
                statesOfAgent[state][data.ChosenActivity].Add(data.GetStateValue(state));
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

    protected void CalculateLikelihoods()
    {
        // Calculate priors and likelihoods (mean and variance)
        foreach (ACTIVITY_TYPE activity in Enum.GetValues(typeof(ACTIVITY_TYPE)))
        {
            float prior = (float)activityCounts[activity] / totalData;
            
            Dictionary<STATE_TYPE, ActivityOneStateData> statesData = new Dictionary<STATE_TYPE, ActivityOneStateData>();

            foreach (STATE_TYPE state in statesOfAgent.Keys)
            {
                float stateMean = CalculateAverage(statesOfAgent[state][activity]);
                float stateVariance = Variance(statesOfAgent[state][activity], stateMean);
                ActivityOneStateData stateData = new ActivityOneStateData(stateMean, stateVariance, state);
                statesData[state] = stateData;
            }

            // float bathroomMean = CalculateAverage(bathroomNeeds[activity]);
            // float bathroomVariance = Variance(bathroomNeeds[activity], bathroomMean);
            // ActivityOneStateData bathroomData = new ActivityOneStateData(statesOfAgent[STATE_TYPE.BathroomNeed].Mean, bathroomVariance, STATE_TYPE.BathroomNeed);

            // float sleepMean = CalculateAverage(sleepNeeds[activity]);
            // float sleepVariance = Variance(sleepNeeds[activity], sleepMean);
            // ActivityOneStateData sleepData = new ActivityOneStateData(sleepMean, sleepVariance, STATE_TYPE.SleepNeed);

            // float foodMean = CalculateAverage(foodNeeds[activity]);
            // float foodVariance = Variance(foodNeeds[activity], foodMean);
            // ActivityOneStateData foodData = new ActivityOneStateData(foodMean, foodVariance, STATE_TYPE.FoodNeed);

            // float crimeMean = CalculateAverage(crimeRates[activity]);
            // float crimeVariance = Variance(crimeRates[activity], crimeMean);
            // ActivityOneStateData crimeData = new ActivityOneStateData(crimeMean, crimeVariance, STATE_TYPE.CrimeRate);

            // ActivityData activityData = new ActivityData(prior, statesOfAgent[STATE_TYPE.BathroomNeed],statesOfAgent[STATE_TYPE.SleepNeed], statesOfAgent[STATE_TYPE.FoodNeed], statesOfAgent[STATE_TYPE.CrimeRate], activity);
            ActivityData activityData = new ActivityData(prior, statesData[STATE_TYPE.BathroomNeed], statesData[STATE_TYPE.SleepNeed], statesData[STATE_TYPE.FoodNeed], statesData[STATE_TYPE.CrimeRate], activity);
            
            // TODO: Change so Activity Data gets the statesOfAgent dictionary and dynamically generates the other data
            // hroomData, sleepData, foodData, crimeData, activity);
            activitiesData[activity] = activityData;
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
    
    public abstract ACTIVITY_TYPE ChooseActivity(TrainingData currentStateValues);
    public abstract void InitializeEngine();
    protected INFERENCE_ENGINE_TYPE inferenceEngineType;
}
