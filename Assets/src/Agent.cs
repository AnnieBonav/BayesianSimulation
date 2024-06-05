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
    [ReadOnly] private bool doingActivity = false;
    private Vector3 agentPosition;
    private float maxDistanceFromEnemy;

    private List<InferenceData> performedActivitiesData;
    public List<InferenceData> PerformedActivitiesData => performedActivitiesData;
    private bool isTraining = false;

    private void Awake()
    {
        agentTransform = this.transform;
        maxDistanceFromEnemy = cam.MaxDistance;
        agentPosition = new Vector3(0, yPos, 0);
        performedActivitiesData = new List<InferenceData>();
        inferenceEngine = inferenceEngineChooser.GetSelectedEngine();

        CacheStates();
        CacheActivities();
    }

    private void Start()
    {
        inferenceEngine.InitializeEngine();
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

    public void StartTraining(bool verbose = false)
    {
        isTraining = true;
        StartCoroutine(TrainingLoop(verbose));
    }

    // Should probably chnage the names because there will be one inference engine that actually does active inference, so it will be traing and infering and naming can be confusing
    public void StartInfering()
    {
        StartCoroutine(InferenceLoop());
    }

    public void StartActiveInfering()
    {
        StartCoroutine(ActiveInferenceLoop());
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

    IEnumerator InferenceLoop()
    {
        while (true)
        {
            if (!doingActivity)
            {
                Activity chosenActivity = ChooseActivityWithDataTrainer();
                Action action = ChooseRandomActionFromActivity(chosenActivity);
                PerformAction(action, false);
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator ActiveInferenceLoop(bool verbose = false)
    {
        while (true)
        {
            if (!doingActivity)
            {
                InferenceData randomTrainingData = new InferenceData();
                randomTrainingData.InitializeRandomInferenceData(statesType);

                randomTrainingData.ChosenActivity = inferenceEngine.ChooseTrainingActivity(randomTrainingData);

                if(verbose) print(JsonSerialization.ToJson(randomTrainingData));

                Activity chosenActivity = activities.Find(activity => activity.ActivityType == randomTrainingData.ChosenActivity);
                Action action = ChooseRandomActionFromActivity(chosenActivity);
                
                PerformAction(action, false);

                performedActivitiesData.Add(randomTrainingData); // Record the chosen activity

                // Dynamically update the model
                inferenceEngine.UpdateModel(performedActivitiesData);
            }
            yield return new WaitForSeconds(0.1f);
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

    // Multiple needs could be affected from Action...Probably change names of PerformAction and PerformActivity
    public void PerformAction(Action action, bool verbose = false)
    {
        MoveTo(action.ActionTransform);
        List<STATE_TYPE> affectedStates = action.GetAffectedStates(statesType);
        doingActivity = true;

        // I start it only one time because the time does not pass for each affected state
        StartCoroutine(AffectState(action, verbose));

        foreach (STATE_TYPE stateType in affectedStates)
        {
            // Will affect the state (that I know corresponds to the action AND exists on the Agent as a state that affects) by the value of the action (by finding it in the dictionary, the affecting float is the value)
            statesDict[stateType].Affect(action.AffectedStates[statesDict[stateType].StateType]);
            if (verbose) print("DEBUG IN PERFORM ACTION" + "  Will be action " + action);
        }
        doingActivity = false;
    }

    IEnumerator AffectState(Action action, bool verbose = false)
    {
        if(hasDebugButtons) debugActivityButtons.SetButtonsInteractable(false);
        float waitingTime = action.ActionInfo.TimeInMin * day.RTSecInSimMin;
        yield return new WaitForSeconds(waitingTime);
        
        if(hasDebugButtons) debugActivityButtons.SetButtonsInteractable(true);
    }

    private void MoveTo(Transform actionObjectToMoveTo)
    {
        agentPosition.x = actionObjectToMoveTo.position.x;
        agentPosition.z = actionObjectToMoveTo.position.z;
        
        iTween.MoveTo(this.gameObject, iTween.Hash("position", agentPosition));
    }

    public void PassTime()
    {
        foreach(State state in states)
        {
            state.AffectByRate();
            if(state.StateType == STATE_TYPE.CrimeRate){
                float currentDistanceFromEnemy = DistanceFromEnemy();
                float crimeRate = Mathf.Pow(1 -NormalizeValue(currentDistanceFromEnemy, 0, maxDistanceFromEnemy), 2) * 100;
                state.UpdateValue(crimeRate);
            }
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

    // TODO: make this disabling better, should probably not need to run if is training? Or maybe yes but maybe there is more than one option of running?
    private void OnDisable() {
        if(isTraining)
        {
            StopCoroutine(TrainingLoop());
        }
        else{
            StopCoroutine(InferenceLoop());
        }
    }
}