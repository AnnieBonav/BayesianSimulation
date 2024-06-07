using UnityEngine;
using TMPro;
using UnityEngine.UI;

// STATE_TYPE is an enum that represents the different states that an agent can have, which are the values that it receives (both internally and externally) and that it can change through its Actions (or simply change over time)
public enum STATE_TYPE
{
    NONE,
    BATHROOM_NEED,
    SLEEP_NEED,
    FOOD_NEED,
    CRIME_RATE,
}
// FUNCTION_TYPE was the representation of the different types of functions that mapped the current STATE value to the probability of choosing it, but now Gaussians will be used.
// FUNCTION_TYPE might become the function that explains the rate of change of the STATE value over time.
public enum FUNCTION_TYPE
{
    EXPONENTIAL,
    LOGARITHMIC,
    SQUARE,
    LINEAR
}
public class State : MonoBehaviour
{
    [SerializeField] private STATE_TYPE stateType;
    public STATE_TYPE StateType => stateType;
    [ReadOnly] private float currentValue;
    public float CurrentValue => currentValue;
    [Header("Values Parameters")]
    [SerializeField] private float affectRate = 0.1f;
    public float IncreaseRate => affectRate;
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

    // TODO: Could add different types of affect functions, and use the FUNCTION_TYPE enum to choose which one to use
    public void AffectByRate()
    {
        if(stateType == STATE_TYPE.CRIME_RATE)
        {
            currentValue += Mathf.Sin(Time.deltaTime) * affectRate;
        }
        else
        {
            currentValue += 1 / affectRate;
        }

        if (currentValue > maxValue)
        {
            currentValue = maxValue;
        }else if (currentValue < minValue)
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
