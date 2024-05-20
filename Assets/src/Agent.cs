using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Agent : MonoBehaviour
{
    [SerializeField]
    private string name;
    public string Name => name;

    [SerializeField]
    private Day day;

    [SerializeField]
    private List<State> states;
    public List<State> States => states;

    public List<Dictionary<string, object>> actionsHistory;
    public bool fakeTime = true;

    public void Start()
    {
         actionsHistory = new List<Dictionary<string, object>>();
    }

    // Affected need should come from the action (multiple needs could be affected)
    public void PerformAction(Action action, Need affectedNeed)
    {
        if (fakeTime)
        {
            // day.FakePassTime(action.timeInMin, "affectedNeed", action.name);
        }
        
        foreach(State state in states)
        {
            Debug.Log(state);
            if(state.Need == affectedNeed)
            {
                state.Decrease(action.value);
                Debug.Log(action);
            }
        }
        
    }

    public void PassTime(int minutes, string activity = null, string action = null)
    {
        float hungerIncrease = Mathf.Round(1f / 2.4f * 10000f) / 10000f;
        float tirednessIncrease = Mathf.Round(1f / 9.6f * 10000f) / 10000f;
        float bladderIncrease = Mathf.Round(1f / 1.8f * 10000f) / 10000f;
        float detectivenessIncrease = Mathf.Round(1f / 6.2f * 10000f) / 10000f;
        float relaxationIncrease = Mathf.Round(1f / 4.8f * 10000f) / 10000f;
        
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
        for (int i = 0; i < minutes; i++)
        {
            // states["hunger"].Increase(hungerIncrease);
            // states["tiredness"].Increase(tirednessIncrease);
            // states["bladder"].Increase(bladderIncrease);
            // states["detectiveness"].Increase(detectivenessIncrease);
            // states["relaxation"].Increase(relaxationIncrease);
        }

        var infoJson = new Dictionary<string, object>
        {
            {"current_time", day.TimeOfDay},
            {"entry_type", "time_increase"},
            {"delta_mins", minutes},
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