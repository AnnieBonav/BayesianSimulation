using System.Collections.Generic;
using UnityEngine;
using System;

// TODO: Should remove the scriptable object and just have a list of TrainingData?
// Define a wrapper class for your list of TrainingData
public class TrainingDataWrapper
{
    public List<TrainingData> TrainingData;
    public TrainingDataWrapper(List<TrainingData> trainingData)
    {
        TrainingData = trainingData;
    }
}

// Will be the saved data that the classifier will use to train the model
[Serializable]
public class TrainingData{
    public float BathroomNeed;
    public float SleepNeed;
    public float FoodNeed;
    public float CrimeRate;
    public ACTIVITY_TYPE ChosenActivity;
    public TrainingData()
    {
        BathroomNeed = GenerateRandomData();
        SleepNeed = GenerateRandomData();
        FoodNeed = GenerateRandomData();
        CrimeRate = GenerateRandomData();
        ChosenActivity = ACTIVITY_TYPE.None;
    }

    // TODO: Would need to get min val and max val per each state
    private int GenerateRandomData()
    {
        return UnityEngine.Random.Range(0, 100);
    }

    public void ChangeValue(STATE_TYPE stateType, float value)
    {
        switch (stateType)
        {
            case STATE_TYPE.BathroomNeed:
                BathroomNeed = value;
                break;
            case STATE_TYPE.SleepNeed:
                SleepNeed = value;
                break;
            case STATE_TYPE.FoodNeed:
                FoodNeed = value;
                break;
            case STATE_TYPE.CrimeRate:
                CrimeRate = value;
                break;
            default:
                break;
        }
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}