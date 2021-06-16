using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveAgentLearn : MonoBehaviour
{
    //this.GetComponent<Unity.MLAgents.Demonstrations.DemonstrationRecorder>().enabled = true;

    private int localCounter = 1;
    private LoadDataLearn.AgentData agentData;
    public Color color;
    public float speed;
    public Vector3 forward;
    public Vector3 goalVector;
    public float angle;
    public bool walkNear;
    

    // Start is called before the first frame update
    void Start()
    {
        this.walkNear = false;
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

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.tag == "Agent")
        {
            MoveAgentLearn colliderAgent = collision.gameObject.GetComponent<MoveAgentLearn>();
            float movingAngle = Vector3.Angle(this.forward, colliderAgent.forward);
            float distanceBetween = Vector3.Distance(this.transform.position, collision.gameObject.transform.position);
            int counterDifference = Mathf.Abs(this.localCounter - colliderAgent.localCounter);
            if (movingAngle < 10f && distanceBetween <= 1.2f && this.localCounter < 40) {
                this.walkNear = true;
                colliderAgent.walkNear = true;
            }
        }
    }

    public void setAgentData(LoadDataLearn.AgentData data)
    {
        this.agentData = data;
    }

    private void visualizeLines()
    {
        this.forward = transform.TransformDirection(Vector3.forward) * 1.6f;
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
        Color randomColor = new Color(
            UnityEngine.Random.Range(0.1f, 1f),
            UnityEngine.Random.Range(0.2f, 1f),
            UnityEngine.Random.Range(0.1f, 1f)
        );
        this.color = randomColor;
        this.transform.GetChild(1).gameObject.GetComponentInChildren<TrailRenderer>().endColor = this.color;
        this.transform.GetChild(2).gameObject.GetComponentInChildren<LineRenderer>().startColor = this.color;
        this.transform.GetChild(2).gameObject.GetComponentInChildren<LineRenderer>().endColor = this.color;
        this.GetComponent<Renderer>().material.SetColor("_Color", this.color);

    }
}
