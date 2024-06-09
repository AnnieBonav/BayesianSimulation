using System.Collections;
using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private string agentName;
    public string AgentName => agentName;
    [SerializeField] private InferenceEngineChooser inferenceEngineChooser;
    private InferenceEngine inferenceEngine;
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

    private List<InferenceData> performedActivitiesData;
    public List<InferenceData> PerformedActivitiesData => performedActivitiesData;
    private bool isTraining = false;

    private void Awake()
    {
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
                // InferenceData trainingData = GetTrainingDataWithIE(verbose);
                GetTrainingDataWithIE(verbose);
                // performedActivitiesData.Add(trainingData); // Record the randomly chosen activity
                // if(verbose) print(JsonSerialization.ToJson(performedActivitiesData));
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator InferenceLoop(bool verbose)
    {
        while (true)
        {
            if (!doingActivity)
            {
                Activity chosenActivity = GetActivityWithIE();
                print("Looping" + chosenActivity.ActivityType);
                Action action = ChooseRandomActionFromActivity(chosenActivity);
                PerformAction(action, verbose);   
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

                currentDataForTraining.ChosenActivity = inferenceEngine.ChooseTrainingActivity(currentDataForTraining);

                if(verbose) print(JsonSerialization.ToJson(currentDataForTraining));

                Activity chosenActivity = activitiesDict[currentDataForTraining.ChosenActivity];
                Action action = ChooseRandomActionFromActivity(chosenActivity);
                
                PerformAction(action, false);
                yield return new WaitForSeconds(action.ActionInfo.TimeInMin * day.RTSecInSimMin);

                performedActivitiesData.Add(currentDataForTraining); // Record the chosen activity

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

    // Will not use it to get but to perform, need to change name
    private InferenceData GetTrainingDataWithIE(bool verbose = false)
    {
        InferenceData randomTrainingData = new InferenceData();
        randomTrainingData.InitializeRandomInferenceData(statesType);

        randomTrainingData.ChosenActivity = inferenceEngine.ChooseTrainingActivity(randomTrainingData);

        Activity chosenActivity = activitiesDict[randomTrainingData.ChosenActivity];
        // Paste from manuallyPerformActionForTraining
        // TODO: would need to change training with one state vs all of them
        Action action = ChooseRandomActionFromActivity(activitiesDict[randomTrainingData.ChosenActivity]); 
        List<STATE_TYPE> affectedStates = action.GetAffectedStates(statesType);

        if(affectedStates.Count == 0)
        {
            performedActivitiesData.Add(randomTrainingData); // Record the chosen activity, all the states (it is relax)
        }else
        {
            randomTrainingData.KeepOnlyOneState(affectedStates[0]);
            performedActivitiesData.Add(randomTrainingData); // Record the chosen activity, only the affected state
        }

        return randomTrainingData;
    }
    
    private Activity GetActivityWithIE()
    {
        InferenceData currentData = new InferenceData();
        currentData.InitializeInferenceData(states);
        print("DEBUG IN GET ACTIVITY WITH IE" + "  " + JsonSerialization.ToJson(currentData));

        ACTIVITY_TYPE chosenActivityType = inferenceEngine.InferActivity(currentData);
        Activity chosenActivity = activitiesDict[chosenActivityType];

        print("DEBUG IN GET ACTIVITY WITH IE" + "  " + chosenActivityType);
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

    IEnumerator AffectState(Action action)
    {
        if(hasDebugButtons) debugActivityButtons.SetButtonsInteractable(false);
        float waitingTime = action.ActionInfo.TimeInMin * day.RTSecInSimMin;
        yield return new WaitForSeconds(waitingTime);
        
        if(hasDebugButtons) debugActivityButtons.SetButtonsInteractable(true);
    }

    public void DebugResetStates()
    {
        foreach (State state in states)
        {
            state.UpdateValue(0);
        }
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
        }
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
    // public IEnumerator ManuallyPerformActionForTraining(ACTIVITY_TYPE activityType, bool verbose = false)
    // {
    //     print("ManuallyPerformActionForTraining");
    //     Activity manuallyChosenActivity = activitiesDict[activityType];
    //     Action action = ChooseRandomActionFromActivity(manuallyChosenActivity);

    //     SaveCurrentStatesData(manuallyChosenActivity, verbose);
    //     PerformAction(action, true);
    //     yield return new WaitForSeconds(action.ActionInfo.TimeInMin * day.RTSecInSimMin);
    // }

    private void SaveCurrentStatesData(Activity chosenActivity, bool verbose = false)
    {
        InferenceData currentStatesForManualTraining = new InferenceData();
        currentStatesForManualTraining.InitializeRandomInferenceData(statesType);
        currentStatesForManualTraining.ChosenActivity = chosenActivity.ActivityType;
        if(verbose) print(JsonSerialization.ToJson(currentStatesForManualTraining));
        
        performedActivitiesData.Add(currentStatesForManualTraining); // Record the chosen activity
    }

    private void SaveSingleCurrentStateData(State state, Activity chosenActivity, bool verbose = false)
    {
        InferenceData currentStateForManualTraining = new InferenceData();
        currentStateForManualTraining.AddStateData(state);
        print("DEBUG IN SAVE SINGLE STATE DATA" + "  " + state.CurrentValue);
        currentStateForManualTraining.ChosenActivity = chosenActivity.ActivityType;
        if(verbose) print(JsonSerialization.ToJson(currentStateForManualTraining));

        performedActivitiesData.Add(currentStateForManualTraining); // Record the chosen activity
    }

    
    public IEnumerator ManuallyPerformActionForTraining(ACTIVITY_TYPE activityType, bool verbose = true)
    {
        //     print("ManuallyPerformActionForTraining");
        //     Activity manuallyChosenActivity = activitiesDict[activityType];
        //     Action action = ChooseRandomActionFromActivity(manuallyChosenActivity);

        //     SaveCurrentStatesData(manuallyChosenActivity, verbose);
        //     PerformAction(action, true);
        //     yield return new WaitForSeconds(action.ActionInfo.TimeInMin * day.RTSecInSimMin);
        print("ManuallyPerformActionForTraining");
        Activity manuallyChosenActivity = activitiesDict[activityType];
        Action action = ChooseRandomActionFromActivity(manuallyChosenActivity);
        List<STATE_TYPE> affectedStates = action.GetAffectedStates(statesType);

        // Save the main affected state (the first one in the list), cause its the most important one and no need to worry about breaking something for now

        if(affectedStates.Count == 0)
        {
            SaveCurrentStatesData(manuallyChosenActivity);
        }
        else
        {
            State mainAffectedState = statesDict[affectedStates[0]];
            SaveSingleCurrentStateData(mainAffectedState, manuallyChosenActivity, verbose);
        }
        
        PerformAction(action, true);
        yield return new WaitForSeconds(action.ActionInfo.TimeInMin * day.RTSecInSimMin);
    }
}