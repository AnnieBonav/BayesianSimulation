using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

// FOR MORNING ANNIE:
// The agent can choose what to do, but it is like it gets stuck in an activity and the values are not increasing? (Probably not increassing with passing time?) Could be that the agent is super quickly making the decision and then getting stuck in the activity?
//Should start by checking the values of the states and the actions that are being performed. Could also tweak the actions so they take more time (also, probably the actions take too little based on how much they decrease the bad values?)
// Also probably checking why they are getting stuck one one could be the way to go :)
public class Agent : MonoBehaviour
{
    [SerializeField] private string agentName;
    public string AgentName => agentName;

    [SerializeField] private Transform agentTransform;
    [SerializeField] private Day day;

    [SerializeField] private List<State> states;
    public List<State> States => states;
    [SerializeField] private List<Activity> activities;
    public List<Activity> Activities => activities;

    [SerializeField] private ActivityButtons debugActivityButtons;
    public List<Dictionary<string, object>> actionsHistory;
    [SerializeField] private List<Action> actions;
    private List<KeyValuePair<Action, Transform>> actionsTransforms;
    [ReadOnly] private bool doingActivity = false;
    public void Awake()
    {
        actionsHistory = new List<Dictionary<string, object>>();
    }

    private void MoveTo(Transform actionObjectToMoveTo = null){
        if(actionObjectToMoveTo == null){
            actionObjectToMoveTo = actions[0].gameObject.GetComponent<Transform>();
        }

        iTween.MoveTo(this.gameObject, iTween.Hash("position", actionObjectToMoveTo.GetComponent<Transform>()));
    }

    private void Start()
    {
        MoveTo();
        StartCoroutine(ActivityLoop());
        // ChooseActivity(true);
    }

    private void OnDestroy()
    {
        StopCoroutine(ActivityLoop());
    }

    IEnumerator ActivityLoop()
    {
        while (true)
        {
            if (!doingActivity)
            {
                Activity chosenActivity = ChooseActivity();
                Action action = actions.Find(action => action.ActivityType == chosenActivity.ActivityType);
                PerformAction(action);
            }
            yield return new WaitForSeconds(1);
        }
    }

    // Multiple needs could be affected from Action...Probably change names of PerformAction and PerformActivity
    public void PerformAction(Action action)
    {
        MoveTo(action.ActionTransform);
        foreach(State state in states)
        {
            Debug.Log(state);
            if(state.StateType == action.AffectedState)
            {
                StartCoroutine(PerformActivity(action.TimeInMin));
                state.Decrease(action.Value);
                Debug.Log(action);
            }
            else
            {
                Debug.Log("No state affected");
            }
        }
        
    }

    IEnumerator PerformActivity(int activityDuration)
    {
        doingActivity = true;
        debugActivityButtons.SetButtonsInteractable(false);
        float waitingTime = activityDuration * day.RTSecInSimMin;
        yield return new WaitForSeconds(waitingTime);
        doingActivity = false;
        debugActivityButtons.SetButtonsInteractable(true);
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
            switch (state.StateType)
            {
                case STATE_TYPE.BathroomNeed:
                    state.Increase(bladderIncrease);
                    break;
                case STATE_TYPE.SleepNeed:
                    state.Increase(tirednessIncrease);
                    break;
                case STATE_TYPE.FoodNeed:
                    state.Increase(hungerIncrease);
                    break;
                case STATE_TYPE.CrimeRate:
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
        // TODO: Make difference between Action and Activity throughout code
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

    public Activity ChooseActivity(bool verbose = false)
    {
        float highestLogSum = Mathf.NegativeInfinity;
        Activity chosenActivity = null;

        foreach (Activity activity in activities)
        {
            float logSum = activity.GetLogsSum(states, verbose);
            if (verbose)
            {
                Debug.Log($"Activity: {activity.ActivityType}, Log Sum: {logSum}");
            }
            if (logSum > highestLogSum)
            {
                highestLogSum = logSum;
                chosenActivity = activity;
            }
        }
        print("Chosen Activity: " + chosenActivity);
        return chosenActivity;
    } 
}