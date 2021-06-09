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
        Time.fixedDeltaTime = 0.05f;
        mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        this.value = 1;
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
