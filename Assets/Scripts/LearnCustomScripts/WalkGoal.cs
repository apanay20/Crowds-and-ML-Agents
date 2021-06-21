using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WalkGoal : Agent
{
    public float rw;
    public override void OnEpisodeBegin()
    {
    }

    [SerializeField] public List<Transform> targetTransform;
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.GetComponent<Rigidbody>().velocity.magnitude);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float speed = actions.ContinuousActions[2];
        this.GetComponent<Rigidbody>().velocity = (new Vector3(moveX, 0 , moveZ) - transform.position) * speed;
        AddReward(-0.005f);
    }

    //just for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = transform.position.x;
        continuousActions[1] = transform.position.z;
        continuousActions[2] = this.GetComponent<Rigidbody>().velocity.magnitude;
    }

    private void Update()
    {
        this.rw = this.GetCumulativeReward();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Goal")
        {
            AddReward(+2f);
        }
        else if(other.tag == "Agent")
        {
            AddReward(-1f);
        }
        else if (other.tag == "Obstacle")
        {
            AddReward(-1f);
        }
        else if (other.tag == "Road")
        {
            AddReward(-0.2f);
        }
    }
}
