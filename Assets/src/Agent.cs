using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Agent : MonoBehaviour
{
    [SerializeField] private string agentName;
    public string AgentName => agentName;

    [SerializeField] private Day day;

    [SerializeField] private List<State> states;
    public List<State> States => states;

    public List<Dictionary<string, object>> actionsHistory;

    public void Awake()
    {
        actionsHistory = new List<Dictionary<string, object>>();
    }

    // Affected need should come from the action (multiple needs could be affected)
    public void PerformAction(Action action, Need affectedNeed)
    {
        foreach(State state in states)
        {
            Debug.Log(state);
            if(state.Need == affectedNeed)
            {
                state.Decrease(action.Value);
                Debug.Log(action);
            }
        }
        
    }

    public void PassTime(string activity = null, string action = null)
    {
        // TODO: Make this come from the states that are oart of the agent
        float hungerIncrease = 1f / 2.4f;
        float tirednessIncrease = 1f / 9.6f;
        float bladderIncrease = 1f / 1.8f;
        float detectivenessIncrease = 1f / 6.2f;
        float relaxationIncrease = 1f / 4.8f;
        
        // print("The increases: " + hungerIncrease + " " + tirednessIncrease + " " + bladderIncrease + " " + detectivenessIncrease + " " + relaxationIncrease);

        foreach(State state in states)
        {
            switch (state.Need)
            {
                case Need.BladderLevel:
                    state.Increase(bladderIncrease);
                    break;
                case Need.TirednessLevel:
                    state.Increase(tirednessIncrease);
                    break;
                case Need.HungerLevel:
                    state.Increase(hungerIncrease);
                    break;
                case Need.RelaxationNeed:
                    state.Increase(relaxationIncrease);
                    break;
                case Need.DetectiveNeed:
                    state.Increase(detectivenessIncrease);
                    break;
                default:
                    print("OOps! That does not exist!");
                    break;
            }
            
        }

        var infoJson = new Dictionary<string, object>
        {
            {"all_time_passed", day.SimAllMin},
            {"day_number", day.SimDay},
            {"current_day_time", day.SimCurrDayMin},
            {"moment_of_day", day.ModTag.ToString()},
            {"entry_type", "time_increase"},
            {"character_name", name},
            {"hunger_increase", hungerIncrease},
            {"tiredness_increase", tirednessIncrease},
            {"bladder_increase", bladderIncrease},
            {"detectiveness_increase", detectivenessIncrease},
            {"relaxation_increase", relaxationIncrease},
            // {"modified_hunger_value", states["hunger"].CurrentValue},
            // {"modified_tiredness_value", states["tiredness"].CurrentValue},
            // {"modified_bladder_value", states["bladder"].CurrentValue},
            // {"modified_detectiveness_value", states["detectiveness"].CurrentValue},
            // {"modified_relaxation_value", states["relaxation"].CurrentValue
        };

        if (activity != null)
        {
            infoJson["called_by_activity"] = activity;
            infoJson["specific_action"] = action;
        }

        actionsHistory.Add(infoJson);
    }

    public void PrintActionsHistory()
    {
        Debug.Log("\n\nActions History");
        foreach (var action in actionsHistory)
        {
            Debug.Log("\n" + action);
        }
    }

    public override string ToString()
    {
        // string theString = $"{name} T{day.TimeOfDay} - Hunger: {states["hunger"].CurrentValue}, Tiredness: {states["tiredness"].CurrentValue}, Bladder: {states["bladder"].CurrentValue}, Detectiveness: {states["detectiveness"].CurrentValue}, Relaxation: {states["relaxation"].CurrentValue}";
        return "I am a fake string";
    }

    public string ChooseRandomActivity()
    {
        // List<string> keys = new List<string>(states.Keys);
        // chosenActivity = keys[Random.Range(0, keys.Count)]
        return "I am a random chosen activity";
    }

    public Need ChooseActivity(bool verbose = false)
    {
        float highestProbability = 0;
        string chosenActivity = null;


        // foreach (var key in states.Keys)
        // {
        //     float probability = states[key].GetProbability(verbose);
        //     if (probability > highestProbability)
        //     {
        //         highestProbability = probability;
        //         chosenActivity = key;
        //     }
        // }
        print("I WILL BE A FAKE NEED, CHECK AGENT CODE");
        return states[0].Need;
    }
}