using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RadiusGizmo : MonoBehaviour
{
    public int segments = 50;
    public float xradius = 2;
    public float yradius = 2;
    public MoveAgentLearn agent;
    LineRenderer line;

    void Start()
    {
        this.agent = this.transform.parent.gameObject.GetComponent<MoveAgentLearn>();
        line = gameObject.GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.useWorldSpace = false;
        CreatePoints();
        InvokeRepeating("blink", 0, 0.25f);

    }
    private void CreatePoints()
    {
        float x;
        float z;
        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            line.SetPosition(i, new Vector3(x, 0, z));

            angle += (360f / segments);
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
