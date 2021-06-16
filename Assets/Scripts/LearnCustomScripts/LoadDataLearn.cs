using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LoadDataLearn : MonoBehaviour
{
    public string path;
    private List<AgentData> data;
    public float counter = 0;
    public GameObject agentPrefab;
    private float minStartTime = 1000000;

    public class AgentData
    {
        public string name;
        public List<Vector3> positions;
        public List<float> timeSteps;
        public Vector3 startPos;
        public Vector3 goalPos;
        public float startTime;
        public float endTime;
    }

    // Start is called before the first frame update
    void Start()
    {
        readData();
        this.counter = this.minStartTime;
        Time.timeScale = 1f;
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

    private string getAgentName(string str)
    {
        string name = Path.GetFileName(str);
        return name.Remove(name.Length - 4);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (var agent in data)
        {
            if(Mathf.Approximately(agent.startTime, this.counter))
            {
                GameObject newAgent = Instantiate(agentPrefab, agent.positions[0], Quaternion.identity);
                newAgent.GetComponent<MoveAgentLearn>().setAgentData(agent);
            }
        }
        counter += 4;
    }
}
