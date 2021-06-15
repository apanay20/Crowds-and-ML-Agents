using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveAgentLearn : MonoBehaviour
{
    //this.GetComponent<Unity.MLAgents.Demonstrations.DemonstrationRecorder>().enabled = true;

    private int localCounter = 1;
    private LoadDataLearn.AgentData agentData;
    private float speed = 20f;

    // Start is called before the first frame update
    void Start()
    {
        this.name = this.agentData.name;
        setAgentColor();
        this.transform.GetChild(0).gameObject.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = this.name.Split('_')[1];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        try
        {
            transform.rotation = Quaternion.LookRotation(this.agentData.positions[localCounter] - transform.position);
            transform.position = Vector3.MoveTowards(transform.position, this.agentData.positions[localCounter], this.speed);
            this.localCounter++;
        }
        catch
        {
            this.gameObject.SetActive(false);
        }
    }

    public void setAgentData(LoadDataLearn.AgentData data)
    {
        this.agentData = data;
    }

    private void setAgentColor()
    {
        Color tailColor = new Color(
            UnityEngine.Random.Range(0.1f, 1f),
            UnityEngine.Random.Range(0.2f, 1f),
            UnityEngine.Random.Range(0.1f, 1f)
        );
        this.transform.GetChild(1).gameObject.GetComponentInChildren<TrailRenderer>().endColor = tailColor;
        this.GetComponent<Renderer>().material.SetColor("_Color", tailColor);
    }
}
