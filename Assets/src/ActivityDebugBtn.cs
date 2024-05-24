using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityDebugBtn : MonoBehaviour
{
    [SerializeField] private Agent agent;
    // TODO: Change so the state affected comes inside of the action
    [SerializeField] private State state;
    [SerializeField] private Action action;
    
    public void CallAgentActivity()
    {
        agent.PerformAction(action);
    }
}
