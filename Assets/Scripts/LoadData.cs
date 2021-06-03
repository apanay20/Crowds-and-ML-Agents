using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadData : MonoBehaviour
{
    public string path = "./Datasets/Zara";
    public GameObject agentPrefab;
    private Button startBtn;
    private List<AgentData> data;
    private bool startMoving = false;
    public float timePassed;
    private float maxX;
    private float maxZ;
    private float maxTime;

    private class AgentData
    {
        public string name;
        public int startTime;
        public List<Vector3> positions;
        public List<float> timeSteps;
    } 
    
    // Start is called before the first frame update
    void Start()
    {
        startBtn = GameObject.FindGameObjectWithTag("StartButton").GetComponent<Button>();
        startBtn.interactable = false;
        readData();
        findMaxPositionAndTime();
        Debug.Log("Max X Coordinate is: "+this.maxX);
        Debug.Log("Max Z Coordinate is: "+this.maxZ);
        Debug.Log(this.maxTime);
        this.GetComponent<Button>().interactable = false;
        this.GetComponentInChildren<Text>().text = "Data Loaded!";
        startBtn.interactable = true;
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
                    agentTemp.timeSteps.Add((float.Parse(values[0])*10) );
                    agentTemp.positions.Add(new Vector3(float.Parse(values[1]), 0f, float.Parse(values[2])));
                }
            }
            data.Add(agentTemp);
        }
    }

    private string getAgentName(string str)
    {
        string name = Path.GetFileName(str);
        return name.Remove(name.Length - 4);
    }

    private void findMaxPositionAndTime()
    {
        float maxXtemp = -1;
        float maxZtemp = -1;
        float maxTimeTemp = 0;
        foreach (var agent in data)
        {
            foreach (var pos in agent.positions)
            {
                if (Mathf.Abs(pos.x) > maxXtemp)
                    maxXtemp = Mathf.Abs(pos.x);
                if (Mathf.Abs(pos.z) > maxZtemp)
                    maxZtemp = Mathf.Abs(pos.z);
            }
            int len = agent.timeSteps.Count;
            if (agent.timeSteps[len - 1] > maxTimeTemp)
                maxTimeTemp = agent.timeSteps[len - 1];
        }
        this.maxX = maxXtemp;
        this.maxZ = maxZtemp;
        this.maxTime = maxTimeTemp*10;
    }

    public void instantiateAgents()
    {
        foreach (var agentTemp in data)
        {
            GameObject tempAgent = Instantiate(agentPrefab, agentTemp.positions[0], Quaternion.identity);
            tempAgent.GetComponent<WalkAgent>().setPositions(agentTemp.positions);
            tempAgent.GetComponent<WalkAgent>().setTimeSteps(agentTemp.timeSteps);
        }
        this.timePassed = Time.time; 
        this.startMoving = true;
    }

    public bool checkStartMoving()
    {
        return this.startMoving;
    }
}
