using System.Collections;
using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEngine;

public class AgentWithEnemy : MonoBehaviour
{
    [SerializeField] private string agentName;
    public string AgentName => agentName;
    [SerializeField] private InferenceEngineChooser inferenceEngineChooser;
    private InferenceEngine inferenceEngine;
    [SerializeField] private Villain villain;
    private Transform villainTransform;
    [SerializeField] private GameCamera cam;
    private Transform agentTransform;
    [SerializeField] private float yPos = 0.6f;
    [SerializeField] private Day day;

    // Cannot acces the states that belong to the Agent, only the Types (same with the activities, only the types are accessible, not the instances)
    [SerializeField] private GameObject statesParent;
    private List<State> states;
    public List<STATE_TYPE> States => statesType;
    private List<STATE_TYPE> statesType;
    // Dictionaries are used for quick lookup
    private Dictionary<STATE_TYPE, State> statesDict;

    [SerializeField] private GameObject activitiesParent;
    private List<Activity> activities;
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
        villainTransform = villain.transform;

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
        states = new List<State>();
        statesType = new List<STATE_TYPE>();
        statesDict = new Dictionary<STATE_TYPE, State>();

        foreach(Transform child in statesParent.transform)
        {
            // Will not use "TryGetComponent" cause I prefer the errror
            // TODO: Check if it returns null or an error
            State state = child.GetComponent<State>();
            states.Add(state);
            statesType.Add(state.StateType);
            statesDict.Add(state.StateType, state);
        }
    }

    // Gets the ACTIVITY_TYPEs from all the activities that were dragged into the agent, so it is easier to go through them when doing the Naive Bayes
    private void CacheActivities()
    {
        // TODO: Learn if having an inactiev object still gets added to the activities
        activities = new List<Activity>();
        activitiesType = new List<ACTIVITY_TYPE>();
        activitiesDict = new Dictionary<ACTIVITY_TYPE, Activity>();

        foreach (Transform child in activitiesParent.transform)
        {
            Activity activity = child.GetComponent<Activity>();
            if (activity != null)
            {
            activities.Add(activity);
            activitiesType.Add(activity.ActivityType);
            activitiesDict.Add(activity.ActivityType, activity);
            }
        }
    }

    // Verbose is sent from the Inference Engine (where this is called)
    public void StartTraining(bool verbose)
    {
        isTraining = true;
        StartCoroutine(TrainingLoop(verbose));
    }

    // Verbose is sent from the Inference Engine (where this is called)
    public void StartInfering(bool verbose)
    {
        StartCoroutine(InferenceLoop(verbose));
    }

    // Verbose is sent from the Inference Engine (where this is called)
    public void StartActiveInfering(bool verbose)
    {
        StartCoroutine(ActiveInferenceLoop(verbose));
    }

    IEnumerator TrainingLoop(bool verbose)
    {
        while (true)
        {
            if (!doingActivity)
            {
                InferenceData trainingData = GetTrainingDataWithIE();
                performedActivitiesData.Add(trainingData); // Record the randomly chosen activity
                if(verbose) print(JsonSerialization.ToJson(performedActivitiesData));
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator InferenceLoop(bool verbose)
    {
        print("STarted inference loop");
        while (true)
        {
            if (!doingActivity)
            {
                print("Will choose actovity");
                Activity chosenActivity = GetActivityWithIE();
                Action action = null;
                if (chosenActivity.ActivityType != ACTIVITY_TYPE.DETECTIVE)
                {
                    print("Chosen activity: " + chosenActivity.ActivityType);
                    action = ChooseRandomActionFromActivity(chosenActivity);
                    PerformAction(action, verbose);
                }else
                {
                    print("Solving THE INFERENCE crime");
                    StartCoroutine(SolveCrime());
                }
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
                InferenceData currentDataForTraining = new InferenceData();
                currentDataForTraining.InitializeRandomInferenceData(statesType);

                // Added sending activities
                currentDataForTraining.ChosenActivity = inferenceEngine.DataTrainer.ChooseTrainingActivity(activitiesType, currentDataForTraining);

                if(verbose) print(JsonSerialization.ToJson(currentDataForTraining));

                Activity chosenActivity = activitiesDict[currentDataForTraining.ChosenActivity];
                Action action = ChooseRandomActionFromActivity(chosenActivity);
                
                PerformAction(action, false);
                yield return new WaitForSeconds(action.ActionInfo.TimeInMin * day.RTSecInSimMin);

                performedActivitiesData.Add(currentDataForTraining); // Record the chosen activity

                // Dynamically update the model
                // Check removed reference
                // inferenceEngine.UpdateModel(performedActivitiesData);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private Action ChooseRandomActionFromActivity(Activity activity)
    {
        int randomIndex = Random.Range(0, activity.PossibleActions.Count);
        return activity.PossibleActions[randomIndex];
    }

    private InferenceData GetTrainingDataWithIE(bool verbose = false)
    {
        InferenceData randomTrainingData = new InferenceData();
        randomTrainingData.InitializeRandomInferenceData(statesType);

        // TODO: Added sending activities
        randomTrainingData.ChosenActivity = inferenceEngine.DataTrainer.ChooseTrainingActivity(activitiesType, randomTrainingData);

        if(verbose) print(JsonSerialization.ToJson(randomTrainingData));
        return randomTrainingData;
    }
    
    private Activity GetActivityWithIE()
    {
        InferenceData currentData = new InferenceData();
        currentData.InitializeInferenceData(states);

        ACTIVITY_TYPE chosenActivityType = inferenceEngine.InferActivity(currentData);
        Activity chosenActivity = activitiesDict[chosenActivityType];

        return chosenActivity;
    }

    // Multiple needs could be affected from Action...Probably change names of PerformAction and PerformActivity
    public void PerformAction(Action action, bool verbose)
    {
        MoveTo(action.ActionTransform);
        List<STATE_TYPE> affectedStates = action.GetAffectedStates(statesType);
        doingActivity = true;

        // I start it only one time because the time does not pass for each affected state
        StartCoroutine(AffectState(action));

        foreach (STATE_TYPE stateType in affectedStates)
        {
            // Will affect the state (that I know corresponds to the action AND exists on the Agent as a state that affects) by the value of the action (by finding it in the dictionary, the affecting float is the value)
            statesDict[stateType].Affect(action.AffectedStates[statesDict[stateType].StateType]);
            if (verbose) print("DEBUG IN PERFORM ACTION" + "  Will be action " + action);
        }
        doingActivity = false;
    }

    // This architecture could be better, but works for now for solving crimes
    // TODO: probably need to check the disabling enebling and state functions
    public IEnumerator SolveCrime()
    {
        // Will probably have debug buttons because this is for the manual training (for now)
        if(hasDebugButtons) debugActivityButtons.SetButtonsInteractable(false);
        print("Solving crime");
        MoveTo(villainTransform);
        villain.ScareAway(10, 15);
        SaveCurrentStatesData(activitiesDict[ACTIVITY_TYPE.DETECTIVE]);
        yield return new WaitForSeconds(3);
        if(hasDebugButtons) debugActivityButtons.SetButtonsInteractable(true);
    }

    IEnumerator AffectState(Action action)
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
        print("Moving to: " + actionObjectToMoveTo.position);
        iTween.MoveTo(this.gameObject, iTween.Hash("position", agentPosition));
    }

    public void PassTime()
    {
        foreach(State state in states)
        {
            if(state.StateType == STATE_TYPE.CRIME_RATE){
                float currentDistanceFromEnemy = DistanceFromEnemy();
                float crimeRate = Mathf.Pow(1 -NormalizeValue(currentDistanceFromEnemy, 0, maxDistanceFromEnemy), 2) * 100;
                state.UpdateValue(crimeRate);
                continue;
            }
            state.AffectByRate();
        }
    }

    private float DistanceFromEnemy()
    {
        return Vector3.Distance(agentTransform.position, villainTransform.position);
    }

    private float NormalizeValue(float value, float minValue, float maxValue)
    {
        return (value - minValue) / (maxValue - minValue);
    }

    // TODO: make this disabling better, should probably not need to run if is training? Or maybe yes but maybe there is more than one option of running?
    private void OnDisable() {
        if(isTraining)
        {
            StopCoroutine(TrainingLoop(false));
        }
        else
        {
            StopCoroutine(InferenceLoop(false));
        }
    }

    // Externally send the activity that will be done, create the performed action information and add it so it can be saved (and then used for inference)
    public IEnumerator ManuallyPerformActionForTraining(ACTIVITY_TYPE activityType, bool verbose = false)
    {
        print("ManuallyPerformActionForTraining");
        Activity manuallyChosenActivity = activitiesDict[activityType];
        Action action = ChooseRandomActionFromActivity(manuallyChosenActivity);

        SaveCurrentStatesData(manuallyChosenActivity, verbose);
        PerformAction(action, true);
        yield return new WaitForSeconds(action.ActionInfo.TimeInMin * day.RTSecInSimMin);
    }

    private void SaveCurrentStatesData(Activity chosenActivity, bool verbose = false)
    {
        InferenceData currentStatesForManualTraining = new InferenceData();
        currentStatesForManualTraining.InitializeRandomInferenceData(statesType);
        currentStatesForManualTraining.ChosenActivity = chosenActivity.ActivityType;
        if(verbose) print(JsonSerialization.ToJson(currentStatesForManualTraining));
        
        performedActivitiesData.Add(currentStatesForManualTraining); // Record the chosen activity
    }

    /*
    public IEnumerator ManuallyPerformActionForTraining(ACTIVITY_TYPE activityType, bool verbose = true)
    {
        print("ManuallyPerformActionForTraining");
        Activity manuallyChosenActivity = activities.Find(activity => activity.ActivityType == activityType);
        Action action = ChooseRandomActionFromActivity(manuallyChosenActivity);
        List<STATE_TYPE> affectedStates = action.GetAffectedStates(statesType);

        // NEED TO CONSIDER WITH WHICH INFO I TRAIN WHAT
        InferenceData currentStatesForManualTraining = new InferenceData();
        foreach (STATE_TYPE stateType in affectedStates)
        {
            currentStatesForManualTraining.AddStateData(statesDict[stateType]);
        }

        currentStatesForManualTraining.ChosenActivity = manuallyChosenActivity.ActivityType;

        if(verbose) print(JsonSerialization.ToJson(currentStatesForManualTraining));
        
        performedActivitiesData.Add(currentStatesForManualTraining); // Record the chosen activity

        PerformAction(action, true);
        yield return new WaitForSeconds(action.ActionInfo.TimeInMin * day.RTSecInSimMin);
    }*/
}