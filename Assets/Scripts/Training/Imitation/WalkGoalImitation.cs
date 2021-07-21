using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Demonstrations;

public class WalkGoalImitation : Agent
{
    private LoadDataLearn controller;
    private AssignData dataScript;
    public TMPro.TMP_Text nameText;
    private Rigidbody agentRB;
    private float spawnDistanceLimit;
    public float currentAngle;
    [HideInInspector]
    public Vector3 startingPos;
    [HideInInspector]
    public Vector3 goalPos;
    private float goalDistance;
    public float currentGoalDistance;
    public float reward;
    private SaveRoute saveRouteScript;
    private int localCounter = 1;
    private float moveSpeed;
    public float currentSpeed;
    public GameObject neighbour;
    public bool hasNeighbour;
    public bool hasInteracted;
    private float neighAngle;
    [HideInInspector]
    public int episodeActivated = -1;
    public int currentEpisode;
    public int currentStep;

    public override void Initialize()
    {
        this.agentRB = this.GetComponent<Rigidbody>();
        this.controller = GameObject.Find("Plane").GetComponent<LoadDataLearn>();
        this.dataScript = this.GetComponent<AssignData>();
        this.saveRouteScript = this.GetComponent<SaveRoute>();
        
        if(this.controller.isImitation == true)
        {
            this.name = this.dataScript.agentData.name;
            this.nameText.text = int.Parse(this.name.Split('_')[1]).ToString();
            this.moveSpeed = 100f;
        }
        else
            this.moveSpeed = 50f;
        setAgentColor();
    }

    public override void OnEpisodeBegin()
    {
        this.neighbour = null;
        this.hasNeighbour = false;
        this.hasInteracted = false;
        this.episodeActivated = -1;

        if(this.controller.isImitation == true)
        {
            this.startingPos = this.dataScript.agentData.positions[0];
            this.goalPos = this.dataScript.agentData.goalPos;
        }

        transform.localPosition = this.startingPos;
        transform.LookAt(this.goalPos);
        float offset = Random.Range(-15f, 15f);
        transform.Rotate(0f, transform.rotation.y + offset, 0f);

        this.goalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        this.transform.GetChild(1).GetComponent<TrailRenderer>().Clear();    
    }

    private void Update()
    {
        this.currentEpisode = this.CompletedEpisodes;
        this.currentStep = this.StepCount;
        this.reward = this.GetCumulativeReward();
        this.currentGoalDistance = Vector3.Distance(transform.localPosition, this.goalPos);

        // If goal reached, give reward and stop
        if (this.currentGoalDistance <= 1f)
        {
            AddReward(+2f);
            this.saveRouteScript.exportRouteAndEndEpisode(this.GetCumulativeReward());
            disableAgent();
        }
        else
        {
            this.currentSpeed = this.agentRB.velocity.magnitude;

            Vector3 goalVector = this.goalPos - transform.localPosition;
            this.currentAngle = Vector3.Angle(transform.forward, goalVector);

            if (this.neighbour != null)
            {
                if (this.neighbour.activeSelf == false || this.episodeActivated != this.neighbour.GetComponent<Agent>().CompletedEpisodes)
                {
                    this.hasNeighbour = false;
                    this.neighbour = null;
                    this.episodeActivated = -1;
                }
            }
            if (this.neighbour != null)
                Debug.DrawLine(transform.position, this.neighbour.transform.position, Color.red);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.InverseTransformDirection(this.agentRB.velocity)); // 3
        sensor.AddObservation(transform.InverseTransformPoint(this.goalPos)); // 3
        sensor.AddObservation(this.hasNeighbour); //1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        moveAgent(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.DiscreteActions[0]);

        // Reward if angle betwenn forward and goal vector is small
        if (this.currentAngle < 45f)
            AddReward(+0.0001f);
        else
            AddReward(-0.0001f);

        // Reward if dsitance to the goal decreases
        if (this.currentGoalDistance < this.goalDistance)
        {
            AddReward(+0.0001f);
            this.goalDistance = this.currentGoalDistance;
        }
        else
            AddReward(-0.0001f);

        //Reward if move with its neigbour
        if (this.hasNeighbour == true)
        {
            float neighDistance = Vector3.Distance(transform.position, this.neighbour.transform.position);
            if(neighDistance <= 0.7f)
                AddReward(+0.0002f);
            else
                AddReward(-0.0002f);
        }

        // Add small punishment in every step
        AddReward(-0.001f);
    }

    private void moveAgent(float angle, float distance, int direction)
    {
        distance = Mathf.Clamp(distance, -0.01f, 0.1f);
        angle = Mathf.Clamp(angle, 0f, 1f);

        if (direction == 0)
            angle = -angle;

        if(this.controller.isImitation == false)
            angle *= 10f;

        transform.Rotate(0f, angle, 0f);
        this.agentRB.velocity = transform.forward * distance * this.moveSpeed;

        if (this.controller.isImitation == true && this.currentSpeed <= 0.1f)
            transform.LookAt(this.goalPos);

        // ----------------- DRAW RAYS ---------------------
        Debug.DrawRay(transform.position, transform.forward, Color.white);
        Vector3 goalVector = this.goalPos - transform.position;
        float m = transform.forward.magnitude / goalVector.magnitude;
        Debug.DrawRay(transform.position, goalVector * m, Color.green);    
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Get next point from list
        Vector3 nextTargetPos = this.dataScript.agentData.positions[this.localCounter];
        Vector3 lastPos = this.dataScript.agentData.positions[this.localCounter - 1];
        Vector3 heading = nextTargetPos - lastPos;
        // Get moving direction
        int direction = angleDir(heading);

        float headingAngle = Vector3.Angle(transform.forward, heading);
        float headingDistance = Vector3.Distance(lastPos, nextTargetPos);

        // Clamp valus as continuous actions range is -1f to 1f
        headingAngle = Mathf.Clamp(headingAngle, -1f, 1f);
        headingDistance = Mathf.Clamp(headingDistance, 0f, 1f);

        // ----------------------ASSIGN ACTIONS------------------------------
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //Angle between forward and next point
        continuousActions[0] = headingAngle;
        //Distance from current point to next point
        continuousActions[1] = headingDistance;

        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = direction;
        // ------------------------------------------------------------------

        if (this.localCounter + 1 < this.dataScript.agentData.timeSteps.Count)
            this.localCounter++;
    }

    private void disableAgent()
    {
        this.GetComponent<DemonstrationRecorder>().Close();
        transform.SetParent(GameObject.Find("InactiveAgents").transform);
        this.gameObject.SetActive(false); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "AgentChild")
        {
            AddReward(-2f);
            EndEpisode();
            disableAgent();
        }
        if (other.tag == "Wall")
        {
            AddReward(-2f);
            EndEpisode();
            disableAgent();
        }
        if (other.tag == "Obstacle")
        {
            AddReward(-2f);
            EndEpisode();
            disableAgent();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Agent")
        {
            if (this.neighbour == null && this.hasInteracted == false)
            {
                if (other.GetComponent<WalkGoalImitation>().hasInteracted == false)
                {
                    this.neighAngle = Vector3.Angle(transform.forward, other.transform.forward);
                    WalkGoalImitation otherScript = other.gameObject.GetComponent<WalkGoalImitation>();
                    float neighGoalDistance = Vector3.Distance(this.goalPos, otherScript.goalPos);
                    if (this.neighAngle <= 30f && neighGoalDistance <= 3f)
                    {
                        this.neighbour = other.gameObject;
                        this.hasNeighbour = true;
                        this.hasInteracted = true;
                        this.episodeActivated = other.GetComponent<Agent>().CompletedEpisodes;

                        otherScript.neighbour = this.gameObject;
                        otherScript.hasNeighbour = true;
                        otherScript.hasInteracted = true;
                        otherScript.episodeActivated = this.CompletedEpisodes;
                    }
                }
            }
        }
        if (other.tag == "Road")
        {
            AddReward(-0.0001f);
        }
    }

    // Get next point direction
    private int angleDir(Vector3 targetDir)
    {
        // First calculate to cross product to get the perpendicular vector
        Vector3 perp = Vector3.Cross(transform.forward, targetDir);
        // Then get the dot product of the perpendicular vector and the up vector
        float dir = Vector3.Dot(perp, transform.up);

        //Going right
        if (dir >= 0f)
            return 1;
        //Going left
        else 
            return 0;
    }

    // Set a random color to agent
    private void setAgentColor()
    {
        Color randomColor = new Color(
            UnityEngine.Random.Range(0.1f, 0.9f),
            UnityEngine.Random.Range(0.2f, 0.9f),
            UnityEngine.Random.Range(0.1f, 0.9f)
        );
        try
        {
            this.GetComponent<Renderer>().material.SetColor("_Color", randomColor);
            this.transform.GetChild(1).gameObject.GetComponentInChildren<TrailRenderer>().endColor = randomColor;            
        }
        catch
        {

        }
    }
}
