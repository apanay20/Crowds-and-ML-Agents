using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WalkGoal : Agent
{   
    public string goalName;
    private float lastDistance;

    public override void OnEpisodeBegin()
    {
        this.lastDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
    }

    [SerializeField] public Transform targetTransform;
    /*public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }*/

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3.MoveTowards(transform.position, new Vector3(moveX, 0, moveZ), this.GetComponent<WalkAgent>().speed * Time.deltaTime);
        if (Vector3.Distance(transform.localPosition, targetTransform.localPosition) > this.lastDistance)
            AddReward(-0.5f);
        this.lastDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
    }

    //just for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = transform.position.x;
        continuousActions[1] = transform.position.z;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == goalName)
        {
            Debug.Log("Goal achieved by agent: " + goalName);
            AddReward(+2f);
        }
        if (other.gameObject.CompareTag("Agent"))
        {
            Debug.Log("Agent " + this.gameObject.name + " hit " + other.gameObject. name);
            AddReward(-1f);
        }
    }
}
