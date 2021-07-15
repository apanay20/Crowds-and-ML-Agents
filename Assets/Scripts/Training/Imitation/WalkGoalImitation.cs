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
    private List<GoalAndSpawn> goals;

    private class GoalAndSpawn
    {
        public Collider goalCollider;
        public List<Collider> spawnCollider;
    }

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
        this.goals.Sort(compareBySize);

        //Select a goal area and a random spawn point in that area
        int randomSpawnIndex = getRandomIndex();
        Collider tempArea = goals[randomSpawnIndex].goalCollider;
        this.goalPos = new Vector3(Random.Range(tempArea.bounds.min.x, tempArea.bounds.max.x), 0f, Random.Range(tempArea.bounds.min.z, tempArea.bounds.max.z));

        float distanceLimit = 0;
        float spawnDistanceLimit = Academy.Instance.EnvironmentParameters.GetWithDefault("distance", distanceLimit);
        Vector3 spawnPoint = Vector3.zero;

        if (spawnDistanceLimit == 0)
        {
            //No limit, select a random goal area
            //Remove selected goal area so will not select is as goal area too
            GoalAndSpawn tempBeforeRemove = this.goals[randomSpawnIndex];
            this.goals.Remove(this.goals[randomSpawnIndex]);
            randomSpawnIndex = getRandomIndex();
            Collider tempArea2 = this.goals[randomSpawnIndex].goalCollider;
            spawnPoint = new Vector3(Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
            this.goals.Add(tempBeforeRemove);
        }
        else
        {
            //Use current lesson's maximum spawn distance
            float spawnToGoalDistance = 0f;
            while (true)
            {
                List<Collider> tempSpawnAreas = this.goals[randomSpawnIndex].spawnCollider;
                int randomSpawnArea = Random.Range(0, tempSpawnAreas.Count);
                Collider tempArea2 = tempSpawnAreas[randomSpawnArea];
                spawnPoint = new Vector3(Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
                spawnToGoalDistance = Vector3.Distance(spawnPoint, this.goalPos);
                if (spawnToGoalDistance <= spawnDistanceLimit && spawnToGoalDistance >= (spawnDistanceLimit - 0.5f))
                    break;

            }
        }
        return spawnPoint;
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

    private int getRandomIndex()
    {
        int listSize = this.goals.Count;
        List<float> colliderSize = new List<float>();
        float maxSize = 0f;
        float minSize = 100000f;

        foreach (GoalAndSpawn c in this.goals)
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
            if (rd <= colliderSize[i])
            {
                retIndex = i;
                break;
            }                
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
        moveAgent(actions.ContinuousActions[0], actions.ContinuousActions[1]);

        // Reward if angle betwenn forward and goal vector is small
        Vector3 goalVector = this.goalPos - transform.localPosition;
        this.currentAngle = Vector3.Angle(transform.forward, goalVector);
        if (this.currentAngle < 45f)
            AddReward(+0.0001f);
        else if(this.currentAngle > 140f)
            AddReward(-0.001f);
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

        // Add small punishment in every step if not stop to interact
        AddReward(-1f / MaxStep);
    }

    private void moveAgent(float angle, float distance)
    {
        distance = Mathf.Clamp(distance, 0f, 0.1f);

        transform.Rotate(0f, angle, 0f);        
        this.agentRB.velocity = transform.forward * distance * this.moveSpeed;

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

        headingAngle *= direction;
        // Clamp valus as continuous actions range is -1f to 1f
        headingAngle = Mathf.Clamp(headingAngle, -1f, 1f);
        headingDistance = Mathf.Clamp(headingDistance, 0f, 1f);

        // ----------------------ASSIGN ACTIONS------------------------------
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //Angle between forward and next point
        continuousActions[0] = headingAngle;
        //Distance from current point to next point
        continuousActions[1] = headingDistance;
        // ------------------------------------------------------------------

        if (this.localCounter + 1 < this.dataScript.agentData.timeSteps.Count)
            this.localCounter++;
    }

    private void disableAgent()
    {
        if (this.controller.isImitation == true)
        {
            this.GetComponent<DemonstrationRecorder>().Close();
            this.gameObject.SetActive(false); 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal" && StepCount > 1f) {
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
                    disableAgent();                    
                }
                else
                {
                    AddReward(+1f);
                    EndEpisode();
                    disableAgent();
                }
            }
        }
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
            return -1;
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

/*using System.Collections;
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
    public float currentGoalAngle;
    public float movingAngle;
    private Vector3 startingPos;
    private Vector3 goalPos;
    private float goalDistance;
    public float currentGoalDistance;
    public float reward;
    private SaveRoute saveRouteScript;
    private int localCounter = 1;
    public float speed = 0f;
    private List<GoalAndSpawn> goals;

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
        this.agentRB = this.GetComponent<Rigidbody>();
        this.controller = GameObject.Find("Plane").GetComponent<LoadDataLearn>();
        this.dataScript = this.GetComponent<AssignData>();
        this.saveRouteScript = this.GetComponent<SaveRoute>();

        if (this.controller.isImitation == true)
        {
            this.name = this.dataScript.agentData.name;
            this.nameText.text = int.Parse(this.name.Split('_')[1]).ToString();
        }
        else
        {
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
        this.goals.Sort(compareBySize);

        //Select a goal area and a random spawn point in that area
        int randomSpawnIndex = getRandomIndex();
        Collider tempArea = goals[randomSpawnIndex].goalCollider;
        this.goalPos = new Vector3(Random.Range(tempArea.bounds.min.x, tempArea.bounds.max.x), 0f, Random.Range(tempArea.bounds.min.z, tempArea.bounds.max.z));

        float distanceLimit = 0;
        float spawnDistanceLimit = Academy.Instance.EnvironmentParameters.GetWithDefault("distance", distanceLimit);
        Vector3 spawnPoint = Vector3.zero;

        if (spawnDistanceLimit == 0)
        {
            //No limit, select a random goal area
            //Remove selected goal area so will not select is as goal area too
            GoalAndSpawn tempBeforeRemove = this.goals[randomSpawnIndex];
            this.goals.Remove(this.goals[randomSpawnIndex]);
            randomSpawnIndex = getRandomIndex();
            Collider tempArea2 = this.goals[randomSpawnIndex].goalCollider;
            spawnPoint = new Vector3(Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
            this.goals.Add(tempBeforeRemove);
        }
        else
        {
            //Use current lesson's maximum spawn distance
            float spawnToGoalDistance = 0f;
            while (true)
            {
                List<Collider> tempSpawnAreas = this.goals[randomSpawnIndex].spawnCollider;
                int randomSpawnArea = Random.Range(0, tempSpawnAreas.Count);
                Collider tempArea2 = tempSpawnAreas[randomSpawnArea];
                spawnPoint = new Vector3(Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
                spawnToGoalDistance = Vector3.Distance(spawnPoint, this.goalPos);
                if (spawnToGoalDistance <= spawnDistanceLimit && spawnToGoalDistance >= (spawnDistanceLimit - 0.5f))
                    break;

            }
        }
        return spawnPoint;
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

    private int getRandomIndex()
    {
        int listSize = this.goals.Count;
        List<float> colliderSize = new List<float>();
        float maxSize = 0f;
        float minSize = 100000f;

        foreach (GoalAndSpawn c in this.goals)
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
            if (rd <= colliderSize[i])
            {
                retIndex = i;
                break;
            }
        }
        return retIndex;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position.x); // 1
        sensor.AddObservation(transform.position.z); // 1
        sensor.AddObservation(this.goalPos); // 3
        sensor.AddObservation(this.currentGoalAngle); // 1
        sensor.AddObservation(this.movingAngle); // 1
        sensor.AddObservation(this.goalDistance); // 1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        moveAgent(actions.ContinuousActions[0], actions.ContinuousActions[1]);

        // Reward if angle betwenn forward and goal vector is small
        Vector3 goalVector = this.goalPos - transform.localPosition;
        this.currentGoalAngle = Vector3.Angle(transform.forward, goalVector);
        if (this.currentGoalAngle < 45f)
            AddReward(+0.0001f);
        else if (this.currentGoalAngle > 140f)
            AddReward(-0.001f);
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

        // Add small punishment in every step if not stop to interact
        AddReward(-1f / MaxStep);
    }

    private void moveAgent(float moveX, float moveZ)
    {
        moveX = Mathf.Clamp(moveX, -0.1f, 0.1f);
        moveZ = Mathf.Clamp(moveZ, -0.1f, 0.1f);

        Vector3 prevPos = transform.position;
        
        Vector3 nextPoint = transform.position + new Vector3(moveX, 0f, moveZ);

        //Get angle between forward and next point, need to be small
        this.movingAngle = Vector3.Angle(transform.forward, nextPoint - transform.position);
        if (this.movingAngle > 5f)
            AddReward(-0.0001f);
        else
            AddReward(+0.0001f);
        
        transform.LookAt(nextPoint);
        transform.position = nextPoint;

        this.speed = Vector3.Distance(transform.position, prevPos) * 10;

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
        Vector3 nextPointLocal = nextTargetPos - lastPos;

        float moveX = nextPointLocal.x;
        float moveZ = nextPointLocal.z;

        // ----------------------ASSIGN ACTIONS------------------------------
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //Angle between forward and next point
        continuousActions[0] = moveX;
        //Distance from current point to next point
        continuousActions[1] = moveZ;
        // ------------------------------------------------------------------

        if (this.localCounter + 1 < this.dataScript.agentData.timeSteps.Count)
            this.localCounter++;
    }

    private void disableAgent()
    {
        if (this.controller.isImitation == true)
        {
            this.GetComponent<DemonstrationRecorder>().Close();
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal" && StepCount > 1f)
        {
            // Check if goal area reached contains the actual goal point
            if (other.bounds.Contains(this.goalPos) == false)
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
                    disableAgent();
                }
                else
                {
                    AddReward(+1f);
                    EndEpisode();
                    disableAgent();
                }
            }
        }
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
        if (other.tag == "Road")
        {
            AddReward(-0.0001f);
        }
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
}*/