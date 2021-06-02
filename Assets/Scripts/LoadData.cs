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

    private class AgentData
    {
        public string name;
        public List<Vector3> positions;
    } 
    
    // Start is called before the first frame update
    void Start()
    {
        startBtn = GameObject.FindGameObjectWithTag("StartButton").GetComponent<Button>();
        startBtn.interactable = false;
        readData();
        instantiateAgents();
    }

    public bool checkStartMoving()
    {
        return this.startMoving;
    }

    private void readData()
    {
        data = new List<AgentData>();
        foreach (string file in Directory.EnumerateFiles(path, "*.csv"))
        {
            AgentData agentTemp = new AgentData();
            agentTemp.name = getAgentName(file);
            agentTemp.positions = new List<Vector3>();
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    agentTemp.positions.Add(new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2])));
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

    private void instantiateAgents()
    {
        foreach (var agentTemp in data)
        {
            GameObject tempAgent = Instantiate(agentPrefab, agentTemp.positions[0], Quaternion.identity);
            tempAgent.GetComponent<WalkAgent>().setPositions(agentTemp.positions);
        }
        this.startMoving = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
