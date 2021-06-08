using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class FileManager : MonoBehaviour
{
    string path;
    public TMPro.TextMeshPro pathText;
    public TMPro.TextMeshPro numOfAgentsText;
    public Button startButton;
    public string sceneName;

    private void Start()
    {
        startButton.interactable = false;
    }
    public void OpenExplorer()
    {
        path = EditorUtility.OpenFolderPanel("Select", "", "");
        int allFileCount = Directory.GetFiles(@path).Length;
        int csvFileCount = Directory.GetFiles(path, "*.csv").Length;

        if (allFileCount != csvFileCount)
            pathText.text = "Selected folder has to contains only .csv files!";
        else if (allFileCount == 0)
            pathText.text = "Selected folder is empty!";
        else if (path != null)
        {
            pathText.text = path;
            numOfAgentsText.text = "Number of Agents: " + csvFileCount;
            startButton.interactable = true;
        }
        else
            pathText.text = "Please select a valid path!";

    }

    public void loadScene()
    {
        PlayerPrefs.SetString("path", this.path);
        SceneManager.LoadScene(this.sceneName);
    }
}
