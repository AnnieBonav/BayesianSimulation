using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using System.Linq;

public class DataTrainer : MonoBehaviour
{
    
    // TODO: Change to READ only
    // [SerializeField] private Dictionary<ACTIVITY_TYPE, ActivityData> activitiesData;
    private List<ACTIVITY_TYPE> activityTypes = new List<ACTIVITY_TYPE> { ACTIVITY_TYPE.Bathroom, ACTIVITY_TYPE.Sleep, ACTIVITY_TYPE.Food, ACTIVITY_TYPE.Relax };

    private void Awake() {
        // activityCounts = new Dictionary<ACTIVITY_TYPE, int>();
        // bathroomNeeds = new Dictionary<ACTIVITY_TYPE, List<float>>();
        // sleepNeeds = new Dictionary<ACTIVITY_TYPE, List<float>>();
        // foodNeeds = new Dictionary<ACTIVITY_TYPE, List<float>>();
        // crimeRates = new Dictionary<ACTIVITY_TYPE, List<float>>();

        // trainingData = new List<TrainingData>();
        // activitiesData = new Dictionary<ACTIVITY_TYPE, ActivityData>();
        // CacheTrainingData();
    }

    private void Start()
    {
        // CalculatePriors();
        // CalculateLikelihoods();
    }

    // TODO: Add a TrainingData scriptable object that gets filled with it so I can have a look at it in the editor?
    // private void CacheTrainingData()
    // {
    //     string jsonPath = $"Assets/src/Data/TrainingData/TrainedData{trainingDataFileNumber}.json";
    //     string jsonText = System.IO.File.ReadAllText(jsonPath);
    //     print("JsonText: " + jsonText);
    //     // TrainingDataScriptableObject trainingDataObject = JsonUtility.FromJson<TrainingDataScriptableObject>(jsonText.ToString());

    //     TrainingDataWrapper trainingDataObject = JsonUtility.FromJson<TrainingDataWrapper>(jsonText.ToString());

    //     print("TrainingDataObject: " + trainingDataObject.TrainingData.Count);
    //     trainingData = trainingDataObject.TrainingData;

    //     if (verbose)
    //     {
    //         foreach (TrainingData data in trainingData)
    //         {
    //             Debug.Log(data.ToJson());
    //         }
    //     }
    // }
    
    // TODO: would need to iterate over the states in the training data to make it dynamic
    // private void CalculatePriors()
    // {

    //     // Initialize dictionaries with all the known ACTIVITY_TYPE values
    //     // TODO would need to make it dynamic if I need to control which activities affect it
    //     foreach (ACTIVITY_TYPE activity in Enum.GetValues(typeof(ACTIVITY_TYPE)))
    //     {
    //         activityCounts[activity] = 0;
    //         bathroomNeeds[activity] = new List<float>();
    //         sleepNeeds[activity] = new List<float>();
    //         foodNeeds[activity] = new List<float>();
    //         crimeRates[activity] = new List<float>();
    //     }

    //     // Count occurrences the ocurrances of each actovoty (in activityCounts) and store the values of the states in the corresponding lists (in bathroomNeeds, sleepNeeds, foodNeeds, crimeRates
    //     foreach (TrainingData data in trainingData)
    //     {
    //         activityCounts[data.ChosenActivity]++;
    //         bathroomNeeds[data.ChosenActivity].Add(data.BathroomNeed);
    //         sleepNeeds[data.ChosenActivity].Add(data.SleepNeed);
    //         foodNeeds[data.ChosenActivity].Add(data.FoodNeed);
    //         crimeRates[data.ChosenActivity].Add(data.CrimeRate);
    //     }

    //     totalData = trainingData.Count;
    // }

    // private float CalculateAverage(List<float> values)
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

    // private void CalculateLikelihoods()
    // {
    //     // Calculate priors and likelihoods (mean and variance)
    //     foreach (ACTIVITY_TYPE activity in Enum.GetValues(typeof(ACTIVITY_TYPE)))
    //     {
    //         float prior = (float)activityCounts[activity] / totalData;

    //         float bathroomMean = CalculateAverage(bathroomNeeds[activity]);
    //         float bathroomVariance = Variance(bathroomNeeds[activity], bathroomMean);
    //         ActivityOneStateData bathroomData = new ActivityOneStateData(bathroomMean, bathroomVariance, STATE_TYPE.BathroomNeed);

    //         float sleepMean = CalculateAverage(sleepNeeds[activity]);
    //         float sleepVariance = Variance(sleepNeeds[activity], sleepMean);
    //         ActivityOneStateData sleepData = new ActivityOneStateData(sleepMean, sleepVariance, STATE_TYPE.SleepNeed);

    //         float foodMean = CalculateAverage(foodNeeds[activity]);
    //         float foodVariance = Variance(foodNeeds[activity], foodMean);
    //         ActivityOneStateData foodData = new ActivityOneStateData(foodMean, foodVariance, STATE_TYPE.FoodNeed);

    //         float crimeMean = CalculateAverage(crimeRates[activity]);
    //         float crimeVariance = Variance(crimeRates[activity], crimeMean);
    //         ActivityOneStateData crimeData = new ActivityOneStateData(crimeMean, crimeVariance, STATE_TYPE.CrimeRate);

    //         ActivityData activityData = new ActivityData(prior, bathroomData, sleepData, foodData, crimeData, activity);
    //         activitiesData[activity] = activityData;
    //     }
    // }

    // private float GaussianProbability(float x, float mean, float variance)
    // {
    //     return (1 / Mathf.Sqrt(2 * Mathf.PI * variance)) * Mathf.Exp(-((x - mean) * (x - mean)) / (2 * variance));
    // }

    // private float Variance(List<float> values, float mean)
    // {
    //     return values.Select(v => (v - mean) * (v - mean)).Sum() / values.Count;
    // }
}
