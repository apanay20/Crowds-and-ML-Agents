using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class WalkAgent : MonoBehaviour
{
    public string AgentName;
    public float speed = 300f;
    private int startTime;
    private List<Vector3> positions;
    private List<float> timeSteps;
    private LoadData trigger;
    private Dictionary<float, Vector3> hashData;

    // Start is called before the first frame update
    void Start()
    {
        trigger = GameObject.Find("LoadButton").GetComponent<LoadData>();
        listToHash();
        this.transform.GetChild(0).gameObject.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = this.AgentName.Split('_')[1];
        setAgentColor();
        if(trigger.IsTrain == true)
        {
            //this.GetComponent<WalkGoal>().goalName = "Goal_" + this.AgentName.Split('_')[1];
        }
    }

    private void listToHash()
    {
        this.hashData = new Dictionary<float, Vector3>();
        for(int i=0; i < this.timeSteps.Count; i++)
        {
            this.hashData.Add(this.timeSteps[i], this.positions[i]);
        }
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

    // Update is called once per frame
    /*void FixedUpdate()
    {
        if(trigger.checkStartMoving() == true && (Time.time - trigger.timePassed) >= this.timeSteps[0] )
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
            this.GetComponent<Renderer>().enabled = true;
            this.GetComponent<Collider>().enabled = true;
            currentIndex++;
            if (currentIndex < positions.Count)
            {
                transform.position = Vector3.MoveTowards(transform.position, positions[currentIndex], this.speed);
            }
            else
            {
                this.GetComponent<Renderer>().enabled = false;
                this.GetComponent<Collider>().enabled = false;
                this.transform.GetChild(1).gameObject.SetActive(false);
            }                
        }
    }*/

    void FixedUpdate()
    {
        if (trigger.checkStartMoving() == true && this.trigger.timestep >= this.timeSteps[0])
        {
            this.transform.GetChild(1).gameObject.SetActive(false);
            if (this.trigger.timestep > this.timeSteps[this.timeSteps.Count - 1])
            {
                setAppearance(false);
            }
            else
            {
                setAppearance(true);
                this.hashData.TryGetValue(this.trigger.timestep, out Vector3 nextPos);
                if(nextPos != Vector3.zero)
                {
                    transform.LookAt(nextPos);
                    transform.position = nextPos;
                }
            }
        }
        else
        {
            setAppearance(false);
        }
        /*if (trigger.checkStartMoving() == true && this.trigger.timestep >= this.timeSteps[0] && this.lastTimestep < this.positions.Count)
        {
            setAppearance(true);
            transform.rotation = Quaternion.LookRotation(this.positions[lastTimestep] - transform.position);
            transform.position = Vector3.MoveTowards(transform.position, this.positions[lastTimestep], this.speed);
            this.lastTimestep++;
        }
        else
        {
            setAppearance(false);
        }*/
    }

    private void setAppearance(bool val)
    {
        this.transform.GetChild(0).gameObject.SetActive(val);
        this.GetComponent<Renderer>().enabled = val;
        this.GetComponent<Collider>().enabled = val;
        //this.transform.GetChild(1).gameObject.SetActive(val);
    }

    public void setName(string nm)
    {
        this.AgentName = nm;
        this.name = "Agent_" + this.AgentName.Split('_')[1];
    }

    public void setPositions(List<Vector3> pos)
    {
        this.positions = pos;
    }

    public void setTimeSteps(List<float> times)
    {
        this.timeSteps = times;
    }
}