using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RunningSlider : MonoBehaviour
{
    public Slider mainSlider;
    private LoadData dataObj;

    // Start is called before the first frame update
    void Start()
    {
        dataObj = GameObject.Find("LoadButton").GetComponent<LoadData>();        
        mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        mainSlider.value = 0;
    }

    void Update()
    {
        mainSlider.maxValue = Math.Abs(dataObj.maxTime / 10);
        //if (dataObj.pause == false)
            mainSlider.value = dataObj.timestep;
    }

    public void ValueChangeCheck()
    {
        if (dataObj.pause == true)
            GameObject.Find("RemainTextValue").GetComponent<Text>().text = ((dataObj.timestep/dataObj.maxTime) * 1000).ToString("F2") + "%";
        dataObj.timestep = (int)mainSlider.value;
    }
}
