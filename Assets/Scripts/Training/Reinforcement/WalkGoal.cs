using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WalkGoal : Agent
{
    private float moveSpeed = 2f;
    private float turnSpeed = 400f;
    private Rigidbody agentRB;
    private Vector3 startingPos;
    public float currentAngle;
    public Vector3 goalPos;
    private float goalDistance;
    public float currentGoalDistance;
    public float reward;
    public int leftRotationCount;
    public int rightRotationCount;
    private List<GoalAndSpawn> goals;
    private SaveRoute saveRouteScript;

    private class GoalAndSpawn
    {
        public Collider goalCollider;
        public List<Collider> spawnCollider;
    }

    private void Update()
    {
        this.reward = this.GetCumulativeReward();
    }

    public override void Initialize()
    {
        Time.timeScale = 1f;
        this.agentRB = this.GetComponent<Rigidbody>();
        this.saveRouteScript = this.GetComponent<SaveRoute>();

        //Find goals areas in scene
        this.goals = new List<GoalAndSpawn>();
        GameObject parentGoal = GameObject.Find("GoalAreas");
        for (int i = 0; i < parentGoal.transform.childCount; i++)
        {
            GameObject child = parentGoal.transform.GetChild(i).gameObject;
            GoalAndSpawn temp = new GoalAndSpawn();
            temp.goalCollider = child.GetComponent<Collider>();
            temp.spawnCollider = new List<Collider>();
            for (int j = 0; j < child.transform.childCount; j++)
                temp.spawnCollider.Add(child.transform.GetChild(j).GetComponent<Collider>());
            this.goals.Add(temp);
        }
    }

    public override void OnEpisodeBegin()
    {
        //Get a random agent spawn point and rotate agent to look at target
        this.startingPos = randomSpawnPoint();
        transform.localPosition = this.startingPos;
        transform.LookAt(this.goalPos);
        this.goalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        this.leftRotationCount = 0;
        this.rightRotationCount = 0;

        this.transform.GetChild(1).GetComponent<TrailRenderer>().Clear();
        setAgentColor();
    }

    // Compare collider size
    private int compareBySize(GoalAndSpawn a, GoalAndSpawn b)
    {
        Vector3 boundsA = a.goalCollider.bounds.size;
        Vector3 boundsB = b.goalCollider.bounds.size;

        float sizeA = boundsA.x * boundsA.z;
        float sizeB = boundsB.x * boundsB.z;
        return sizeA.CompareTo(sizeB);
    }

    private int getRandomIndex(List<GoalAndSpawn> list)
    {
        int listSize = list.Count;
        List<float> colliderSize = new List<float>();
        float maxSize = 0f;
        float minSize = 100000f;

        foreach (GoalAndSpawn c in list)
        {
            float tempBoundSize = c.goalCollider.bounds.size.x * c.goalCollider.bounds.size.z;
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

    private Vector3 randomSpawnPoint()
    {
        // Sort list by collider size
        goals.Sort(compareBySize);

        //Select a goal area and a random spawn point in that area
        int randomSpawnIndex = getRandomIndex(goals);
        Collider tempArea = goals[randomSpawnIndex].goalCollider;
        this.goalPos = new Vector3(Random.Range(tempArea.bounds.min.x, tempArea.bounds.max.x), 0f, Random.Range(tempArea.bounds.min.z, tempArea.bounds.max.z));

        float distanceLimit = 0;
        float spawnDistanceLimit = Academy.Instance.EnvironmentParameters.GetWithDefault("distance", distanceLimit);
        Vector3 spawnPoint = Vector3.zero;

        if(spawnDistanceLimit == 0)
        {
            //No limit, select a random goal area
            //Remove spawn area so will not select is as goal area too
            GoalAndSpawn tempBeforeRemove = this.goals[randomSpawnIndex];
            this.goals.Remove(this.goals[randomSpawnIndex]);
            Collider tempArea2 = this.goals[randomSpawnIndex].goalCollider;
            spawnPoint = new Vector3(Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
            this.goals.Add(tempBeforeRemove);
        }
        else
        {
            //Use current lesson's maximum spawn distance
            float spawnToGoalDistance = 0f;
            do
            {                
                List<Collider> tempSpawnAreas = this.goals[randomSpawnIndex].spawnCollider;
                int randomSpawnArea = Random.Range(0, tempSpawnAreas.Count);
                Collider tempArea2 = tempSpawnAreas[randomSpawnArea];
                spawnPoint = new Vector3(Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
                spawnToGoalDistance = Vector3.Distance(spawnPoint, this.goalPos);
            } while ( spawnToGoalDistance > spawnDistanceLimit || spawnToGoalDistance < (spawnDistanceLimit - 1) );
        }

        return spawnPoint;
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

        Debug.DrawRay(transform.position, transform.forward, Color.white);
        float m = transform.forward.magnitude / goalVector.magnitude;
        Debug.DrawRay(transform.position, goalVector * m, Color.green);

        AddReward(-1f / MaxStep);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal" && StepCount >= 5f) {
            // Check if goal reached is different than starting
            if (other.bounds.Contains(this.startingPos) == true)
            { 
                AddReward(-2f);
                EndEpisode();
            }
            // Check if goal area reached contains the actual goal point
            else if(other.bounds.Contains(this.goalPos) == false)
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