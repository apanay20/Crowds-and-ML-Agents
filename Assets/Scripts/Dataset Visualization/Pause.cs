using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public Button pauseBtn;
    private LoadData dataObj;
    public Slider runningSlider;
    public bool paused = false;

    void Start()
    {
        Button btn = pauseBtn.GetComponent<Button>();
        dataObj = GameObject.Find("LoadButton").GetComponent<LoadData>();
        btn.onClick.AddListener(TaskOnClick);
        runningSlider.interactable = false;
    }

    void TaskOnClick()
    {
        if (this.paused == false)
        {
            this.paused = true;
            dataObj.pause = true;
            runningSlider.interactable = true;
            this.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Continue";
        }
        else
        {
            this.paused = false;
            dataObj.pause = false;
            runningSlider.interactable = false;
            this.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Pause";
        }
    }
}
