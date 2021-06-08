using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadData : MonoBehaviour
{
    public string path;
    public GameObject agentPrefab;
    private Button startBtn;
    private List<AgentData> data;
    private bool startMoving = false;
    public float timePassed;
    private float maxX;
    private float maxZ;
    private float minX;
    private float minZ;
    private float maxTime;
    private bool simulationStop = false;

    private class AgentData
    {
        public string name;
        public List<Vector3> positions;
        public List<float> timeSteps;
    } 
    
    // Start is called before the first frame update
    void Start()
    {
        this.path = PlayerPrefs.GetString("path");
        startBtn = GameObject.FindGameObjectWithTag("StartButton").GetComponent<Button>();
        startBtn.interactable = false;
        readData();
        findMaxPositionAndTime();
        Debug.Log("Min/Max X Coordinate are: "+this.minX+" / "+this.maxX);
        Debug.Log("Min/Max Z Coordinate are: " + this.minZ + " / " + this.maxZ);
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
        float maxXtemp = -1000000;
        float maxZtemp = -1000000;
        float minXtemp = 1000000;
        float minZtemp = 1000000;
        float maxTimeTemp = 0;
        foreach (var agent in data)
        {
            foreach (var pos in agent.positions)
            {
                if (pos.x > maxXtemp)
                    maxXtemp = pos.x;
                if (pos.z > maxZtemp)
                    maxZtemp = pos.z;
                if (pos.x < minXtemp)
                    minXtemp = pos.x;
                if (pos.z < minZtemp)
                    minZtemp = pos.z;
            }
            int len = agent.timeSteps.Count;
            if (agent.timeSteps[len - 1] > maxTimeTemp)
                maxTimeTemp = agent.timeSteps[len - 1];
        }
        this.maxX = maxXtemp;
        this.maxZ = maxZtemp;
        this.minX = minXtemp;
        this.minZ = minZtemp;
        this.maxTime = maxTimeTemp*10;
    }

    public void instantiateAgents()
    {
        foreach (var agentTemp in data)
        {
            GameObject newAgent = Instantiate(agentPrefab, agentTemp.positions[0], Quaternion.identity);
            newAgent.GetComponent<WalkAgent>().setName(agentTemp.name);
            newAgent.GetComponent<WalkAgent>().setPositions(agentTemp.positions);
            newAgent.GetComponent<WalkAgent>().setTimeSteps(agentTemp.timeSteps);
        }
        this.timePassed = Time.time; 
        this.startMoving = true;
    }

    public bool checkStartMoving()
    {
        return this.startMoving;
    }

    private void Update()
    {
        if (this.startMoving == true && this.simulationStop == false)
        {
            GameObject.Find("RemainText").GetComponent<Text>().enabled = true;
            float percentage = ((Time.time - this.timePassed) / this.maxTime * 1000);
            if(percentage >= 100)
            {
                this.simulationStop = true;
                GameObject.Find("RemainTextValue").GetComponent<Text>().text = "100.00%";
            }else
                GameObject.Find("RemainTextValue").GetComponent<Text>().text = percentage.ToString("F2") + "%";
        }
    }
}

