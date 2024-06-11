// TODO: Somehow mark tha this cannot be used to train (architecture could also be better)
using System;
using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEngine;

public class PredefinedGaussiansIE : InferenceEngine
{
    // // TODO: Check note bacause of change of Inference Engine.
    // // An activity will not necessarily have a Gaussian Scriptable (morelike use it) given the implementation of the Inference Engine. Will need to review.

    // // Better to have as a dictionary than a list because of the O(1) access time
    // public override void InitializeEngine()
    // {
    //     inferenceEngineType = INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS;

    //     // TODO: Check note bacause of change of Inference Engine.
    //     // statesGaussiansValues will have n KeyValuePairs (equal to the amount of gaussians in the atatched stateGaussians scriptable object), where the key will be the state and the value will be an instance of PlotValues that holds the x (value from 0-100) and y (corresponding probability) values of the gaussian
    //     statesGaussiansValues = new Dictionary<STATE_TYPE, PlotValues>();
    //     foreach(GaussianInfo gaussianInfo in statesGaussians.gaussians)
    //     {
    //         PlotValues plotValues = CacheGaussianValues(gaussianInfo.Mean, gaussianInfo.StandardDeviation, gaussianInfo.MinValue, gaussianInfo.MaxValue);
    //         statesGaussiansValues.Add(gaussianInfo.StateType, plotValues);
    //     }

    //     newTrainingDataFileName = $"PredefinedGaussians{fileCount}";
    //     existingTrainingDataFileName = $"PredefinedGaussians{trainingDataFileNumber}";
    //     base.InitializeEngine();
    // }

    // private PlotValues CacheGaussianValues(float mean, float standardDeviation, int minValue, int maxValue)
    // {
    //     int[] xValues = new int[maxValue + 1];
    //     float[] yValues = new float[maxValue + 1];
    //     for (int i = minValue; i <= maxValue; i++)
    //     {
    //         xValues[i] = i;
    //         yValues[i] = GaussianFunction(i, mean, standardDeviation);
    //     }

    //     return new PlotValues(xValues, yValues, mean, standardDeviation, minValue, maxValue);
    // }

    // private float GaussianFunction(float x, float mean, float standardDeviation)
    // {
    //     double xResult = 1 / (standardDeviation * Math.Sqrt(2 * Math.PI)) * Math.Exp(-Math.Pow(x - mean, 2) / (2 * Math.Pow(standardDeviation, 2)));
    //     float logxResult = (float)Math.Log(xResult);
    //     return logxResult;
    // }

    // // Was not sending activityType before (when this was in the Activity class)
    // public float GetLogsSum(List<State> states, ACTIVITY_TYPE activityType, bool verbose = false)
    // {
    //     float logsSum = 0;
    //     foreach(State state in states)
    //     {
    //         if (statesGaussiansValues.ContainsKey(state.StateType))
    //         {
    //             float stateLog = statesGaussiansValues[state.StateType].YValues[(int)state.CurrentValue];
    //             if (verbose)
    //             {
    //                 Debug.Log($"Activity: {activityType}, State: {state.StateType}, Value: {state.CurrentValue}, Log: {stateLog}");
    //             }
    //             logsSum += stateLog;
    //         }
    //     }

    //     return logsSum;
    // }

    private GaussianScriptableObject[] activitiesGaussiansScriptableObjects;
    private Dictionary<ACTIVITY_TYPE, Dictionary<STATE_TYPE, PlotValues>> activitiesGaussians;
    public override void InitializeEngine()
    {
        activitiesGaussiansScriptableObjects = Resources.LoadAll<GaussianScriptableObject>("Gaussians");
        print("Initializing Predefined Gaussians Inference Engine");
        print(JsonSerialization.ToJson(activitiesGaussiansScriptableObjects));
        for (int i = 0; i < activitiesGaussiansScriptableObjects.Length; i++)
        {
            print("Activity Type: " + activitiesGaussiansScriptableObjects[i].ActivityType);
        }

        activitiesGaussians = new Dictionary<ACTIVITY_TYPE, Dictionary<STATE_TYPE, PlotValues>>();
        foreach(GaussianScriptableObject gaussianScriptableObject in activitiesGaussiansScriptableObjects)
        {
            activitiesGaussians.Add(gaussianScriptableObject.ActivityType, new Dictionary<STATE_TYPE, PlotValues>());
            CacheActivitiesGaussians(gaussianScriptableObject);
        }

        base.InitializeEngine();
    }

    private void CacheActivitiesGaussians(GaussianScriptableObject gaussianScriptableObject)
    {
        foreach(GaussianInfo gaussianInfo in gaussianScriptableObject.gaussians)
        {
            print("Found Gaussian Info: " + JsonSerialization.ToJson(gaussianInfo));
            PlotValues plotValues = CacheGaussianValues(gaussianInfo.Mean, gaussianInfo.StandardDeviation, gaussianInfo.MinValue, gaussianInfo.MaxValue);
            activitiesGaussians[gaussianScriptableObject.ActivityType].Add(gaussianInfo.StateType, plotValues);
        }
    }

    private PlotValues CacheGaussianValues(float mean, float standardDeviation, int minValue, int maxValue)
    {
        int[] xValues = new int[maxValue + 1];
        float[] yValues = new float[maxValue + 1];
        for (int i = minValue; i <= maxValue; i++)
        {
            xValues[i] = i;
            yValues[i] = GaussianFunction(i, mean, standardDeviation);
        }

        return new PlotValues(xValues, yValues, mean, standardDeviation, minValue, maxValue);
    }

    private float GaussianFunction(float x, float mean, float standardDeviation)
    {
        double xResult = 1 / (standardDeviation * Math.Sqrt(2 * Math.PI)) * Math.Exp(-Math.Pow(x - mean, 2) / (2 * Math.Pow(standardDeviation, 2)));
        float logxResult = (float)Math.Log(xResult);
        return logxResult;
    }

    // Was used for getting normalized "random" values
    private float GenerateGaussianInRange(float mean, float stdDev, float lowerBound, float upperBound)
    {
        float value;
        do
        {
            value = GenerateGaussianValue(mean, stdDev);
        } while (value < lowerBound || value > upperBound);

        return Mathf.Round(value);
    }

    private float GenerateGaussianValue(float mean, float standardDeviation)
    {
        float u1 = UnityEngine.Random.value;
        float u2 = UnityEngine.Random.value;
        float z0 = Mathf.Sqrt(-2 * Mathf.Log(u1)) * Mathf.Cos(2 * Mathf.PI * u2);
        return mean + standardDeviation * z0;
    }

    public override ACTIVITY_TYPE InferActivity(InferenceData currentStateValues)
    {
        float highestLogSum = Mathf.NegativeInfinity;
        ACTIVITY_TYPE chosenActivityType = ACTIVITY_TYPE.NONE;

        foreach (ACTIVITY_TYPE activityType in activitiesGaussians.Keys)
        {
            print("Activity Type: " + activityType);
            List<State> states = agent.StatesForGaussians;
            float logSum = GetLogsSum(activityType, states, verbose);
            if (verbose) Debug.Log($"Activity Type: {activityType}, Log Sum: {logSum}");
            if (logSum > highestLogSum)
            {
                highestLogSum = logSum;
                chosenActivityType = activityType;
            }
        }
        if (verbose) print("Chosen Activity Type: " + chosenActivityType);
        return chosenActivityType;
    }

    public float GetLogsSum(ACTIVITY_TYPE activityType, List<State> states, bool verbose = false)
    {
        float logsSum = 0;
        foreach(State state in states)
        {
            if (activitiesGaussians[activityType].ContainsKey(state.StateType))
            {
                float stateLog = activitiesGaussians[activityType][state.StateType].YValues[(int)state.CurrentValue];
                if (verbose)
                {
                    Debug.Log($"Activity: {activityType}, State: {state.StateType}, Value: {state.CurrentValue}, Log: {stateLog}");
                }
                logsSum += stateLog;
            }
        }
    
        return logsSum;
    }

    protected override void RunAutomaticTraining()
    {
        throw new NotImplementedException();
    }

    protected override void RunInference()
    {
        print("Called inference in Predefined Guassians Inference Engine");
        agent.StartInfering(verbose);
    }
}
