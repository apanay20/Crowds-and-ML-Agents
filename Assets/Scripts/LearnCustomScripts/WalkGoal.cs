using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WalkGoal : Agent
{   

    public override void OnEpisodeBegin()
    {
    }

    [SerializeField] public List<Transform> targetTransform;
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float speed = actions.ContinuousActions[2];
        transform.position += new Vector3(moveX, 0f, moveZ);
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
    }
}
