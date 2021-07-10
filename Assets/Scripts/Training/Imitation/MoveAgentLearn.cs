using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;

public class MoveAgentLearn : MonoBehaviour
{
    private LoadDataLearn controller;
    private int localCounter = 1;
    [HideInInspector]
    public LoadDataLearn.AgentData agentData;
    private Color color;
    public float speed;
    private Vector3 forward;
    private Vector3 goalVector;
    public float goalAngle;
    private List<GameObject> neighList;
    [HideInInspector]
    public bool walkNear;
    private bool stopMoving;
    public int direction;
    public float headingAngle;
    private Vector3 heading;

    private int AngleDir(Vector3 targetDir)
    {
        Vector3 perp = Vector3.Cross(transform.forward, targetDir);
        float dir = Vector3.Dot(perp, transform.up);

        //Going right
        if (dir > 0f)
            return 1;
        //Going left
        else if (dir < 0f)
            return -1;
        //Going straight
        else
            return 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.controller = GameObject.Find("Plane").GetComponent<LoadDataLearn>();
        this.walkNear = false;
        this.stopMoving = false;
        this.neighList = new List<GameObject>();
        this.name = this.agentData.name;
        setAgentColor();
        this.transform.GetChild(0).gameObject.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = int.Parse(this.name.Split('_')[1]).ToString();
    }

    void FixedUpdate()
    {
        Vector3 nextTargetPos = this.agentData.positions[localCounter];
        this.heading = nextTargetPos - transform.position;
        this.direction = AngleDir(this.heading);

        visualizeLines();
        calculateAngle();

        float currentSpeed = Vector3.Distance(nextTargetPos, this.agentData.positions[localCounter - 1]) / ((this.agentData.timeSteps[localCounter] - this.agentData.timeSteps[localCounter - 1]) / 100);
        this.speed = this.controller.normalizedSpeed(currentSpeed);

        this.GetComponent<Rigidbody>().velocity = (nextTargetPos - transform.position) * this.speed * 60f;
        if (this.speed > 0.1f)
            transform.rotation = Quaternion.LookRotation(nextTargetPos - transform.position);

        if (this.localCounter + 1 < this.agentData.timeSteps.Count)
            this.localCounter++;
        else
            this.stopMoving = true;
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.tag == "Goal" && this.stopMoving == true)
        {
            this.gameObject.SetActive(false);
        }
        /*if (collision.gameObject.tag == "Agent")
        {
            MoveAgentLearn colliderAgent = collision.gameObject.GetComponent<MoveAgentLearn>();

            if (this.neighList.IndexOf(collision.gameObject) < 0)
            {
                float movingAngle = Vector3.Angle(this.forward, colliderAgent.forward);
                int generalCounter = 0;
                int agreeCounter = 0;
                int colliderAgentIndex = colliderAgent.agentData.timeSteps.IndexOf(this.agentData.timeSteps[this.localCounter]);
                int agentIndex = this.localCounter;
                while ((agentIndex < this.agentData.timeSteps.Count - 1) && (colliderAgentIndex < colliderAgent.agentData.timeSteps.Count - 1) && (colliderAgentIndex >= 0) )
                {
                    float distanceBetween = Vector3.Distance(this.agentData.positions[agentIndex], colliderAgent.agentData.positions[colliderAgentIndex]);
                    if (distanceBetween <= 1.5f)
                        agreeCounter++;

                    generalCounter++;
                    colliderAgentIndex++;
                    agentIndex++;
                }
                if (generalCounter > 0)
                {
                    float ratio = agreeCounter / generalCounter;
                    if (movingAngle < 10f && ratio >= 0.7f)
                    {
                        this.walkNear = true;
                        colliderAgent.walkNear = true;
                        this.neighList.Add(collision.gameObject);
                    }
                }
            }
        }*/
    }

    public void setAgentData(LoadDataLearn.AgentData data)
    {
        this.agentData = data;
    }

    private void visualizeLines()
    {
        this.forward = transform.TransformDirection(Vector3.forward) * 1.5f;
        Debug.DrawRay(transform.position, this.forward , Color.white);

        float m = this.forward.magnitude / this.heading.magnitude;
        Debug.DrawRay(transform.position, this.heading * m, Color.blue);

        this.goalVector = this.agentData.goalPos - transform.position;
        m = this.forward.magnitude / this.goalVector.magnitude;
        Debug.DrawRay(transform.position, this.goalVector * m, Color.green);   

        foreach (var neigh in this.neighList)
        {
            for(var i=0;i<5;i++)
                Debug.DrawLine(transform.position, neigh.transform.position, Color.red);            
        }
    }

    private void calculateAngle()
    {
        this.goalAngle = Vector3.Angle(this.forward, this.goalVector);
        this.headingAngle = Vector3.Angle(this.forward, this.heading);
    }

    private void setAgentColor()
    {
        Color randomColor = new Color(
            UnityEngine.Random.Range(0.1f, 1f),
            UnityEngine.Random.Range(0.2f, 1f),
            UnityEngine.Random.Range(0.1f, 1f)
        );
        this.color = randomColor;
        try
        {
            this.transform.GetChild(1).gameObject.GetComponentInChildren<TrailRenderer>().endColor = this.color;
            this.transform.GetChild(2).gameObject.GetComponentInChildren<LineRenderer>().startColor = this.color;
            this.transform.GetChild(2).gameObject.GetComponentInChildren<LineRenderer>().endColor = this.color;
            this.transform.GetChild(3).gameObject.GetComponentInChildren<LineRenderer>().startColor = this.color;
            this.transform.GetChild(3).gameObject.GetComponentInChildren<LineRenderer>().endColor = this.color;
            this.GetComponent<Renderer>().material.SetColor("_Color", this.color);
        }
        catch
        {

        }
    }
}
