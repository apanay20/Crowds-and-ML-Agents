using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSlider : MonoBehaviour
{
    public int value;
    public Slider mainSlider;

    // Start is called before the first frame update
    void Start()
    {
        mainSlider.maxValue = 100;
        Time.fixedDeltaTime = 0.1f;
        mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        this.value = 10;
        mainSlider.value = this.value;
        Time.timeScale = this.value;
        GameObject.Find("TimeTextValue").GetComponent<Text>().text = this.value + "x";
    }

    public void ValueChangeCheck()
    {
        this.value = (int) mainSlider.value;
        Time.timeScale = this.value;
        GameObject.Find("TimeTextValue").GetComponent<Text>().text = this.value + "x";
    }
}
