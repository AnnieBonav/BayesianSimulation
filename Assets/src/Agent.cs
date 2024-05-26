using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

// FOR MORNING ANNIE:
// The agent can choose what to do, but it is like it gets stuck in an activity and the values are not increasing? (Probably not increassing with passing time?) Could be that the agent is super quickly making the decision and then getting stuck in the activity?
//Should start by checking the values of the states and the actions that are being performed. Could also tweak the actions so they take more time (also, probably the actions take too little based on how much they decrease the bad values?)
// Also probably checking why they are getting stuck one one could be the way to go :)
public class Agent : MonoBehaviour
{
    [SerializeField] private string agentName;
    public string AgentName => agentName;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private GameCamera cam;
    [SerializeField] private Transform agentTransform;
    [SerializeField] private float yPos = 0.6f;
    [SerializeField] private Day day;

    [SerializeField] private List<State> states;
    public List<State> States => states;
    [SerializeField] private List<Activity> activities;
    public List<Activity> Activities => activities;

    [SerializeField] private ActivityButtons debugActivityButtons;
    [SerializeField] private bool hasDebugButtons = false;
    public List<Dictionary<string, object>> actionsHistory;
    [SerializeField] private List<Action> actions;
    private List<KeyValuePair<Action, Transform>> actionsTransforms;
    [ReadOnly] private bool doingActivity = false;
    private Vector3 placeholderPosition;
    private float maxDistanceFromEnemy;
    private float currentDistanceFromEnemy;
    private float crimeRatePlaceholder;
    public void Awake()
    {
        placeholderPosition = new Vector3(0, yPos, 0);
        actionsHistory = new List<Dictionary<string, object>>();
        maxDistanceFromEnemy = cam.MaxDistance;
    }

    private float DistanceFromEnemy()
    {
        return Vector3.Distance(agentTransform.position, enemyTransform.position);
    }

    // TODO: Change the states to be a dictionary

    private float NormalizeValue(float value, float minValue, float maxValue)
    {
        return (value - minValue) / (maxValue - minValue);
    }

    private void MoveTo(Transform actionObjectToMoveTo = null)
    {
        if(actionObjectToMoveTo == null){
            actionObjectToMoveTo = actions[0].gameObject.GetComponent<Transform>();
        }
        placeholderPosition.x = actionObjectToMoveTo.position.x;
        placeholderPosition.z = actionObjectToMoveTo.position.z;
        
        iTween.MoveTo(this.gameObject, iTween.Hash("position", placeholderPosition));
    }

    private void Start()
    {
        MoveTo();
        StartCoroutine(ActivityLoop());
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
                PerformAction(action, false);
            }
            yield return new WaitForSeconds(1);
        }
    }

    // Multiple needs could be affected from Action...Probably change names of PerformAction and PerformActivity
    public void PerformAction(Action action, bool verbose = false)
    {
        MoveTo(action.ActionTransform);
        State stateToAffect = states.Find(state => state.StateType == action.AffectedState);
        if(stateToAffect)
        {
            doingActivity = true;
            StartCoroutine(AffectState(action, verbose));
            stateToAffect.Decrease(action.Value);
            doingActivity = false;
            if (verbose) print("DEBUG IN PERFORM ACTION" + "  Will be action " + action);
        }
    }

    IEnumerator AffectState(Action action, bool verbose = false)
    {
        if(hasDebugButtons) debugActivityButtons.SetButtonsInteractable(false);
        float waitingTime = action.TimeInMin * day.RTSecInSimMin;
        yield return new WaitForSeconds(waitingTime);
        
        if(hasDebugButtons) debugActivityButtons.SetButtonsInteractable(true);
    }

    public void PassTime(string activity = null, string action = null)
    {
        // TODO: Make this come from the states that are part of the agent
        float bathroomNeedDelta = 1f / 3f;
        float sleepNeedDelta = 1f / 9.6f;
        float foodNeedDelta = 1f / 2.4f;
        
        foreach(State state in states)
        {
            switch (state.StateType)
            {
                case STATE_TYPE.BathroomNeed:
                    state.Increase(bathroomNeedDelta);
                    break;
                case STATE_TYPE.SleepNeed:
                    state.Increase(sleepNeedDelta);
                    break;
                case STATE_TYPE.FoodNeed:
                    state.Increase(foodNeedDelta);
                    break;
                case STATE_TYPE.CrimeRate:
                    currentDistanceFromEnemy = DistanceFromEnemy();
                    crimeRatePlaceholder = Mathf.Pow(1 -NormalizeValue(currentDistanceFromEnemy, 0, maxDistanceFromEnemy), 2) * 100;
                    state.UpdateValue(crimeRatePlaceholder);
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
            {"hunger_increase", foodNeedDelta},
            {"tiredness_increase", sleepNeedDelta},
            {"bladder_increase", bathroomNeedDelta},
            // {"detectiveness_increase", detectivenessIncrease},
            // {"relaxation_increase", relaxationDelta},
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

    // TODO: Check if this is needed/better than the default one
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }

    public Activity ChooseActivity(bool verbose = false)
    {
        float highestLogSum = Mathf.NegativeInfinity;
        Activity chosenActivity = null;

        foreach (Activity activity in activities)
        {
            float logSum = activity.GetLogsSum(states, verbose);
            if (verbose) Debug.Log($"Activity: {activity.ActivityType}, Log Sum: {logSum}");
            if (logSum > highestLogSum)
            {
                highestLogSum = logSum;
                chosenActivity = activity;
            }
        }
        if (verbose) print("Chosen Activity: " + chosenActivity);
        return chosenActivity;
    } 
}