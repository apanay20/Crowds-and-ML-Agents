using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAgent : MonoBehaviour
{
    public string name;
    public float speed = 5f;
    private int startTime;
    private List<Vector3> positions;
    private List<float> timeSteps;
    private LoadData trigger;
    private int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        trigger = GameObject.Find("LoadButton").GetComponent<LoadData>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(trigger.checkStartMoving() == true && (Time.time - trigger.timePassed) >= this.timeSteps[0] )
        {
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

    public void setPositions(List<Vector3> pos)
    {
        this.positions = pos;
    }

    public void setTimeSteps(List<float> times)
    {
        this.timeSteps = times;
    }
}
