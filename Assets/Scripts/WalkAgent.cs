using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalkAgent : MonoBehaviour
{
    public string name;
    public float speed = 300f;
    private int startTime;
    private List<Vector3> positions;
    private List<float> timeSteps;
    private LoadData trigger;
    private int currentIndex = 0;

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
    void FixedUpdate()
    {
        if(trigger.checkStartMoving() == true && (Time.time - trigger.timePassed) >= this.timeSteps[0] )
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
            this.GetComponent<Renderer>().enabled = true;
            this.GetComponent<Collider>().enabled = true;
            currentIndex++;
            if (currentIndex < positions.Count)
            {
                float step = speed;
                transform.position = Vector3.MoveTowards(transform.position, positions[currentIndex], step);
            }
            else
                Destroy(this.gameObject);
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
