using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEngine;

public class CombinedTrainer : DataTrainer
{
    [SerializeField] private float explorationRate = 0.2f;

    public CombinedTrainer()
    {
        dataTrainerType = DATA_TRAINER_TYPE.COMBINED;
    }

    public override ACTIVITY_TYPE ChooseTrainingActivity(List<ACTIVITY_TYPE> availableActivities, InferenceData trainingStateValues)
    {
        if (Random.value < explorationRate)
        {
            ACTIVITY_TYPE activityType = availableActivities[Random.Range(0, availableActivities.Count)];
            Debug.Log("IN RANDOM TRAINER " + JsonUtility.ToJson(trainingStateValues) + $"  Chosen Activity: {activityType}");
            return activityType;
        }
        else
        {
            ACTIVITY_TYPE activityType = ACTIVITY_TYPE.NONE;
            Debug.Log("IN HEURISTICS TRAINER " + JsonSerialization.ToJson(trainingStateValues));
            
            // TODO: Copy pastes from HeuristicsTrainer
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

    }
}
