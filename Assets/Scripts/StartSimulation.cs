using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSimulation : MonoBehaviour
{
	public Button startBtn;
	public GameObject parentPanel;

	void Start()
	{
		Button btn = startBtn.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
	}

	void TaskOnClick()
	{
		parentPanel.SetActive(true);
		GameObject.Find("LoadButton").GetComponent<LoadData>().instantiateAgents();
		startBtn.interactable = false;
	}
}
