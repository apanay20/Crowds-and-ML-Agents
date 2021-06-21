using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WalkGoal : Agent
{
    public float rw;
    public GameObject startingGoal;
    private Vector3 startingPos;
    private bool initializedGoal = false;

    public override void OnEpisodeBegin()
    {
    }

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
        Debug.Log(moveX + " " + moveZ +" "+ speed);
        Vector3 nextPos = new Vector3(moveX, 0, moveZ);
        this.GetComponent<Rigidbody>().velocity = (nextPos - transform.position) * speed;
        if(speed > 0.1f)
            this.GetComponent<Rigidbody>().MoveRotation(Quaternion.LookRotation(nextPos - transform.position));

        if (this.initializedGoal)
        {
            Vector3 goalVector = this.startingPos - transform.position;
            float angle = Vector3.Angle(transform.TransformDirection(Vector3.forward), goalVector);
            if (angle <= 60f)
                AddReward(-0.2f);
            else
                AddReward(+0.05f);
        }
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
        if (other.tag == "Goal")
        {
            if (this.initializedGoal != true)
            {
                this.startingGoal = other.gameObject;
                this.startingPos = transform.position;
                this.initializedGoal = true;
            }
            else
            {
                //if goal area reached is same with the starting one
                if(GameObject.ReferenceEquals(this.gameObject, this.startingGoal))
                    AddReward(-1f);
                else
                    AddReward(+2f);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Agent")
        {
            float distance = Vector3.Distance(transform.position, other.gameObject.transform.position);
            if(distance <= 0.5f)
                AddReward(-0.05f);
        }
        else if (other.tag == "Obstacle")
        {
            Debug.Log(this.name + " OBSTACLE");
            AddReward(-0.1f);
        }
        else if (other.tag == "Road")
        {
            AddReward(-0.05f);
        }
    }
}
