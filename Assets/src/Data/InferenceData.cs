using System.Collections.Generic;
using System;
using UnityEngine;

// TODO: Should remove the scriptable object and just have a list of InferenceData?
// Define a wrapper class for your list of InferenceData
public class TrainingDataWrapper
{
    // TODO: Check, cause maybe the ACTIVITY_TYPE should each have its affected by states? (- cause maybe ACTIVITY_TYPE and STATE_TYPE should be in the InferenceData-)
    public List<ACTIVITY_TYPE> ActivityTypes;
    public List<STATE_TYPE> StateTypes;
    public List<InferenceData> InferenceData;

    public TrainingDataWrapper()
    {
    }

    public TrainingDataWrapper(List<ACTIVITY_TYPE> activityTypes, List<STATE_TYPE> stateTypes, List<InferenceData> trainingData)
    {
        ActivityTypes = activityTypes;
        StateTypes = stateTypes;
        InferenceData = trainingData;
    }
}

[Serializable]
public class StateData
{
    public STATE_TYPE StateType;
    public float Value;
    public StateData()
    {
    }
    public StateData(STATE_TYPE stateType, float value)
    {
        StateType = stateType;
        Value = value;
    }
}

// Will be the saved data that the classifier will use to train the model
[Serializable]
public class InferenceData
{
    public List<StateData> StatesValues;
    public ACTIVITY_TYPE ChosenActivity;
    public InferenceData()
    {
        StatesValues = new List<StateData>();
        ChosenActivity = ACTIVITY_TYPE.NONE;
    }
    public void InitializeRandomInferenceData(List<STATE_TYPE> states)
    {
        foreach (STATE_TYPE state in states)
        {
            StatesValues.Add(new StateData(state, GenerateRandomData()));
        }
    }

    public void InitializeInferenceData(List<State> states)
    {
        StatesValues = new List<StateData>();
        foreach (State state in states)
        {
            StatesValues.Add(new StateData(state.StateType, state.CurrentValue));
        }
    }

    public void AddStateData(State state)
    {
        StatesValues.Add(new StateData(state.StateType, state.CurrentValue));
    }

    // TODO: Would need to get min val and max val per each state
    private int GenerateRandomData()
    {
        return UnityEngine.Random.Range(0, 100);
    }

    public float GetStateValue(STATE_TYPE stateType)
    {
        return StatesValues.Find(foundState => foundState.StateType == stateType).Value;
    }

    public void ChangeValue(STATE_TYPE stateType, float value)
    {
        StateData stateData = StatesValues.Find(foundState => foundState.StateType == stateType);
        stateData.Value = value;
    }
}