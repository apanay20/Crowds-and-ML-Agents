using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveAgentLearn : MonoBehaviour
{
    //this.GetComponent<Unity.MLAgents.Demonstrations.DemonstrationRecorder>().enabled = true;

    private int localCounter = 1;
    private LoadDataLearn.AgentData agentData;
    public float speed;
    public Vector3 forward;
    public Vector3 goalVector;
    public float angle;

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
            Vector3 nextTargetPos = this.agentData.positions[localCounter];
            this.speed = Vector3.Distance(nextTargetPos, transform.position) / ((this.agentData.timeSteps[localCounter] - this.agentData.timeSteps[localCounter-1])/100);
            if(this.speed > 0.1f)
                transform.rotation = Quaternion.LookRotation(nextTargetPos - transform.position);
            transform.position = Vector3.MoveTowards(transform.position, nextTargetPos, this.speed);
            visualizeLines();
            calculateAngle();
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

    private void visualizeLines()
    {
        this.forward = transform.TransformDirection(Vector3.forward) * 3;
        Debug.DrawRay(transform.position, this.forward, Color.white);
        this.goalVector = this.agentData.goalPos - transform.position;
        float m = this.forward.magnitude / this.goalVector.magnitude;
        Debug.DrawRay(transform.position, this.goalVector * m, Color.green);
    }

    private void calculateAngle()
    {
        this.angle = Vector3.Angle(this.forward, this.goalVector);
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
