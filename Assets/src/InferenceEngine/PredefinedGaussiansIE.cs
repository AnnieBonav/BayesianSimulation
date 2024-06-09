// TODO: Somehow mark tha this cannot be used to train (architecture could also be better)
using System;
using System.Collections.Generic;
using UnityEngine;

public class PredefinedGaussiansIE : InferenceEngine
{
    // TODO: Check note bacause of change of Inference Engine.
    // An activity will not necessarily have a Gaussian Scriptable (morelike use it) given the implementation of the Inference Engine. Will need to review.
    [SerializeField] private GaussianScriptableObject statesGaussians;

    // Better to have as a dictionary than a list because of the O(1) access time
    private Dictionary<STATE_TYPE, PlotValues> statesGaussiansValues;
    public override void InitializeEngine()
    {
        inferenceEngineType = INFERENCE_ENGINE_TYPE.PREDEFINED_GAUSSIANS;

        // TODO: Check note bacause of change of Inference Engine.
        // statesGaussiansValues will have n KeyValuePairs (equal to the amount of gaussians in the atatched stateGaussians scriptable object), where the key will be the state and the value will be an instance of PlotValues that holds the x (value from 0-100) and y (corresponding probability) values of the gaussian
        statesGaussiansValues = new Dictionary<STATE_TYPE, PlotValues>();
        foreach(GaussianInfo gaussianInfo in statesGaussians.gaussians)
        {
            PlotValues plotValues = CacheGaussianValues(gaussianInfo.Mean, gaussianInfo.StandardDeviation, gaussianInfo.MinValue, gaussianInfo.MaxValue);
            statesGaussiansValues.Add(gaussianInfo.StateType, plotValues);
        }

        newTrainingDataFileName = $"PredefinedGaussians{fileCount}";
        existingTrainingDataFileName = $"PredefinedGaussians{trainingDataFileNumber}";
        base.InitializeEngine();
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

    // Was not sending activityType before (when this was in the Activity class)
    public float GetLogsSum(List<State> states, ACTIVITY_TYPE activityType, bool verbose = false)
    {
        float logsSum = 0;
        foreach(State state in states)
        {
            if (statesGaussiansValues.ContainsKey(state.StateType))
            {
                float stateLog = statesGaussiansValues[state.StateType].YValues[(int)state.CurrentValue];
                if (verbose)
                {
                    Debug.Log($"Activity: {activityType}, State: {state.StateType}, Value: {state.CurrentValue}, Log: {stateLog}");
                }
                logsSum += stateLog;
            }
        }
    
        return logsSum;
    }

    public override ACTIVITY_TYPE ChooseTrainingActivity(InferenceData trainingStateValues)
    {
        print("Predefined Gaussians cannot be used to train");
        throw new System.NotImplementedException();
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
        // float highestLogSum = Mathf.NegativeInfinity;
        // Activity chosenActivity = null;

        // foreach (Activity activity in activities)
        // {
        //     float logSum = activity.GetLogsSum(states, verbose);
        //     if (verbose) Debug.Log($"Activity: {activity.ActivityType}, Log Sum: {logSum}");
        //     if (logSum > highestLogSum)
        //     {
        //         highestLogSum = logSum;
        //         chosenActivity = activity;
        //     }
        // }
        // if (verbose) print("Chosen Activity: " + JsonSerialization.ToJson(chosenActivity));
        // return chosenActivity;
        return ACTIVITY_TYPE.NONE;
    }
}
