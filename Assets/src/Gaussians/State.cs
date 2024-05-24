using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

// STATE_TYPE is an enum that represents the different states that an agent can have, which are the values that it receives (both internally and externally) and that it can change through its Actions (or simply change over time)
public enum STATE_TYPE
{
    BathroomNeed,
    SleepNeed,
    FoodNeed,
    CrimeRate,
}
// FUNCTION_TYPE was the representation of the different types of functions that mapped the current STATE value to the probability of choosing it, but now Gaussians will be used.
// FUNCTION_TYPE might become the function that explains the rate of change of the STATE value over time.
public enum FUNCTION_TYPE
{
    Exponential,
    Logarithmic,
    Square,
    Linear
}
public class State : MonoBehaviour
{
    [SerializeField] private STATE_TYPE stateType;
    public STATE_TYPE StateType => stateType;
    [ReadOnly] private float currentValue;
    public float CurrentValue => currentValue;
    [Header("Values Parameters")]
    [SerializeField] private float minValue = 0;
    public float MinValue => minValue;

    [SerializeField] private float maxValue = 100;
    public float MaxValue => maxValue;

    [Header("UI Elements")]
    [SerializeField] private Image barImage;
    [SerializeField] private TextMeshProUGUI stateName;
    [SerializeField] private TextMeshProUGUI debugValue;

    [Header("Gaussian Parameters")]
    [SerializeField] private float mean;
    [SerializeField] private float standardDeviation;

    // private Func<float, float> probabilityFunction;
    // [SerializeField] private float minLevel = 0;
    // [SerializeField] private float maxLevel = 100;
    // private Func<float, float, float, float> normalizationFunction;

    // // These represents the min and max in normalization
    // [SerializeField] private float minProbability = 0;
    // [SerializeField] private float maxProbability = 1;


    private void Awake()
    {
        // TODO: Could save this in object?
        // normalizationFunction = (currentValue, minProbability, maxProbability) => (currentValue - minProbability) * (maxLevel - minLevel) / (maxProbability - minProbability) + minLevel;

       
        // TODO: Check, probably do not need the cs
        // switch (functionType)
        // {
        //     case FUNCTION_TYPE.Exponential:
        //         probabilityFunction = (currentValue) => a * Mathf.Exp(b * currentValue) + c;
        //         break;
        //     case FUNCTION_TYPE.Logarithmic:
        //         probabilityFunction = (currentValue) => a * Mathf.Log(b * currentValue) + c;
        //         break;
        //     case FUNCTION_TYPE.Square:
        //         probabilityFunction = (currentValue) => a * Mathf.Pow(b * currentValue, 2) + c ;
        //         break;
        //     case FUNCTION_TYPE.Linear:
        //         probabilityFunction = (currentValue) =>  a * currentValue + c;
        //         break;
        //     default:
        //         throw new ArgumentOutOfRangeException();
        // }
        // CacheProbabilityValues();
    }

    private void Start()
    {
        // stateName.text = need.ToString();
        // debugValue.text = currentValue.ToString();
    }

    // private void CacheProbabilityValues()
    // {
    //     // If I want more points, get them
    //     float[] xValues = new float[100];
    //     float[] yValues = new float[100];
    //     for (int i = 0; i < 100; i++)
    //     {
    //         float currentValue = i * 0.1f;
    //         yValues[i] = probabilityFunction(currentValue);
    //     }
    //     xProbabilityValues = xValues;
    //     yProbabilityValues = yValues;

    //     float minProbability = Mathf.Min(yValues);
    //     float maxProbability = Mathf.Max(yValues);

    //     for (int i = 0; i < 100; i++)
    //     {
    //         float currentValue = i * 0.1f;
    //         yProbabilityValues[i] = normalizationFunction(currentValue, minProbability, maxProbability);
    //     }

    //     return;
    // }

    // public float GetProbability(bool verbose = false)
    // {
    //     // Need to make sure currentValue is always between 0 and 100
    //     int index = (int)currentValue;
    //     float probability = yProbabilityValues[index];
    //     if (verbose) Debug.Log($"{need}: the y value for x = {currentValue} (with index {index}) is {probability}");
    //     return probability;
    // }

    public void Increase(float value)
    {
        currentValue += value;
        if (currentValue > maxValue)
        {
            currentValue = maxValue;
        }

        currentValue = Mathf.Round(currentValue * 1000f) / 1000f;
        barImage.fillAmount = currentValue/100f;
        debugValue.text = currentValue.ToString();
    }

    public void Decrease(float value)
    {
        currentValue -= value;
        if (currentValue < minValue)
        {
            currentValue = maxValue;
        }

        barImage.fillAmount = 1 - (currentValue/100);
        debugValue.text = currentValue.ToString();
        Debug.Log($"{stateType}: {currentValue}");
    }

    public override string ToString()
    {
        return $"{stateType}: {currentValue}";
    }
}
