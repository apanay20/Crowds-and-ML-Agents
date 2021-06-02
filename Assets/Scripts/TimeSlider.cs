using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSlider : MonoBehaviour
{
    public float value;
    public Slider mainSlider;

    // Start is called before the first frame update
    void Start()
    {
        mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        this.value = 0.1f;
        mainSlider.value = this.value;
        Time.fixedDeltaTime = this.value;
        GameObject.Find("TimeTextValue").GetComponent<Text>().text = this.value + "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ValueChangeCheck()
    {
        this.value = mainSlider.value;
        if (this.value == 0f)
            this.value = 0.001f;
        Time.fixedDeltaTime = this.value;
        GameObject.Find("TimeTextValue").GetComponent<Text>().text = this.value+"";
    }
}
