using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityDebugBtn : MonoBehaviour
{
    [SerializeField] private Agent agent;
    // TODO: Change so the need affected comes inside of the action
    [SerializeField] private Need need;
    [SerializeField] private Action action;
    
    public void CallAgentActivity()
    {
        agent.PerformAction(action);
    }
}
