using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSimulation : MonoBehaviour
{
	public Button startBtn;

	void Start()
	{
		Button btn = startBtn.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
	}

	void TaskOnClick()
	{
		GameObject.Find("LoadButton").GetComponent<LoadData>().instantiateAgents();
		startBtn.interactable = false;
	}
}
