using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Day : MonoBehaviour
{
    [Header("Time of Day Settings (will alter the simulation Real Time)")]
    [SerializeField, Tooltip("How many hours on the Simulation Day (Simulation Hours in Simulation Day)")] private const int simHrInSimDay = 24;

    [SerializeField, Tooltip("How many minutes on the Simulation Hour (Simulation Minutes in Simulation Hour)")] private const int simMinInSimHr = 60;
    [ReadOnly] private int simMinInSimDay = -1;

    [SerializeField, Tooltip("How many minutes a Simulation Day should last in Real Time (Real Time Minutes in Simulation Day)")] private int rtMinInSimDay = 2;
    
    [ReadOnly, Tooltip("Day Duration Real Time Sec, defined by the above (used for calculations)")] private int rtSecInSimDay = -1;
    [ReadOnly, Tooltip("Min Duration Real Time Sec")] private float rtSecInSimMin = -1;
    public float RTSecInSimMin => rtSecInSimMin;
    [ReadOnly, Tooltip("Represents how long a Moment Of Day in the Simulation (morning, evening...) lasts in Simulation minutes")] private int simMODInSimMin = -1;

    [Header("Current Simulation Time info (Read Only)")]
    [ReadOnly, Tooltip("All time passed in Simulation in RT seconds")] private float rtTotalTimeSec = 0.0f;
    public float RTTotalTimeSec => rtTotalTimeSec;
    [ReadOnly, Tooltip("All minutes in Simulation (all Days)")] private int simAllMin = 0;
    // The last and current minutes (of the current hour) help with calculations (mainly calling the agents to update)
    public int SimAllMin => simAllMin;
    [ReadOnly, Tooltip("Current min in Simulation all Day Hours")] private int simCurrDayMin = 0;
    public int SimCurrDayMin => simCurrDayMin;
    // The last and current minutes (of the current hour) help with calculations (mainly calling the agents to update)
    [ReadOnly, Tooltip("Last min in Simulation current Hour")] private int simLastMinCurrHr = 0;
    [ReadOnly, Tooltip("Current min in Simulation current Hour")] private int simCurrMinCurrHr = 0;

    [Header("Current Simulation Moment of Day Info")]
    [ReadOnly] private int simDay = 1;
    public int SimDay => simDay;
    [SerializeField] private MOD_TAG modTag;
    public MOD_TAG ModTag => modTag;

    [Header("Subscribed agents")]
    // TODO: Change to be an event that the agent subscribes to
    [SerializeField] private Agent agent;
    private List<Agent> observers;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI digitalClockLabel;
    [SerializeField] private TextMeshProUGUI modTextLabel;
    
    private void Awake() {
        observers = new List<Agent>();
    }

    private void Start()
    {
        simMinInSimDay = simHrInSimDay * simMinInSimHr; // CHECKED
        rtSecInSimDay = rtMinInSimDay * 60; // The total seconds a day lasts is the amount of time the user wants the day to be in RT (in minutes) times how many seconds are in a minute (60) // CHECKED
        simMODInSimMin = simMinInSimDay / 4; // The amount of minutes a moment of the day (MOD) lasts is the amount of minutes in a Sim Day divided by 4 (4 moments in a day) // CHECKED
        rtSecInSimMin = (float)rtSecInSimDay / simMinInSimDay; // CHECKED
        modTextLabel.text = modTag.ToString();

        print($"STARTED: {this}");
    }

    private float GetHour()
    {
        return  simCurrDayMin / 60 % simHrInSimDay; // CHECKED
    }

    // TODO: Could probably just be saved? Returns the minutes in the day
    private float GetAllMinutes()
    {
        return simCurrDayMin * simHrInSimDay * simMinInSimDay / rtMinInSimDay; // CHECKED
    }

    private float GetMinutes()
    {
        return simCurrDayMin % simMinInSimHr; // CHECKED
    }

    private void CheckTimeOfDay()
    {
        // print("All Sim minutes: " + simAllMin + "All Sim minutes in Day" + simCurrDayMin + " Day Moment of Day duration in min: " + simMODInSimMin);

        // TODO: Change this to have timers, so that there is no need to check every frame
        // Could add a TOD class and whenever it is changed it will notify the agents
        if (simCurrDayMin >= 0 && simCurrDayMin < simMODInSimMin)
        {
            modTag = MOD_TAG.Morning;
            modTextLabel.text = "Morning";
        }
        else if (simCurrDayMin >= simMODInSimMin && simCurrDayMin < simMODInSimMin * 2)
        {
            modTag = MOD_TAG.Afternoon;
            modTextLabel.text = "Afternoon";
        }
        else if (simCurrDayMin >= simMODInSimMin * 2 && simCurrDayMin < simMODInSimMin * 3)
        {
            modTag = MOD_TAG.Evening;
            modTextLabel.text = "Evening";
        }
        else
        {
            modTag = MOD_TAG.Night;
            modTextLabel.text = "Night";
        }

        // TODO: fix day passing, could be better
        simDay = simAllMin / simMinInSimDay; // LIKED
    }

    private void PassTime()
    {
        simCurrMinCurrHr = Mathf.FloorToInt(GetMinutes());
        if(simLastMinCurrHr < simCurrMinCurrHr){
            simLastMinCurrHr = simCurrMinCurrHr;
            agent.PassTime();
        }
        else if(simLastMinCurrHr == 59 && simCurrMinCurrHr == 0){
            simLastMinCurrHr = 0;
        }
    }

    public void FixedUpdate()
    {
        // PassTime();
        // CheckTimeOfDay();
        
        // rtTotalTimeSec += Time.deltaTime;

        // simAllMin = Mathf.FloorToInt(rtTotalTimeSec / rtSecInSimMin); // CHECKED
        // simCurrDayMin = simAllMin % simMinInSimDay; // CHECKED

        // digitalClockLabel.text = Mathf.FloorToInt(GetHour()).ToString("00") + ":" + Mathf.FloorToInt(GetMinutes()).ToString("00"); // CHECKED
    }
}
