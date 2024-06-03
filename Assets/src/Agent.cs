using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// FOR MORNING ANNIE:
// The agent can choose what to do, but it is like it gets stuck in an activity and the values are not increasing? (Probably not increassing with passing time?) Could be that the agent is super quickly making the decision and then getting stuck in the activity?
//Should start by checking the values of the states and the actions that are being performed. Could also tweak the actions so they take more time (also, probably the actions take too little based on how much they decrease the bad values?)
// Also probably checking why they are getting stuck one one could be the way to go :)
public class Agent : MonoBehaviour
{
    [SerializeField] private string agentName;
    public string AgentName => agentName;
    [SerializeField] private bool isTraining;
    public bool IsTraining => isTraining;
    [SerializeField] private DataTrainer dataTrainer;
    [SerializeField] private bool saveTrainingData;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private GameCamera cam;
    [SerializeField] private Transform agentTransform;
    [SerializeField] private float yPos = 0.6f;
    [SerializeField] private Day day;

    [SerializeField] private List<State> states;
    public List<State> States => states;
    private Dictionary<STATE_TYPE, State> statesDict;
    [SerializeField] private List<Activity> activities;
    public List<Activity> Activities => activities;
    [SerializeField, Tooltip("Will be the one used to test the naive agent, might not be the initial definition of the Activity")] private Activity naiveActivity;

    [SerializeField] private ActivityButtons debugActivityButtons;
    [SerializeField] private bool hasDebugButtons = false;
    public List<Dictionary<string, object>> actionsHistory;
    [ReadOnly] private bool doingActivity = false;
    private Vector3 placeholderPosition;
    private float maxDistanceFromEnemy;
    private float currentDistanceFromEnemy;
    private float crimeRatePlaceholder;
    private List<TrainingData> trainedData;
    private List<ACTIVITY_TYPE> activityTypes;
    public void Awake()
    {
        maxDistanceFromEnemy = cam.MaxDistance;
        placeholderPosition = new Vector3(0, yPos, 0);
        actionsHistory = new List<Dictionary<string, object>>();
        trainedData = new List<TrainingData>();
        CacheStates();
        CacheActivities();
    }

    // Gets the ACTIVITY_TYPEs from all the activities that were dragged into the agent, so it is easier to go through them when doing the Naive Bayes
    private void CacheActivities()
    {
        activityTypes = new List<ACTIVITY_TYPE>();
        foreach (Activity activity in activities)
        {
            activityTypes.Add(activity.ActivityType);
        }
    }

    private void CacheStates()
    {
        statesDict = new Dictionary<STATE_TYPE, State>();
        foreach (State state in states)
        {
            statesDict.Add(state.StateType, state);
        }
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

    private void MoveTo(Transform actionObjectToMoveTo)
    {
        placeholderPosition.x = actionObjectToMoveTo.position.x;
        placeholderPosition.z = actionObjectToMoveTo.position.z;
        
        iTween.MoveTo(this.gameObject, iTween.Hash("position", placeholderPosition));
    }

    private void Start()
    {
        if(!isTraining)
        {
            StartCoroutine(ActivityLoop());
        }
        else
        {
            StartCoroutine(TrainingLoop());
        }
    }

    // TODO: Save scriptable object in the future, rn will be a JSON that works because gets serialized from the trainingDataScriptableObject so the list is respected, can just open it later
    private void SaveTrainingData(TrainingDataWrapper trainingDataWrapper)
    {
        string trainingDataJSON = JsonUtility.ToJson(trainingDataWrapper);
        print("Final Training Data JSON" + trainingDataJSON);

        int fileCount = Directory.GetFiles("Assets/src/Data/TrainingData").Length;
        string filePath = $"Assets/src/Data/TrainingData/TrainedData{fileCount}.json";

        File.WriteAllText(filePath, trainingDataJSON);
    }

    private void OnDestroy()
    {
        if(!isTraining)
        {
            StopCoroutine(ActivityLoop());
        }
        else
        {
            StopCoroutine(TrainingLoop());
            if(saveTrainingData)
            {
                TrainingDataWrapper trainingDataWrapper = new TrainingDataWrapper(trainedData);
                SaveTrainingData(trainingDataWrapper);
            }else
            {
                print("Training Data not saved");
            }
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
        TrainingData currentData = new TrainingData();
        currentData.BathroomNeed = statesDict[STATE_TYPE.BathroomNeed].CurrentValue;
        currentData.SleepNeed = statesDict[STATE_TYPE.SleepNeed].CurrentValue;
        currentData.FoodNeed = statesDict[STATE_TYPE.FoodNeed].CurrentValue;
        currentData.CrimeRate = statesDict[STATE_TYPE.CrimeRate].CurrentValue;

        ACTIVITY_TYPE chosenActivityType = dataTrainer.ChooseActivity(currentData);
        Activity chosenActivity = activities.Find(activity => activity.ActivityType == chosenActivityType);

        return chosenActivity;
    }

    IEnumerator TrainingLoop()
    {
        print("Training Loop");
        while (true)
        {
            if (!doingActivity)
            {
                // TODO: Change trainingData to stateValues (once cretaing it per state is programmed)
                // Generate random state values between 0 and 100
                TrainingData trainingData = new TrainingData();

                // Choose activity based on basic heuristics
                trainingData.ChosenActivity = ChooseActivitySimpleHeuristics(trainingData); // Initially use basic logic to choose activity

                // print(trainingData.ToJson());
                trainedData.Add(trainingData); // Record the chosen activity
            }
            yield return new WaitForSeconds(0.01f);
        }
    }


    float GaussianProbability(float x, float mean, float variance)
    {
        return (1 / Mathf.Sqrt(2 * Mathf.PI * variance)) * Mathf.Exp(-((x - mean) * (x - mean)) / (2 * variance));
    }


    private ACTIVITY_TYPE ChooseActivitySimpleHeuristics(TrainingData trainingValues)
    {
        // Initial logic for choosing activity, this will be updated later with Naive Bayes inference. Will start with basic "importance" (bathroom > sleep > food > relax)
        // TODO: Will be changed for integers?
        if (trainingValues.BathroomNeed > 70f) return ACTIVITY_TYPE.Bathroom;
        if (trainingValues.SleepNeed > 80f) return ACTIVITY_TYPE.Sleep;
        if (trainingValues.FoodNeed > 50f) return ACTIVITY_TYPE.Food;
        return ACTIVITY_TYPE.Relax;
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