using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.XPath;

public enum ACTIVITY_TYPE
{
    None,
    Bathroom,
    Sleep,
    Food,
    Relax,
}

public class PlotValues
{
    public PlotValues(int[] xValues, float[] yValues)
    {
        this.xValues = xValues;
        this.yValues = yValues;
    }

    private int[] xValues;
    public int[] XValues => xValues;
    private float[] yValues;
    public float[] YValues => yValues;
}

// Basically the Activities are the different classses the classifier will choose from, they all have different gaussians on how the States affect them
public class Activity : MonoBehaviour
{
    [SerializeField] private ACTIVITY_TYPE activityType;
    public ACTIVITY_TYPE ActivityType => activityType;

    [SerializeField] private GaussianScriptableObject statesGaussians;
    [SerializeField] private List<Action> possibleActions;

    private Dictionary<STATE_TYPE, PlotValues> statesGaussiansValues;

    private void Awake()
    {
        // statesGaussiansValues will have n KeyValuePairs (equal to the amount of gaussians in the atatched stateGaussians scriptable object), where the key will be the state and the value will be an instance of PlotValues that holds the x (value from 0-100) and y (corresponding probability) values of the gaussian
        statesGaussiansValues = new Dictionary<STATE_TYPE, PlotValues>();
        foreach(GaussianInfo gaussianInfo in statesGaussians.gaussians)
        {
            PlotValues plotValues = CacheGaussianValues(gaussianInfo.mean, gaussianInfo.standardDeviation, gaussianInfo.minValue, gaussianInfo.maxValue);
            statesGaussiansValues.Add(gaussianInfo.stateType, plotValues);
        }
    }

    public float GetLogsSum(List<State> states, bool verbose = false)
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

    private float GaussianFunction(float x, float mean, float standardDeviation)
    {
        double xResult = 1 / (standardDeviation * Math.Sqrt(2 * Math.PI)) * Math.Exp(-Math.Pow(x - mean, 2) / (2 * Math.Pow(standardDeviation, 2)));
        float logxResult = (float)Math.Log(xResult);
        return logxResult;
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

        return new PlotValues(xValues, yValues);
    }
}
