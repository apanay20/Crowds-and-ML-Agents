using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WalkGoalImitation : Agent
{
    private float moveSpeed = 2f;
    private float turnSpeed = 400f;
    private MoveAgentLearn moveAgentScript;
    private Rigidbody agentRB;
    private Vector3 startingPos;
    public float currentAngle;
    public Vector3 goalPos;
    private float goalDistance;
    public float currentGoalDistance;
    public float reward;
    public int leftRotationCount;
    public int rightRotationCount;
    private SaveRoute saveRouteScript;

    private void Update()
    {
        this.reward = this.GetCumulativeReward();
    }

    public override void Initialize()
    {
        this.moveAgentScript = this.GetComponent<MoveAgentLearn>();
        this.startingPos = this.moveAgentScript.agentData.startPos;
        this.goalPos = this.moveAgentScript.agentData.goalPos;
        this.agentRB = this.GetComponent<Rigidbody>();
        this.saveRouteScript = this.GetComponent<SaveRoute>();
    }

    public override void OnEpisodeBegin()
    {
        this.goalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        this.leftRotationCount = 0;
        this.rightRotationCount = 0;

        this.transform.GetChild(1).GetComponent<TrailRenderer>().Clear();
        setAgentColor();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.agentRB.position); // 3
        sensor.AddObservation(this.agentRB.rotation); // 4
        sensor.AddObservation(this.goalPos); // 3
        sensor.AddObservation(this.currentAngle); // 1
        sensor.AddObservation(this.goalDistance); // 1
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
            this.leftRotationCount++;
            this.rightRotationCount = 0;
            transform.Rotate(new Vector3(0f, -1f, 0f), turnSpeed * Time.fixedDeltaTime);
        }
        else if (dActions[1] == 1)
        {
            this.rightRotationCount++;
            this.leftRotationCount = 0;
            transform.Rotate(new Vector3(0f, 1f, 0f), turnSpeed * Time.fixedDeltaTime);
        }

        // Add a large punishment if agent make rotation around itself
        if (this.leftRotationCount >= 70 || this.rightRotationCount >= 70)
            AddReward(-0.05f);

        if (dActions[0] == 1)
        {
            this.agentRB.AddForce(transform.forward * this.moveSpeed * Time.deltaTime, ForceMode.VelocityChange);
            this.agentRB.velocity = Vector3.ClampMagnitude(this.agentRB.velocity, moveSpeed);
        }
        else if (dActions[0] == 0)
        {
            this.agentRB.velocity = this.agentRB.velocity / 1.05f;
        }

        Vector3 goalVector = this.goalPos - transform.localPosition;
        this.currentAngle = Vector3.Angle(transform.forward, goalVector);
        if (this.currentAngle < 45f)
            AddReward(+0.0001f);
        else
            AddReward(-0.0001f);

        this.currentGoalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        if (this.currentGoalDistance < this.goalDistance)
        {
            AddReward(+0.0001f);
            this.goalDistance = this.currentGoalDistance;
        }
        else
            AddReward(-0.0001f);

        AddReward(-1f / MaxStep);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal" && StepCount >= 5f) {
            // Check if goal area reached contains the actual goal point
            if(other.bounds.Contains(this.goalPos) == false)
            {
                AddReward(-2f);
                EndEpisode();
            }
            else
            {
                // If goal area reached contains the actual point, give extra reward if agent is close enought
                float goalReachedDistance = Vector3.Distance(transform.localPosition, this.goalPos);
                if (goalReachedDistance <= 5f)
                {
                    AddReward(+2f);
                    this.saveRouteScript.exportRouteAndEndEpisode(this.GetCumulativeReward());
                }
                else
                {
                    AddReward(+1f);
                    EndEpisode();
                }
            }
        }
        if (other.tag == "AgentChild")
        {
            AddReward(-2f);
            EndEpisode();
        }
        if (other.tag == "Wall")
        {
            AddReward(-2f);
            EndEpisode();
        }
        if (other.tag == "Obstacle")
        {
            AddReward(-2f);
            EndEpisode();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Road")
        {
            AddReward(-0.0001f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;
        actions[0] = 0;

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

    private void setAgentColor()
    {
        try
        {
            Color randomColor = new Color(
                UnityEngine.Random.Range(0.1f, 1f),
                UnityEngine.Random.Range(0.2f, 1f),
                UnityEngine.Random.Range(0.1f, 1f)
            );
            this.GetComponent<Renderer>().material.SetColor("_Color", randomColor);
            this.transform.GetChild(1).gameObject.GetComponentInChildren<TrailRenderer>().endColor = randomColor;            
        }
        catch
        {

        }
    }
}