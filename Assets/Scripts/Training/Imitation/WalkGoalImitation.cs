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
    public float currentAngle;
    private Vector3 startingPos;
    public Vector3 goalPos;
    private float goalDistance;
    public float currentGoalDistance;
    public float reward;
    private SaveRoute saveRouteScript;
    private int localCounter = 1;
    private float moveSpeed;
    public float currentSpeed;
    private List<Collider> goals;

    private void Update()
    {
        this.reward = this.GetCumulativeReward();
        this.currentSpeed = this.agentRB.velocity.magnitude;
    }

    public override void Initialize()
    {
        this.agentRB = this.GetComponent<Rigidbody>();
        this.controller = GameObject.Find("Plane").GetComponent<LoadDataLearn>();
        this.dataScript = this.GetComponent<AssignData>();
        this.saveRouteScript = this.GetComponent<SaveRoute>();
        this.moveSpeed = 100f;
        
        if(this.controller.isImitation == true)
        {
            this.name = this.dataScript.agentData.name;
            this.nameText.text = int.Parse(this.name.Split('_')[1]).ToString();
        }
        else
        {
            //Find goals areas in scene
            this.goals = new List<Collider>();
            GameObject parentGoal = GameObject.Find("GoalAreas");
            foreach (Transform child in parentGoal.transform)
            {
                this.goals.Add(child.GetComponent<Collider>());
            }
        }
        setAgentColor();
    }

    public override void OnEpisodeBegin()
    {
        // If current run is imitation then get data from file, else create random points 
        if (this.controller.isImitation == true)
        {
            this.startingPos = this.dataScript.agentData.positions[0];
            this.goalPos = this.dataScript.agentData.goalPos;
        }
        else
        {
            this.startingPos = randomSpawnPoint();
            transform.localPosition = this.startingPos;
        }
        this.goalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        transform.LookAt(this.goalPos);
        this.transform.GetChild(1).GetComponent<TrailRenderer>().Clear();    
    }

    private Vector3 randomSpawnPoint()
    {
        // Sort list by collider size
        goals.Sort(compareBySize);

        //Select a goal area and a random spawn point in that area
        int randomSpawnIndex = getRandomIndex(goals);
        Collider tempArea = goals[randomSpawnIndex];
        //Set goal area
        this.goalPos = new Vector3(Random.Range(tempArea.bounds.min.x, tempArea.bounds.max.x), 0f, Random.Range(tempArea.bounds.min.z, tempArea.bounds.max.z));

        Vector3 spawnPoint = Vector3.zero;
        //Remove selected goal area so will not select is as goal area too
        Collider tempBeforeRemove = this.goals[randomSpawnIndex];
        this.goals.Remove(this.goals[randomSpawnIndex]);
        Collider tempArea2 = this.goals[randomSpawnIndex];
        spawnPoint = new Vector3(Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
        this.goals.Add(tempBeforeRemove);

        return spawnPoint;
    }

    //Compare collider size
    private int compareBySize(Collider a, Collider b)
    {
        Vector3 boundsA = a.bounds.size;
        Vector3 boundsB = b.bounds.size;

        float sizeA = boundsA.x * boundsA.z;
        float sizeB = boundsB.x * boundsB.z;
        return sizeA.CompareTo(sizeB);
    }

    // Return a weighted random collider area
    private int getRandomIndex(List<Collider> list)
    {
        int listSize = list.Count;
        List<float> colliderSize = new List<float>();
        float maxSize = 0f;
        float minSize = 100000f;

        foreach (Collider c in list)
        {
            float tempBoundSize = c.bounds.size.x * c.bounds.size.z;
            if (tempBoundSize > maxSize)
                maxSize = tempBoundSize;
            if (tempBoundSize < minSize)
                minSize = tempBoundSize;

            colliderSize.Add(tempBoundSize);
        }

        int retIndex = 0;
        float rd = Random.Range(minSize, maxSize);
        for (int i = 0; i < colliderSize.Count; i++)
        {
            if (rd >= colliderSize[i])
                retIndex = i;
        }
        return retIndex;
    }

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
            if (this.currentSpeed > 0.5f)
                AddReward(-0.0001f);

        // Reward if ditance to the goal decreases
        this.currentGoalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        if (this.currentGoalDistance < this.goalDistance)
        {
            AddReward(+0.0001f);
            this.goalDistance = this.currentGoalDistance;
        }
        else
            if (this.currentSpeed > 0.5f)
                AddReward(-0.0001f);

        // Add small punishment in every step if not stop to interact
        if(this.currentSpeed > 0.5f)
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
        this.agentRB.velocity = nextVector * this.moveSpeed;
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
        Vector3 nextTargetPos = this.dataScript.agentData.positions[this.localCounter];
        Vector3 lastPos = this.dataScript.agentData.positions[this.localCounter - 1];
        Vector3 heading = nextTargetPos - lastPos;
        // Get moving direction
        int direction = angleDir(heading);

        Vector3 goalVector = this.dataScript.agentData.goalPos - transform.position;
        float goalAngle = Vector3.Angle(transform.forward, goalVector);
        float headingAngle = Vector3.Angle(transform.forward, heading);
        float headingDistance = Vector3.Distance(lastPos, nextTargetPos);

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

        if (this.localCounter + 1 < this.dataScript.agentData.timeSteps.Count)
            this.localCounter++;
    }

    private void disableAgent()
    {
        if(this.controller.isImitation == true)
        {
            this.GetComponent<DemonstrationRecorder>().Close();
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal" && StepCount >= 30f) {
            // Check if goal area reached contains the actual goal point
            if(other.bounds.Contains(this.goalPos) == false)
            {
                AddReward(-1f);
                EndEpisode();
                this.GetComponent<WalkGoalImitation>().enabled = false;
                this.gameObject.SetActive(false);
            }
            else
            {
                // If goal area reached contains the actual point, give extra reward if agent is close enought
                float goalReachedDistance = Vector3.Distance(transform.localPosition, this.goalPos);
                if (goalReachedDistance <= 4f)
                {
                    AddReward(+1f);                    
                    this.saveRouteScript.exportRouteAndEndEpisode(this.GetCumulativeReward());
                    disableAgent();
                    
                }
                else
                {
                    AddReward(+0.5f);
                    EndEpisode();
                    disableAgent();
                }
            }
        }
        if (other.tag == "AgentChild")
        {
            AddReward(-1f);
            EndEpisode();
            disableAgent();
        }
        if (other.tag == "Wall")
        {
            AddReward(-1f);
            EndEpisode();
            disableAgent();
        }
        if (other.tag == "Obstacle")
        {
            AddReward(-1f);
            EndEpisode();
            disableAgent();
        }
    }

    private void OnTriggerStay(Collider other)
    {
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
        if (dir > 0f)
            return 1;
        //Going left
        else if (dir < 0f)
            return -1;
        //Going straight
        else
            return 0;
    }

    // Set a random color to agent
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