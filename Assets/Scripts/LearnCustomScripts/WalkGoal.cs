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
    private Rigidbody agentRB;

    public override void Initialize()
    {
        this.agentRB = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        //float speed = actions.ContinuousActions[2];

        /*Vector3 nextPos = new Vector3(moveX, 0, moveZ);
        Vector3 dir = (nextPos - transform.position).normalized * speed;
        this.agentRB.velocity = dir;
        if (!dir.Equals(Vector3.zero))
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-nextPos), 30f);*/


        transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * 15f;

        if (this.initializedGoal)
        {
            Vector3 goalVector = this.startingPos - transform.position;
            float angle = Vector3.Angle(transform.TransformDirection(Vector3.forward), goalVector);
            /*if (angle <= 60f)
                AddReward(-0.05f);
            else
                AddReward(+0.001f);
            */
        }
    }

    //just for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");//transform.position.x;
        continuousActions[1] = Input.GetAxisRaw("Vertical");//transform.position.z;
        //continuousActions[2] = this.agentRB.velocity.magnitude;
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
                if(GameObject.ReferenceEquals(other.gameObject, this.startingGoal))
                    AddReward(-0.5f);
                else
                    AddReward(+2f);
                EndEpisode();
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
            AddReward(-0.05f);
        }
        else if (other.tag == "Road")
        {
            AddReward(-0.01f);
        }
    }
}
