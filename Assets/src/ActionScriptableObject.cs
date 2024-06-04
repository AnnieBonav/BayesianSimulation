using System.Collections.Generic;
using UnityEngine;

// Can manually add in the inspector the data for the action, so that the action can be dragged to an object that wants to use this data (Like an activity, which has a list of possible actions). This way, the activity can easuly get the ActionData, and actions can easily me generated and modified in runtime.
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ActionScriptableObject", order = 2)]
public class ActionScriptableObject : ScriptableObject
{
    public string ActionName;
    public ACTIVITY_TYPE ActivityType;
    public int TimeInMin;
    public List<AffectedState> AffectedStates;
    public Sprite Sprite;
}
