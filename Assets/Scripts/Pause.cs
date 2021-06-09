using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public Button pauseBtn;
    private LoadData dataObj;

    void Start()
    {
        Button btn = pauseBtn.GetComponent<Button>();
        dataObj = GameObject.Find("LoadButton").GetComponent<LoadData>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        dataObj.pause = true;
        this.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Continue";
    }
}
