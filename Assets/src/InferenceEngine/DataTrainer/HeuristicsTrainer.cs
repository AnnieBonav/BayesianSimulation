using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEngine;

// This heuristics trainer simulates a human-like decision making process for the training, based on the state values (and what the human would do in that situation). It is used to avoid needing to manually train the model.
public class HeuristicsTrainer : DataTrainer
{
    // TODO: Add that it actally checks the activity exists on the availableActivities
    // TODO: Add that it checks if the state exists in the trainingStateValues
    public override ACTIVITY_TYPE ChooseTrainingActivity(List<ACTIVITY_TYPE> availableActivities, InferenceData trainingStateValues)
    {
        ACTIVITY_TYPE activityType = ACTIVITY_TYPE.NONE;
        Debug.Log("IN HEURISTICS TRAINER " + JsonSerialization.ToJson(trainingStateValues));
        
        // TODO: Need to change this to be dynamic, do not know how at the moment :)
        // SEE I had an issea because this was callinhg relax and that didnt exist anymore **angry** need to change
        if (trainingStateValues.GetStateValue(STATE_TYPE.CRIME_RATE) > 30f && trainingStateValues.GetStateValue(STATE_TYPE.CRIME_RATE) < 40f)
        {
            activityType = ACTIVITY_TYPE.DETECTIVE;
        }
        else if (trainingStateValues.GetStateValue(STATE_TYPE.BATHROOM_NEED) > 70f && trainingStateValues.GetStateValue(STATE_TYPE.BATHROOM_NEED) < 80f)
        {
            activityType = ACTIVITY_TYPE.BATHROOM;
        }
        else if (trainingStateValues.GetStateValue(STATE_TYPE.SLEEP_NEED) > 90f && trainingStateValues.GetStateValue(STATE_TYPE.SLEEP_NEED) < 95f)
        {
            activityType = ACTIVITY_TYPE.SLEEP;
        }
        else if (trainingStateValues.GetStateValue(STATE_TYPE.FOOD_NEED) > 45f && trainingStateValues.GetStateValue(STATE_TYPE.FOOD_NEED) < 55f)
        {
            activityType = ACTIVITY_TYPE.FOOD;
        }

        if (activityType == ACTIVITY_TYPE.NONE)
        {
            activityType = ACTIVITY_TYPE.RELAX;
        }

        Debug.Log($"IN HEURISTICS TRAINER " + $"Chosen Activity: {activityType}");

        return activityType;
    }

    public HeuristicsTrainer()
    {
        dataTrainerType = DATA_TRAINER_TYPE.HEURISTICS;
    }
}
