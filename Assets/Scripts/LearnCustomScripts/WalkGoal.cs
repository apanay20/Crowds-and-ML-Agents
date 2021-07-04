using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WalkGoal : Agent
{
    private float moveSpeed = 3f;
    private float turnSpeed = 300f;
    private Rigidbody agentRB;
    private Vector3 startingPos;
    public float currentAngle;
    public Vector3 goalPos;
    private float goalDistance;
    public float currentGoalDistance;
    public float rw;
    public int leftRotationCount;
    public int rightRotationCount;
    public float stepsss;

    private class GoalAndSpawn
    {
        public Collider goalCollider;
        public Collider spawnCollider;
    }

    private void Update()
    {
        this.rw = this.GetCumulativeReward();
        this.stepsss = StepCount;
    }

    public override void Initialize()
    {
        Time.timeScale = 1f;
        this.agentRB = this.GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        this.startingPos = randomSpawnPoint();
        transform.localPosition = this.startingPos;
        transform.LookAt(this.goalPos);
        this.goalDistance = Vector3.Distance(transform.localPosition, this.goalPos);
        this.leftRotationCount = 0;
        this.rightRotationCount = 0;
        this.transform.GetChild(1).GetComponent<TrailRenderer>().Clear();
        setAgentColor();
    }

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
        List<GoalAndSpawn> goals = new List<GoalAndSpawn>();
        GameObject parentGoal = GameObject.Find("GoalAreas");
        for (int i = 0; i < parentGoal.transform.childCount; i++)
        {
            GoalAndSpawn temp = new GoalAndSpawn();
            temp.goalCollider = parentGoal.transform.GetChild(i).GetComponent<Collider>();
            temp.spawnCollider = parentGoal.transform.GetChild(i).GetChild(0).GetComponent<Collider>();
            goals.Add(temp);
        }
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
            goals.Remove(goals[randomSpawnIndex]);
            Collider tempArea2 = goals[randomSpawnIndex].goalCollider;
            spawnPoint = new Vector3(Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
        }
        else
        {
            //Use current lesson's maximum spawn distance
            float spawnToGoalDistance = 0f;
            do
            {
                Collider tempArea2 = goals[randomSpawnIndex].spawnCollider;
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
            }
            else if(other.bounds.Contains(this.goalPos) == false)
            {
                AddReward(-2f);
            }
            else
            {
                float goalReachedDistance = Vector3.Distance(transform.localPosition, this.goalPos);
                if (goalReachedDistance <= 3f)
                    AddReward(+2f);
                else
                    AddReward(+1f);
            }
            EndEpisode();
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
        Color randomColor = new Color(
            UnityEngine.Random.Range(0.1f, 1f),
            UnityEngine.Random.Range(0.2f, 1f),
            UnityEngine.Random.Range(0.1f, 1f)
        );
        try
        {
            this.transform.GetChild(1).gameObject.GetComponentInChildren<TrailRenderer>().endColor = randomColor;
            this.GetComponent<Renderer>().material.SetColor("_Color", randomColor);
        }
        catch
        {

        }
    }
}