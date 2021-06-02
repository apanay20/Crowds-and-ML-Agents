using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAgent : MonoBehaviour
{
    public string name;
    public float speed = 5f;
    private List<Vector3> positions;
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
        if(trigger.checkStartMoving() == true)
        {
            currentIndex++;
            if (currentIndex < positions.Count) {
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, positions[currentIndex], step);
            }
        }
    }

    public void setPositions(List<Vector3> pos)
    {
        this.positions = pos;
    }
}
