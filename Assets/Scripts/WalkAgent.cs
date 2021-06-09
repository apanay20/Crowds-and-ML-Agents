using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalkAgent : MonoBehaviour
{
    public string name;
    public float speed = 30f;
    private int startTime;
    private List<Vector3> positions;
    private List<float> timeSteps;
    private LoadData trigger;
    private int currentIndex = 0;
    private int lastTimestep = 0;

    // Start is called before the first frame update
    void Start()
    {
        trigger = GameObject.Find("LoadButton").GetComponent<LoadData>();
        this.transform.GetChild(0).gameObject.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = this.name.Split('_')[1];
        setAgentColor();
    }

    private void setAgentColor()
    {
        Color tailColor = new Color(
            Random.Range(0.1f, 1f),
            Random.Range(0.2f, 1f),
            Random.Range(0.1f, 1f)
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

    void Update()
    {
        if (trigger.checkStartMoving() == true && this.trigger.timestep >= this.timeSteps[0])
        {
            this.transform.GetChild(1).gameObject.SetActive(false);
            if (this.trigger.timestep > this.timeSteps[this.timeSteps.Count - 1])
            {
                this.transform.GetChild(0).gameObject.SetActive(false);
                this.GetComponent<Renderer>().enabled = false;
                this.GetComponent<Collider>().enabled = false;
                this.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                this.transform.GetChild(0).gameObject.SetActive(true);
                this.GetComponent<Renderer>().enabled = true;
                this.GetComponent<Collider>().enabled = true;
                //this.transform.GetChild(1).gameObject.SetActive(true);
                for (int i = 0; i < this.timeSteps.Count; i++)
                {
                    if (this.timeSteps[i] == this.trigger.timestep)
                    {
                        if (this.trigger.timestep <= this.lastTimestep)
                            transform.position = positions[i];
                        else
                            transform.position = Vector3.MoveTowards(transform.position, positions[i], this.speed);
                        this.lastTimestep = this.trigger.timestep;
                    }
                }
            }
        }
        else
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.GetComponent<Renderer>().enabled = false;
            this.GetComponent<Collider>().enabled = false;
            this.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void setName(string nm)
    {
        this.name = nm;
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
