using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class State : MonoBehaviour
{
    [SerializeField] private Need need;
    public Need Need => need;
    [SerializeField] private float increaseByMinute;

    [Header("Function Parameters")]
    [SerializeField] private FUNCTION_TYPE functionType;
    [SerializeField] private float a;
    [SerializeField] private float b;
    [SerializeField] private float c;

    [Header("UI Elements")]
    [SerializeField] private Image barImage;
    [SerializeField] private TextMeshProUGUI stateName;
    [SerializeField] private TextMeshProUGUI debugValue;

    private Func<float, float> probabilityFunction;
    [SerializeField] private float minLevel = 0;
    [SerializeField] private float maxLevel = 100;
    private Func<float, float, float, float> normalizationFunction;

    // These represents the min and max in normalization
    [SerializeField] private float minProbability = 0;
    [SerializeField] private float maxProbability = 1;

    [Header("Debugging")]
    // TODO: Probably make these visible?
    [ReadOnly] private float currentValue;
    public float CurrentValue => currentValue;

    private float[] xProbabilityValues;
    private float[] yProbabilityValues;

    private void Awake()
    {
        // TODO: Could save this in object?
        normalizationFunction = (currentValue, minProbability, maxProbability) => (currentValue - minProbability) * (maxLevel - minLevel) / (maxProbability - minProbability) + minLevel;

        // min_arr = np.min(arr)
        // max_arr = np.max(arr)
        // normalized_arr = (arr - min_arr) * (max_val - min_val) / (max_arr - min_arr) + min_val

        // TODO: Check, probably do not need the cs
        switch (functionType)
        {
            case FUNCTION_TYPE.Exponential:
                probabilityFunction = (currentValue) => a * Mathf.Exp(b * currentValue) + c;
                break;
            case FUNCTION_TYPE.Logarithmic:
                probabilityFunction = (currentValue) => a * Mathf.Log(b * currentValue) + c;
                break;
            case FUNCTION_TYPE.Square:
                probabilityFunction = (currentValue) => a * Mathf.Pow(b * currentValue, 2) + c ;
                break;
            case FUNCTION_TYPE.Linear:
                probabilityFunction = (currentValue) =>  a * currentValue + c;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        CacheProbabilityValues();
    }

    private void Start()
    {
        stateName.text = need.ToString();
        debugValue.text = currentValue.ToString();
    }

    private void CacheProbabilityValues()
    {
        // If I want more points, get them
        float[] xValues = new float[100];
        float[] yValues = new float[100];
        for (int i = 0; i < 100; i++)
        {
            float currentValue = i * 0.1f;
            yValues[i] = probabilityFunction(currentValue);
        }
        xProbabilityValues = xValues;
        yProbabilityValues = yValues;

        float minProbability = Mathf.Min(yValues);
        float maxProbability = Mathf.Max(yValues);

        for (int i = 0; i < 100; i++)
        {
            float currentValue = i * 0.1f;
            yProbabilityValues[i] = normalizationFunction(currentValue, minProbability, maxProbability);
        }

        return;
    }

    public float GetProbability(bool verbose = false)
    {
        // Need to make sure currentValue is always between 0 and 100
        int index = (int)currentValue;
        float probability = yProbabilityValues[index];
        if (verbose) Debug.Log($"{need}: the y value for x = {currentValue} (with index {index}) is {probability}");
        return probability;
    }

    public void Increase(float value)
    {
        currentValue += value;
        if (currentValue > maxProbability)
        {
            currentValue = maxProbability;
        }

        currentValue = Mathf.Round(currentValue * 1000f) / 1000f;
        barImage.fillAmount = currentValue/100f;
        debugValue.text = currentValue.ToString();
    }

    public void Decrease(float value)
    {
        currentValue -= value;
        if (currentValue < minProbability)
        {
            currentValue = minProbability;
        }

        barImage.fillAmount = 1 - (currentValue/100);
        debugValue.text = currentValue.ToString();
        Debug.Log($"{need}: {currentValue}");
    }

    public override string ToString()
    {
        return $"{need}: {currentValue}";
    }
}
