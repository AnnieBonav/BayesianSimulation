using System.Collections;
using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private string agentName;
    public string AgentName => agentName;

    // [SerializeField] private DataTrainer dataTrainer;
    [SerializeField] private InferenceEngineChooser inferenceEngineChooser;
    private InferenceEngine inferenceEngine;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private GameCamera cam;
    private Transform agentTransform;
    [SerializeField] private float yPos = 0.6f;
    [SerializeField] private Day day;

    // Cannot acces the states that belong to the Agent, only the Types (same with the activities, only the types are accessible, not the instances)
    [SerializeField] private List<State> states;
    private List<STATE_TYPE> statesType;
    public List<STATE_TYPE> States => statesType;
    // Dictionaries are used for quick lookup
    private Dictionary<STATE_TYPE, State> statesDict;

    [SerializeField] private List<Activity> activities;
    private List<ACTIVITY_TYPE> activitiesType;
    public List<ACTIVITY_TYPE> Activities => activitiesType;
    private Dictionary<ACTIVITY_TYPE, Activity> activitiesDict;

    [SerializeField] private ActivityButtons debugActivityButtons;
    [SerializeField] private bool hasDebugButtons = false;
    public List<Dictionary<string, object>> actionsHistory;
    [ReadOnly] private bool doingActivity = false;
    private Vector3 agentPosition;
    private float maxDistanceFromEnemy;

    private List<InferenceData> performedActivitiesData;
    public List<InferenceData> PerformedActivitiesData => performedActivitiesData;

    private bool isTraining = false;
    public void Awake()
    {
        agentTransform = this.transform;
        maxDistanceFromEnemy = cam.MaxDistance;
        agentPosition = new Vector3(0, yPos, 0);
        actionsHistory = new List<Dictionary<string, object>>();
        performedActivitiesData = new List<InferenceData>();
        inferenceEngine = inferenceEngineChooser.GetSelectedEngine();
        
        CacheStates();
        CacheActivities();
    }

    private void CacheStates()
    {
        statesType = new List<STATE_TYPE>();
        statesDict = new Dictionary<STATE_TYPE, State>();

        foreach (State state in states)
        {
            statesType.Add(state.StateType);
            statesDict.Add(state.StateType, state);
        }
    }

    // Gets the ACTIVITY_TYPEs from all the activities that were dragged into the agent, so it is easier to go through them when doing the Naive Bayes
    private void CacheActivities()
    {
        activitiesType = new List<ACTIVITY_TYPE>();
        activitiesDict = new Dictionary<ACTIVITY_TYPE, Activity>();

        foreach (Activity activity in activities)
        {
            activitiesType.Add(activity.ActivityType);
            activitiesDict.Add(activity.ActivityType, activity);
        }
    }

    private float DistanceFromEnemy()
    {
        return Vector3.Distance(agentTransform.position, enemyTransform.position);
    }

    private float NormalizeValue(float value, float minValue, float maxValue)
    {
        return (value - minValue) / (maxValue - minValue);
    }

    private void MoveTo(Transform actionObjectToMoveTo)
    {
        agentPosition.x = actionObjectToMoveTo.position.x;
        agentPosition.z = actionObjectToMoveTo.position.z;
        
        iTween.MoveTo(this.gameObject, iTween.Hash("position", agentPosition));
    }

    public void StartTraining(bool verbose = false)
    {
        isTraining = true;
        StartCoroutine(TrainingLoop(verbose));
    }

    // Should probably chnage the names because there will be one inference engine that actually does active inference, so it will be traing and infering and naming can be confusing
    public void StartInfering()
    {
        StartCoroutine(ActivityLoop());
    }

    // TODO: make this disabling better, should probably not need to run if is training? Or maybe yes but maybe there is more than one option of running?
    private void OnDisable() {
        if(isTraining)
        {
            StopCoroutine(TrainingLoop());
        }
        else{
            StopCoroutine(ActivityLoop());
        }
    }

    IEnumerator ActivityLoop()
    {
        while (true)
        {
            if (!doingActivity)
            {
                // Will choose the activity based on the Naive Bayes, NOT CLEAN
                Activity chosenActivity = ChooseActivityWithDataTrainer();
                // Activity chosenActivity = ChooseActivity();

                Action action = ChooseRandomActionFromActivity(chosenActivity);
                PerformAction(action, false);
            }
            yield return new WaitForSeconds(1);
        }
    }

    private Action ChooseRandomActionFromActivity(Activity activity)
    {
        int randomIndex = Random.Range(0, activity.PossibleActions.Count);
        return activity.PossibleActions[randomIndex];
    }

    
    private Activity ChooseActivityWithDataTrainer()
    {
        InferenceData currentData = new InferenceData();
        currentData.InitializeInferenceData(states);

        ACTIVITY_TYPE chosenActivityType = inferenceEngine.InferActivity(currentData);
        Activity chosenActivity = activities.Find(activity => activity.ActivityType == chosenActivityType);

        return chosenActivity;
    }

    IEnumerator TrainingLoop(bool verbose = false)
    {
        while (true)
        {
            if (!doingActivity)
            {
                InferenceData randomTrainingData = new InferenceData();
                randomTrainingData.InitializeRandomInferenceData(statesType);

                // Choose activity based on basic heuristics
                randomTrainingData.ChosenActivity = inferenceEngine.ChooseTrainingActivity(randomTrainingData); // Use the logic on the inferenceEngine

                if(verbose) print(JsonSerialization.ToJson(randomTrainingData));

                performedActivitiesData.Add(randomTrainingData); // Record the chosen activity
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    // Multiple needs could be affected from Action...Probably change names of PerformAction and PerformActivity
    public void PerformAction(Action action, bool verbose = false)
    {
        MoveTo(action.ActionTransform);
        List<State> affectedStates = AffectedStatesByAction(action);
        doingActivity = true;

        // I start it only one time because the time does not pass for each affected state
        StartCoroutine(AffectState(action, verbose));

        foreach (State state in affectedStates)
        {
            // Will affect the state (that I know corresponds to the action AND exists on the Agent as a state that affects) by the value of the action (by finding it in the dictionary, the affecting float is the value)
            state.Affect(action.AffectedStates[state.StateType]);
            if (verbose) print("DEBUG IN PERFORM ACTION" + "  Will be action " + action);
        }
        doingActivity = false;
    }

    private List<State> AffectedStatesByAction(Action action)
    {
        List<State> affectedStates = new List<State>();
        foreach (STATE_TYPE stateType in action.AffectedStates.Keys)
        {
            if (statesDict.ContainsKey(stateType))
            {
                affectedStates.Add(statesDict[stateType]);
            }
        }
        return affectedStates;
    }

    IEnumerator AffectState(Action action, bool verbose = false)
    {
        if(hasDebugButtons) debugActivityButtons.SetButtonsInteractable(false);
        float waitingTime = action.ActionInfo.TimeInMin * day.RTSecInSimMin;
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
                    float currentDistanceFromEnemy = DistanceFromEnemy();
                    float crimeRate = Mathf.Pow(1 -NormalizeValue(currentDistanceFromEnemy, 0, maxDistanceFromEnemy), 2) * 100;
                    state.UpdateValue(crimeRate);
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
        if (verbose) print("Chosen Activity: " + JsonSerialization.ToJson(chosenActivity));
        return chosenActivity;
    } 
}