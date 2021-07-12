using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WalkGoalImitation : Agent
{
    private MoveAgentLearn moveAgentScript;
    private Rigidbody agentRB;
    public float currentAngle;
    public Vector3 goalPos;
    private float goalDistance;
    public float currentGoalDistance;
    public float reward;
    private SaveRoute saveRouteScript;
    private int localCounter = 1;


    private void Update()
    {
        this.reward = this.GetCumulativeReward();
    }

    public override void Initialize()
    {
        this.moveAgentScript = this.GetComponent<MoveAgentLearn>();
        this.goalPos = this.moveAgentScript.agentData.goalPos;
        this.agentRB = this.GetComponent<Rigidbody>();
        this.saveRouteScript = this.GetComponent<SaveRoute>();
        setAgentColor();
    }

    public override void OnEpisodeBegin()
    {
        this.goalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        transform.LookAt(this.moveAgentScript.agentData.positions[1]);
        this.transform.GetChild(1).GetComponent<TrailRenderer>().Clear();    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.agentRB.position); // 3
        sensor.AddObservation(this.agentRB.rotation); // 4
        sensor.AddObservation(this.goalPos); // 3
        sensor.AddObservation(this.currentAngle); // 1
        sensor.AddObservation(this.goalDistance); // 1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        moveAgent(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.DiscreteActions[0]);

        // Reward if angle betwenn forward and goal vector is small
        Vector3 goalVector = this.goalPos - transform.localPosition;
        this.currentAngle = Vector3.Angle(transform.forward, goalVector);
        if (this.currentAngle < 45f)
            AddReward(+0.0001f);
        else
            AddReward(-0.0001f);

        // Reward if ditance to the goal decreases
        this.currentGoalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        if (this.currentGoalDistance < this.goalDistance)
        {
            AddReward(+0.0001f);
            this.goalDistance = this.currentGoalDistance;
        }
        else
            AddReward(-0.0001f);

        // Add small punishment in every step
        AddReward(-1f / MaxStep);
    }

    private void moveAgent(float angle, float distance, int direction)
    {
        // Decide direction
        if (direction < 0)
            angle = -angle;

        // Clamp values to avoid unexpected behavours
        angle = Mathf.Clamp(angle, -5f, 5f);
        distance = Mathf.Clamp(distance, 0f, 0.1f);

        Vector3 nextVector = transform.forward * distance;        
        transform.position += nextVector;
        transform.Rotate(0f, angle, 0f);

        // ----------------- DRAW RAYS ---------------------
        Debug.DrawRay(transform.position, transform.forward, Color.white);
        Vector3 goalVector = this.goalPos - transform.position;
        float m = transform.forward.magnitude / goalVector.magnitude;
        Debug.DrawRay(transform.position, goalVector * m, Color.green);    
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Get next point from list
        Vector3 nextTargetPos = this.moveAgentScript.agentData.positions[this.localCounter];
        Vector3 heading = nextTargetPos - this.moveAgentScript.agentData.positions[this.localCounter - 1];
        // Get moving direction
        int direction = angleDir(heading);

        Vector3 goalVector = this.moveAgentScript.agentData.goalPos - transform.position;
        float goalAngle = Vector3.Angle(transform.forward, goalVector);
        float headingAngle = Vector3.Angle(transform.forward, heading);
        float headingDistance = Vector3.Distance(this.moveAgentScript.agentData.positions[this.localCounter - 1], nextTargetPos);

        // ----------------------ASSIGN ACTIONS------------------------------
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //Angle between forward and next point
        continuousActions[0] = headingAngle;
        //Distance from current point tonext point
        continuousActions[1] = headingDistance;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        //Direction, -1 left, 0 straight, 1 right
        discreteActions[0] = direction;
        // ------------------------------------------------------------------

        if (this.localCounter + 1 < this.moveAgentScript.agentData.timeSteps.Count)
            this.localCounter++;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal" && StepCount >= 30f) {
            // Check if goal area reached contains the actual goal point
            if(other.bounds.Contains(this.goalPos) == false)
            {
                AddReward(-2f);
                EndEpisode();
                this.gameObject.SetActive(false);
            }
            else
            {
                // If goal area reached contains the actual point, give extra reward if agent is close enought
                float goalReachedDistance = Vector3.Distance(transform.localPosition, this.goalPos);
                if (goalReachedDistance <= 5f)
                {
                    AddReward(+2f);
                    this.saveRouteScript.exportRouteAndEndEpisode(this.GetCumulativeReward());
                    this.gameObject.SetActive(false);
                }
                else
                {
                    AddReward(+1f);
                    EndEpisode();
                    this.gameObject.SetActive(false);
                }
            }
        }
        if (other.tag == "AgentChild")
        {
            AddReward(-2f);
            EndEpisode();
            this.gameObject.SetActive(false);
        }
        if (other.tag == "Wall")
        {
            AddReward(-2f);
            EndEpisode();
            this.gameObject.SetActive(false);
        }
        if (other.tag == "Obstacle")
        {
            AddReward(-2f);
            EndEpisode();
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Road")
        {
            AddReward(-0.0001f);
        }
    }

    private int angleDir(Vector3 targetDir)
    {
        Vector3 perp = Vector3.Cross(transform.forward, targetDir);
        float dir = Vector3.Dot(perp, transform.up);

        //Going right
        if (dir > 0f)
            return 1;
        //Going left
        else if (dir < 0f)
            return -1;
        //Going straight
        else
            return 0;
    }

    private void setAgentColor()
    {
        try
        {
            Color randomColor = new Color(
                UnityEngine.Random.Range(0.1f, 0.9f),
                UnityEngine.Random.Range(0.2f, 0.9f),
                UnityEngine.Random.Range(0.1f, 0.9f)
            );
            this.GetComponent<Renderer>().material.SetColor("_Color", randomColor);
            this.transform.GetChild(1).gameObject.GetComponentInChildren<TrailRenderer>().endColor = randomColor;            
        }
        catch
        {

        }
    }
}