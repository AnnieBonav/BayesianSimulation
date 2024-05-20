using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    // TODO: Cahnge to be an event that the agent subscribes to
    [SerializeField]
    private Agent agent;

    private List<Agent> observers;

    private int lastMinute = 0;
    private int currentMinute = 0;


    // public Day()
    // {
    //     time = 0;
    //     timeOfDay = TOD.Morning;
    //     observers = new List<Agent>();
    // }
    
    public void Start(){
        dayDurationSec = dayDurationMin * 60;
        timeOfDayText.text = timeOfDay.ToString();
    }

    private float GetHour(){
        return timeOfDay * hoursInDay / dayDurationSec;
    }

    private float GetMinutes(){
        return (timeOfDay * hoursInDay * minutesInHour / dayDurationSec) % minutesInHour;
    }

    public void FixedUpdate(){
        currentMinute = Mathf.FloorToInt(GetMinutes());
        if(lastMinute < currentMinute){
            lastMinute = currentMinute;
            agent.PassTime(1);
        }
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
