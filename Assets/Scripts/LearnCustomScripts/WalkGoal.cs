using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WalkGoal : Agent
{
    private float moveSpeed = 7f;
    private float turnSpeed = 3f;
    public Vector3 startingPos;
    private Rigidbody agentRB;
    public float currentAngle;
    public float startDistance;
    public float currentDistance;

    public override void Initialize()
    {
        Time.timeScale = 1f;
        this.agentRB = this.GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = randomSpawnPoint();
        transform.LookAt(Vector3.zero);
        this.startingPos = transform.localPosition;
        this.startDistance = 0f;
        this.transform.GetChild(1).GetComponent<TrailRenderer>().Clear();
    }

    private Vector3 randomSpawnPoint()
    {
        List<Collider> goalTargets = new List<Collider>();
        GameObject parentGoal = GameObject.Find("GoalAreas");
        for (int i = 0; i < parentGoal.transform.childCount; i++)
        {
            goalTargets.Add(parentGoal.transform.GetChild(i).GetComponent<Collider>());
        }
        int rd = Random.Range(0, goalTargets.Count);
        Collider tempArea = goalTargets[rd];
        return new Vector3(
            Random.Range(tempArea.bounds.min.x, tempArea.bounds.max.x),
            0f,
            Random.Range(tempArea.bounds.min.z, tempArea.bounds.max.z)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); // 3
        sensor.AddObservation(transform.localRotation); // 4
        sensor.AddObservation(currentAngle); // 1
        sensor.AddObservation(currentDistance); // 1
    }


    /// <summary>
    /// dActions[0] | (0) - Nothing, (1) - Running forward
    /// dActions[1] | (0) - Turn left, (1) - Turn right, (2) - Nothing
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        var dActions = actions.DiscreteActions;

        if (dActions[1] == 0)
        {
            transform.Rotate(new Vector3(0f, -1f, 0f) * turnSpeed);
            AddReward(-0.005f);
        }
        else if (dActions[1] == 1)
        {
            transform.Rotate(new Vector3(0f, 1f, 0f) * turnSpeed);
            AddReward(-0.005f);
        }

        if (dActions[0] == 0)
        {
            AddReward(-0.01f);
            this.agentRB.AddForce(this.agentRB.velocity.normalized * -0.01f, ForceMode.Impulse);
        }
        if (dActions[0] == 1)
        {
            this.agentRB.AddForce(transform.forward.normalized * this.moveSpeed * Time.deltaTime, ForceMode.VelocityChange);
            this.agentRB.velocity = Vector3.ClampMagnitude(this.agentRB.velocity, moveSpeed);
            //transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }

        //If moving away from starting position then give reward
        Vector3 startVector = this.startingPos - transform.localPosition;
        this.currentAngle = Vector3.Angle(transform.TransformDirection(Vector3.forward), startVector);
        if (this.currentAngle <= 90f)
            AddReward(-0.05f);
        else
            AddReward(+0.05f);

        this.currentDistance = Vector3.Distance(transform.localPosition, this.startingPos);
        if(this.currentDistance > this.startDistance)
        {
            AddReward(+0.05f);
            this.startDistance = currentDistance;
        }
        else
            AddReward(-0.05f);

        AddReward(-2f / MaxStep);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W))
        {
            actions[0] = 1;
        }

        actions[1] = 2;

        if (Input.GetKey(KeyCode.A))
        {
            actions[1] = 0;
        }
        if (Input.GetKey(KeyCode.D))
        {
            actions[1] = 1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal") {
            Debug.Log("Goal");
            if (Vector3.Distance(transform.localPosition, this.startingPos) > 6f)
            {
                // Check if goal reached is different than starting
                if (other.bounds.Contains(this.startingPos))
                    AddReward(-5f);
                else
                    AddReward(+5f);
                EndEpisode();
            }
        }
        if (other.tag == "AgentChild")
        {
            AddReward(-5f);
            EndEpisode();
        }
        if (other.tag == "Wall")
        {
            AddReward(-5f);
            EndEpisode();
        }
        if (other.tag == "Obstacle")
        {
            AddReward(-5f);
            EndEpisode();
        }

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Road")
        {
            AddReward(-0.01f);
        }
    }
}
