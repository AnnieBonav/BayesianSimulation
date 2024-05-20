using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Day : MonoBehaviour
{
    
    [SerializeField]
    private float timeOfDay = 0.0f;
    public float TimeOfDay => timeOfDay;

    [SerializeField]
    private float totalTime = 0.0f;
    public float TotalTime => totalTime;

    [SerializeField]
    private const int hoursInDay = 24;

    [SerializeField]
    private const int minutesInHour = 60;

    public int dayDurationMin = 2;
    private float dayDurationSec;

    [SerializeField]
    private TextMeshProUGUI timeOfDayText;

    [SerializeField]
    private TOD_DESCRIBER todDescriber;
    public TOD_DESCRIBER TimeOfDayDescriber => todDescriber;

    [SerializeField]
    private TextMeshProUGUI timeOfDayDescriberText;

    // TODO: Cahnge to be an event that the agent subscribes to
    [SerializeField]
    private Agent agent;

    private List<Agent> observers;

    private int lastMinute = 0;
    private int currentMinute = 0;

    [SerializeField]
    private int dayPartDuration = 0;
    [SerializeField]
    private int dayNumber = 0;
    public int DayNumber => dayNumber;
    
    public void Start()
    {
        dayDurationSec = dayDurationMin * 60;
        timeOfDayText.text = timeOfDay.ToString();
        dayPartDuration = (int)(dayDurationSec / 4);
    }

    private float GetHour()
    {
        return timeOfDay * hoursInDay / dayDurationSec;
    }

    // TODO: Could probably just be saved? Returns the minutes in the day
    private float GetAllMinutes()
    {
        return timeOfDay * hoursInDay * minutesInHour / dayDurationSec;
        // return (timeOfDay * hoursInDay * minutesInHour / dayDurationSec) % dayNumber;
    }

    private float GetMinutes()
    {
        return (timeOfDay * hoursInDay * minutesInHour / dayDurationSec) % minutesInHour;
    }

    private void CheckTimeOfDay()
    {
        int allMInutes = Mathf.FloorToInt(GetAllMinutes());
        print("All minutes: " + allMInutes + " Day part duration: " + dayPartDuration);
        // TODO: Change this to have timers, so that there is no need to check every frame
        // Could add a TOD class and whenever it is changed it will notify the agents
        if (allMInutes >= 0 && allMInutes < dayPartDuration)
        {
            todDescriber = TOD_DESCRIBER.Morning;
            timeOfDayDescriberText.text = "Morning";
        }
        else if (allMInutes >= dayPartDuration && allMInutes < dayPartDuration * 2)
        {
            todDescriber = TOD_DESCRIBER.Afternoon;
            timeOfDayDescriberText.text = "Afternoon";
        }
        else if (allMInutes >= dayPartDuration * 2 && allMInutes < dayPartDuration * 3)
        {
            todDescriber = TOD_DESCRIBER.Evening;
            timeOfDayDescriberText.text = "Evening";
        }
        else
        {
            todDescriber = TOD_DESCRIBER.Night;
            timeOfDayDescriberText.text = "Night";
        }

        // TODO: fix day passing not working (stuck in morning of next day)
        // if(allMInutes >= dayDurationMin)
        // {
        //     dayNumber++;
        // }
    }

    private void PassTime()
    {
        currentMinute = Mathf.FloorToInt(GetMinutes());
        if(lastMinute < currentMinute){
            lastMinute = currentMinute;
            agent.PassTime(1);
        }
    }

    public void FixedUpdate()
    {
        PassTime();
        CheckTimeOfDay();
        
        totalTime += Time.deltaTime;
        timeOfDay = totalTime % dayDurationSec;
        timeOfDayText.text = Mathf.FloorToInt(GetHour()).ToString("00") + ":" + Mathf.FloorToInt(GetMinutes()).ToString("00");
    }

    public void TickInMin(int minutes = 10, int times = 1)
    {
        for (int i = 0; i < times; i++)
        {
            foreach (var agent in observers)
            {
                agent.PassTime(minutes);
            }
            timeOfDay += minutes;
        }
    }

    public void FakePassTime(int minutes = 10, string activity = null, string action = null)
    {
        foreach (var agent in observers)
        {
            agent.PassTime(minutes, activity, action);
        }
        timeOfDay += minutes;
    }
}
