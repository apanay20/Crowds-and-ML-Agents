using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignData : MonoBehaviour
{
    [HideInInspector]
    public LoadDataLearn.AgentData agentData;

    public void setAgentData(LoadDataLearn.AgentData data)
    {
        this.agentData = data;
        this.GetComponent<WalkGoalImitation>().enabled = true;
    }
}
