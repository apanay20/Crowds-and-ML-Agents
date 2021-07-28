using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Unity.MLAgents;
using UnityEngine.SceneManagement;

public class LoadDataLearn : MonoBehaviour
{
    public bool isImitation;
    public string path;
    [HideInInspector]
    public List<AgentData> data;
    public float counter = -160f;
    public GameObject agentPrefabDemo;
    public int numOfAgents;
    public float neighbourPercentage;
    public GameObject agentPrefab;
    private float minStartTime = 1000000;
    private float minSpeed;
    private float maxSpeed;
    private List<GoalAndSpawn> goals;
    public Transform activeAgents;
    public Transform inactiveAgents;
    public Transform watiToSpawnTogether;
    [HideInInspector]
    public string directorySaveRoute;

    public class AgentData
    {
        public string name;
        public List<Vector3> positions;
        public List<float> timeSteps;
        public Vector3 startPos;
        public Vector3 goalPos;
        public float startTime;
        public float endTime;
        public float maxSpeed;
        public float minSpeed;
    }

    private class GoalAndSpawn
    {
        public Collider goalCollider;
        public List<Collider> spawnCollider;
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.DeleteAll();
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

        this.directorySaveRoute = "./ExportedDatasets/" + SceneManager.GetActiveScene().name + "_Time_" + DateTime.Now.ToString("hh-mm-ss") + "/";

        if (this.isImitation == true)
        {
            readData();
            findMinMaxSpeed();
            this.counter = this.minStartTime;
            Time.timeScale = 0.5f;
        }
        else
        {
            spawnInitialAgents();
            Time.timeScale = 0.4f;
}
    }

    private void readData()
    {
        data = new List<AgentData>();
        foreach (string file in Directory.EnumerateFiles(path, "*.csv"))
        {
            AgentData agentTemp = new AgentData();
            agentTemp.name = getAgentName(file);
            agentTemp.positions = new List<Vector3>();
            agentTemp.timeSteps = new List<float>();
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    agentTemp.timeSteps.Add(float.Parse(values[0]) * 100f);
                    agentTemp.positions.Add(new Vector3(float.Parse(values[1]), 0f, float.Parse(values[2])));
                    agentTemp.startPos = agentTemp.positions[0];
                    agentTemp.goalPos = agentTemp.positions[agentTemp.positions.Count-1];
                    agentTemp.startTime = agentTemp.timeSteps[0];
                    agentTemp.endTime = agentTemp.timeSteps[agentTemp.timeSteps.Count-1];
                }
            }
            if (agentTemp.startTime < this.minStartTime)
                this.minStartTime = agentTemp.startTime;
            data.Add(agentTemp);
        }
    }

    private void findMinMaxSpeed()
    {
        float minSpeedTemp = 10000;
        float maxSpeedTemp = 0;
        foreach (var agent in this.data)
        {
            for (int i = 0; i < agent.positions.Count - 1; i++)
            {
                float speedTemp = Vector3.Distance(agent.positions[i + 1], agent.positions[i]) / ((agent.timeSteps[i+1] - agent.timeSteps[i]) / 100);
                if (speedTemp > maxSpeedTemp)
                    maxSpeedTemp = speedTemp;
                if (speedTemp < minSpeedTemp)
                    minSpeedTemp = speedTemp;
            }
        }
        this.maxSpeed = maxSpeedTemp;
        this.minSpeed = minSpeedTemp;
    }

    private string getAgentName(string str)
    {
        string name = Path.GetFileName(str);
        return name.Remove(name.Length - 4);
    }

    public float normalizedSpeed(float speed)
    {
        return ((speed - this.minSpeed) / (this.maxSpeed - this.minSpeed));
    }

    private void spawnInitialAgents()
    {
        for(int i=0; i<this.numOfAgents; i++)
        {
            // [0] goal Pos, [1] spawnPos
            List<Vector3> points = generateGoalAndSpawnPoints(false);
            GameObject newAgent = Instantiate(agentPrefab, points[1], Quaternion.identity);
            newAgent.name = "Agent_" + i;
            PlayerPrefs.SetString(newAgent.name, points[0].x + ",0," + points[0].z + "," + points[1].x + ",0," + points[1].z);
            newAgent.transform.SetParent(this.inactiveAgents);
        }
    }

    private void respawnAgents()
    {     
        foreach (Transform child in this.inactiveAgents)
        {
            int childCount = this.watiToSpawnTogether.childCount;
            if (childCount < 2)
            {
                float rd = UnityEngine.Random.Range(0f, 100f);
                if (rd <= this.neighbourPercentage)
                    child.transform.SetParent(this.watiToSpawnTogether);
                else
                {
                    // [0] goal Pos, [1] spawnPos
                    List<Vector3> points = generateGoalAndSpawnPoints(false);
                    child.transform.SetParent(this.activeAgents);
                    PlayerPrefs.SetString(child.name, points[0].x+",0,"+points[0].z+","+points[1].x + ",0," + points[1].z);
                    child.gameObject.SetActive(true);
                }
            }
            // twoAgents found and ready to spawn together
            else
            {
                // [0] goal Pos 1, [1] spawnPos 1, [2] goal Pos 2, [3] spawnPos 2
                List<Vector3> points = generateGoalAndSpawnPoints(true);
                GameObject child0 = this.watiToSpawnTogether.transform.GetChild(0).gameObject;
                GameObject child1 = this.watiToSpawnTogether.transform.GetChild(1).gameObject;
                // Spawn first Agent
                PlayerPrefs.SetString(child0.name, points[0].x + ",0," + points[0].z + "," + points[1].x + ",0," + points[1].z);
                PlayerPrefs.SetString(child1.name, points[2].x + ",0," + points[2].z + "," + points[3].x + ",0," + points[3].z);
                // Spawn second Agent
                child1.transform.SetParent(this.activeAgents);
                child0.transform.SetParent(this.activeAgents);
                child0.SetActive(true);
                child1.SetActive(true);
            }
        }

        if (this.watiToSpawnTogether.childCount == 2)
        {
            // [0] goal Pos 1, [1] spawnPos 1, [2] goal Pos 2, [3] spawnPos 2
            List<Vector3> points = generateGoalAndSpawnPoints(true);
            GameObject child0 = this.watiToSpawnTogether.transform.GetChild(0).gameObject;
            GameObject child1 = this.watiToSpawnTogether.transform.GetChild(1).gameObject;
            // Spawn first Agent
            PlayerPrefs.SetString(child0.name, points[0].x + ",0," + points[0].z + "," + points[1].x + ",0," + points[1].z);
            PlayerPrefs.SetString(child1.name, points[2].x + ",0," + points[2].z + "," + points[3].x + ",0," + points[3].z);
            // Spawn second Agent
            child1.transform.SetParent(this.activeAgents);
            child0.transform.SetParent(this.activeAgents);
            child0.SetActive(true);
            child1.SetActive(true);
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.isImitation == true)
        {
            foreach (var agent in data)
            {
                if (Mathf.Approximately(agent.startTime, this.counter))
                {
                    GameObject newAgent = Instantiate(agentPrefabDemo, agent.positions[0], Quaternion.identity);
                    newAgent.GetComponent<AssignData>().setAgentData(agent);
                    newAgent.transform.SetParent(this.activeAgents);
                }
            }
            counter += 4;
        }
        else
            respawnAgents();
    }

    private List<Vector3> generateGoalAndSpawnPoints(bool isNeighbourPoints)
    {
        // [0] goal Pos, [1] spawnPos
        List<Vector3> retPoints = new List<Vector3>();

        float spawnDistanceLimit = Academy.Instance.EnvironmentParameters.GetWithDefault("distance", 0f);

        // Sort list by collider size
        this.goals.Sort(compareBySize);

        //Select a goal area and a random spawn point in that area
        int randomSpawnIndex = getRandomIndex(true);
        Collider tempArea = goals[randomSpawnIndex].goalCollider;
        Vector3 goalPoint = new Vector3(UnityEngine.Random.Range(tempArea.bounds.min.x, tempArea.bounds.max.x), 0f, UnityEngine.Random.Range(tempArea.bounds.min.z, tempArea.bounds.max.z));
        Vector3 goalPoint2 = Vector3.zero;
        //Select another goal point near the first
        if (isNeighbourPoints == true)
        {
            while(true)
            {
                goalPoint2 = new Vector3(UnityEngine.Random.Range(tempArea.bounds.min.x, tempArea.bounds.max.x), 0f, UnityEngine.Random.Range(tempArea.bounds.min.z, tempArea.bounds.max.z));
                float dis = Vector3.Distance(goalPoint, goalPoint2);
                if (dis >= 1.1f && dis < 1.6f)
                    break;
            }
        }


        //========================================= SELECT SPAWN POIN BELOW=================================================

        Vector3 spawnPoint = Vector3.zero;
        Vector3 spawnPoint2 = Vector3.zero;
        Collider tempArea2;

        if (spawnDistanceLimit == 0)
        {
            //No limit, select a random goal area
            //Remove selected goal area so will not select is as goal area too
            GoalAndSpawn tempBeforeRemove = this.goals[randomSpawnIndex];
            this.goals.Remove(this.goals[randomSpawnIndex]);
            randomSpawnIndex = getRandomIndex(false);
            tempArea2 = this.goals[randomSpawnIndex].goalCollider;
            spawnPoint = new Vector3(UnityEngine.Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, UnityEngine.Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
            this.goals.Add(tempBeforeRemove);
        }
        else
        {
            //Use current lesson's maximum spawn distance
            float spawnToGoalDistance = 0f;
            while (true)
            {
                List<Collider> tempSpawnAreas = this.goals[randomSpawnIndex].spawnCollider;
                int randomSpawnArea = UnityEngine.Random.Range(0, tempSpawnAreas.Count);
                tempArea2 = tempSpawnAreas[randomSpawnArea];
                spawnPoint = new Vector3(UnityEngine.Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, UnityEngine.Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
                spawnToGoalDistance = Vector3.Distance(spawnPoint, goalPoint);
                if (spawnToGoalDistance <= spawnDistanceLimit && spawnToGoalDistance >= (spawnDistanceLimit - 0.5f))
                    break;

            }
        }
        //Select another spawn point near the first
        if (isNeighbourPoints == true)
        {
            while(true)
            {
                spawnPoint2 = new Vector3(UnityEngine.Random.Range(tempArea2.bounds.min.x, tempArea2.bounds.max.x), 0f, UnityEngine.Random.Range(tempArea2.bounds.min.z, tempArea2.bounds.max.z));
                float dis = Vector3.Distance(spawnPoint, spawnPoint2);
                if (dis >= 0.5f && dis <= 1f)
                    break;
            };
        }
        
        retPoints.Add(goalPoint);
        retPoints.Add(spawnPoint);

        if(isNeighbourPoints == true)
        {
            retPoints.Add(goalPoint2);
            retPoints.Add(spawnPoint2);
        }
        return retPoints;
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

    private int getRandomIndex(bool isGoalPlace)
    {
        //Goal points selected randomly
        if (isGoalPlace == true)
            return UnityEngine.Random.Range(0, this.goals.Count - 1);
        // If is a spawn point, select random weighted relative to area size
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
        float rd = UnityEngine.Random.Range(minSize, maxSize);
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
}


