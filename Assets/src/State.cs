using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class State : MonoBehaviour
{
    [SerializeField]
    private Need need;
    public Need Need => need;

    [SerializeField]
    private Image barImage;

    [SerializeField]
    private TextMeshProUGUI stateName;

    [SerializeField]
    private TextMeshProUGUI debugValue;

    private Func<float[]> probabilityFunction;

    // TODO: Probably make these visible?
    private float minValue = 0;
    private float maxValue = 100;

    [SerializeField]
    private float currentValue;
    public float CurrentValue => currentValue;

    private float[] probabilityValues;

    public void Start()
    {
        // probabilityValues = CacheProbabilityValues();
        stateName.text = need.ToString();
        debugValue.text = currentValue.ToString();
    }

    private float[] CacheProbabilityValues()
    {
        probabilityValues = probabilityFunction();
        return probabilityValues;
    }

    public float GetProbability(bool verbose = false)
    {
        int index = Array.IndexOf(probabilityValues, Mathf.Abs(probabilityValues[0] - currentValue));
        float probability = probabilityValues[index];
        if (verbose) Debug.Log($"{need}: the y value for x = {currentValue} (with index {index}) is {probability}");
        return probability;
    }

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
            currentValue = minValue;
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
