using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

// STATE_TYPE is an enum that represents the different states that an agent can have, which are the values that it receives (both internally and externally) and that it can change through its Actions (or simply change over time)
public enum STATE_TYPE
{
    None,
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

    private void UpdateUI()
    {
        barImage.fillAmount = currentValue/100;
        debugValue.text = currentValue.ToString();
        // Debug.Log($"{stateType}: {currentValue}");
    }

    public void UpdateValue(float value)
    {
        currentValue = value;
        if (currentValue > maxValue)
        {
            currentValue = maxValue;
        }
        else if (currentValue < minValue)
        {
            currentValue = minValue;
        }
        UpdateUI();
    }

    // TODO: Could reduce Increase and Decrease to Only Affect
    public void Increase(float value, bool verbose = false)
    {
        if (verbose) print("State Type Name" + stateType + " Min Value" + minValue + " Max Value" + maxValue  + " Current Value: " + currentValue + " Value to add: " + value + " Remaining Value: " + (currentValue + value));

        currentValue += value;
        if (currentValue > maxValue)
        {
            currentValue = maxValue;
        }

        UpdateUI();
    }

    public void Decrease(float value)
    {
        currentValue -= value;
        if (currentValue < minValue)
        {
            currentValue = minValue;
        }

        UpdateUI();
    }

    public void Affect(float value)
    {
        currentValue += value;
        if (currentValue > maxValue)
        {
            currentValue = maxValue;
        }
        else if (currentValue < minValue)
        {
            currentValue = minValue;
        }
        UpdateUI();
    }

    public override string ToString()
    {
        return $"{stateType}: {currentValue}";
    }
}
