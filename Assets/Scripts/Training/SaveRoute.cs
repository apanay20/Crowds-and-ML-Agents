using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveRoute : MonoBehaviour
{
    public bool saveRoute;
    public float saveRouteRewardThreshold;
    private List<Route> routeList;
    private int count;
    private string directoryPath;
    private LoadDataLearn dataScript;
    private bool locking = false;

    private class Route
    {
        public float timestep;
        public float pointX;
        public float pointZ;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.dataScript = GameObject.Find("Plane").GetComponent<LoadDataLearn>();
        // Initialize list to save agent's route if enabled
        this.routeList = new List<Route>();
        this.count = 0;
        if (this.saveRoute == true)
        {
            this.directoryPath = "./ExportedDatasets/" + SceneManager.GetActiveScene().name + "/";
            Directory.CreateDirectory(this.directoryPath);
        }
    }

    public void exportRouteAndEndEpisode(float reward)
    {
        if (this.saveRoute == true && reward >= this.saveRouteRewardThreshold)
        {
            this.locking = true;
            string filePath = this.directoryPath + this.gameObject.name + "-" + this.count + ".csv";
            this.count++;
            StreamWriter writer = new StreamWriter(filePath);
            for (int i = 1; i < this.routeList.Count; i++)
            {
                    writer.WriteLine((this.routeList[i].timestep).ToString("F4") + ";" + this.routeList[i].pointX + ";" + this.routeList[i].pointZ);
            }
            writer.Flush();
            writer.Close();
        }
        // Clear list
        this.routeList.Clear();
        // End Episode
        try
        {
            this.GetComponent<WalkGoal>().EndEpisode();
        }
        catch
        {
            if (gameObject.GetComponent<WalkGoalInteract>() != null)
                this.GetComponent<WalkGoalInteract>().EndEpisode();
            if (gameObject.GetComponent<WalkGoalImitation>() != null)
                this.GetComponent<WalkGoalImitation>().EndEpisode();
        }
    }

    private void FixedUpdate()
    {
        if (this.saveRoute == true && this.locking == false)
        {
            Route temp = new Route();
            temp.timestep = this.dataScript.counter / 100f;
            temp.pointX = transform.position.x;
            temp.pointZ = transform.position.z;
            this.routeList.Add(temp);
        }
    }
}
