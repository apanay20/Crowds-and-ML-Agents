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
    private Rigidbody agentRB;
    public float currentAngle;
    private Vector3 startingPos;
    private float startDistance;
    private float currentStartDistance;
    private Vector3 goalPos;
    private float goalDistance;
    private float currentGoalDistance;

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
        this.goalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        //this.transform.GetChild(1).GetComponent<TrailRenderer>().Clear();
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

        goalTargets.Remove(goalTargets[rd]);
        rd = Random.Range(0, goalTargets.Count);

        Collider tempArea2 = goalTargets[rd];
        this.goalPos = new Vector3(Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));

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
        sensor.AddObservation(currentStartDistance); // 1
        sensor.AddObservation(currentGoalDistance); //1
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
            AddReward(-0.005f);
            this.agentRB.AddForce(this.agentRB.velocity.normalized * -0.01f, ForceMode.Impulse);
        }
        if (dActions[0] == 1)
        {
            this.agentRB.AddForce(transform.forward.normalized * this.moveSpeed * Time.deltaTime, ForceMode.VelocityChange);
            this.agentRB.velocity = Vector3.ClampMagnitude(this.agentRB.velocity, moveSpeed);
            //transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }

        //If moving away from starting position then give reward
        Vector3 goalVector = this.goalPos - transform.localPosition;
        Debug.DrawRay(transform.position, transform.forward, Color.white);
        Debug.DrawRay(transform.position, this.startingPos * 0.1f, Color.blue);
        Debug.DrawRay(transform.position, this.goalPos * 0.1f, Color.green);
        this.currentAngle = Vector3.Angle(transform.forward, goalVector);
        if (this.currentAngle > 40f)
            AddReward(-0.05f);
        else
            AddReward(+0.05f);

        this.currentStartDistance = Vector3.Distance(transform.localPosition, this.startingPos);
        if(this.currentStartDistance > this.startDistance)
        {
            AddReward(+0.05f);
            this.startDistance = this.currentStartDistance;
        }
        else
            AddReward(-0.05f);

        this.currentGoalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        if (this.currentGoalDistance < this.goalDistance)
        {
            AddReward(+0.05f);
            this.goalDistance = this.currentGoalDistance;
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
                    AddReward(+10f);
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
            AddReward(-0.001f);
        }
    }
}
