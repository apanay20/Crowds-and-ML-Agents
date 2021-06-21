using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RadiusGizmo : MonoBehaviour
{
    public bool isSpeed;
    public float completeAngle = 360f;
    public float height = 0f;
    public int segments = 50;
    public float xradius = 2;
    public float yradius = 2;
    public MoveAgentLearn agent;
    LineRenderer line;

    void Start()
    {
        this.agent = this.transform.parent.gameObject.GetComponent<MoveAgentLearn>();
        line = gameObject.GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        if(this.isSpeed == false)
            CreatePoints();
        //InvokeRepeating("blink", 0, 0.25f);
    }

    void FixedUpdate()
    {
        if(this.isSpeed == true)
        {
            this.completeAngle = this.transform.parent.gameObject.GetComponent<MoveAgentLearn>().speed * 360f;
            CreatePoints();
        }
    }
    private void CreatePoints()
    {
        line.positionCount = 0;
        line.positionCount = segments + 1;
        float x;
        float z;
        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            line.SetPosition(i, new Vector3(x, height, z));

            angle += (completeAngle / segments);
        }
    }

    private void blink()
    {
        if (this.agent.walkNear == true)
        {
            if (line.endWidth > 0.01f)
            {
                line.startWidth = 0.01f;
                line.endWidth = 0.01f;
            }
            else
            {
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;
            }
        }
    }
}
