using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using AsyncIO;
using NetMQ;


public class DropdownButtonEventListener : MonoBehaviour
{
    private Button continueButton, breakButton, jaButton, neinButton;
    private TMP_Dropdown dropdownItem;
    public static string changedValue;
    private static string prevValue;
    private Canvas dropdownCanvas, detectedLabelCanvas;
    private TextMeshProUGUI detectedLabelText;
    private LoadSceneAsync loadSceneAsync;
    private GameObject loadingScreen;
    private LabelCorrectionRequester _labelCorrectionRequester;

    private ConcurrentQueue<Action> runOnMainThread = new ConcurrentQueue<Action>();

    private void LoadWarehouseScene() => SceneChanger.LoadWarehouseScene();

    private void Init()
    {
        // get button components
        continueButton = GameObject.Find("/DropdownCanvas/ContinueButton").GetComponentInChildren<Button>();
        jaButton = GameObject.Find("/DetectedLabelCanvas/JaButton").GetComponentInChildren<Button>();
        neinButton = GameObject.Find("/DetectedLabelCanvas/NeinButton").GetComponentInChildren<Button>();

        dropdownItem = GameObject.Find("/DropdownCanvas/Dropdown").GetComponentInChildren<TMP_Dropdown>();
        //get canvas component
        dropdownCanvas = GameObject.Find("DropdownCanvas").GetComponent<Canvas>();
        detectedLabelCanvas = GameObject.Find("DetectedLabelCanvas").GetComponent<Canvas>();
        //get label and set detected label text
        string label = PlayerPrefs.GetString("label");
        detectedLabelText = GameObject.Find("/DetectedLabelCanvas/DetectedLabelTxt").GetComponentInChildren<TextMeshProUGUI>();
        detectedLabelText.text = "Das ermittelte Wort ist, " + label + ", ist das richtig?"; 
    }

    private void Awake()
    {
        Init();
        dropdownCanvas.enabled = false;
        // unselect the default select value using -1
        dropdownItem.value = -1;

        jaButton.onClick.AddListener(OnClickJaButton);
        neinButton.onClick.AddListener(ActivateDropdownCanvas);

        dropdownItem.onValueChanged.AddListener(delegate
        {
            GetSelectedValue(dropdownItem);
        });

        continueButton.onClick.AddListener(OnClickContinueButton);
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        ForceDotNet.Force();
        _labelCorrectionRequester = new LabelCorrectionRequester();
    }

    // Update is called once per frame
    void Update()
    {
        if (changedValue != prevValue)
        {
            SetPlayerPrefs();
        }
        if (!runOnMainThread.IsEmpty)
        {
            DequeueRunOnMainThread();
        }

    }


    private void GetSelectedValue(TMP_Dropdown dropdownItem)
    {
        int num = dropdownItem.value;
        changedValue = dropdownItem.options[num].text;
        PlayerPrefs.SetString("label", changedValue);
        PlayerPrefs.Save();
    }

    private void SetPlayerPrefs()
    {

        string prevRemovedWords = PlayerPrefs.GetString("prevRemovedWords");
        if (string.IsNullOrEmpty(prevRemovedWords))
        {
            PlayerPrefs.SetString("prevRemovedWords", changedValue);
        }
        else
        {
            PlayerPrefs.SetString("prevRemovedWords", prevRemovedWords + " " + changedValue);
        }
        PlayerPrefs.Save();

        prevValue = changedValue;
    }

    private void SetPlayerPrefsOnJa()
    {

        string prevRemovedWords = PlayerPrefs.GetString("prevRemovedWords");
        string label = PlayerPrefs.GetString("label");
        if (string.IsNullOrEmpty(prevRemovedWords))
        {
            PlayerPrefs.SetString("prevRemovedWords", label);
        }
        else
        {
            PlayerPrefs.SetString("prevRemovedWords", prevRemovedWords + " " + label);
        }
        PlayerPrefs.Save();
    }

    private void ActivateDropdownCanvas()
    {
        detectedLabelCanvas.enabled = false;
        dropdownCanvas.enabled = true;
    }

    private void SendRequestToCorrectLabel()
    {
        Debug.Log("request to correct label sent");
        _labelCorrectionRequester.correctedLabel = PlayerPrefs.GetString("label");
        _labelCorrectionRequester.Start((string message) => runOnMainThread.Enqueue(() =>
        {
            if (message == "LabelCorrected")
            {
                LoadWarehouseScene();
            }
        }));
    }

    private void SendRequestToCorrectLabelWhenJa()
    {
        Debug.Log("request to correct label sent");
        _labelCorrectionRequester.correctedLabel = PlayerPrefs.GetString("label");
        _labelCorrectionRequester.Start((string message) => runOnMainThread.Enqueue(() =>
        {
            if (message == "LabelCorrected")
            {
                LoadWarehouseScene();
            }
        }));
    }

    private void DequeueRunOnMainThread()
    {
        Action action;
        while (runOnMainThread.TryDequeue(out action))
        {
            action.Invoke();
        }
    }

    private void OnClickJaButton()
    {
        if (!_labelCorrectionRequester.IsThreadRunning())
        {
            SendRequestToCorrectLabelWhenJa();
        }
        else
        {
            Debug.Log("label requester already started");
        }
        SetPlayerPrefsOnJa();
    }

    private void OnClickContinueButton() {
        if (!_labelCorrectionRequester.IsThreadRunning())
        {
            SendRequestToCorrectLabel();
            
        }
        else
        {
            Debug.Log("label requester already started");
        }
        SetPlayerPrefs();
        
    }



    private void OnDestroy()
    {
        StopAllCoroutines();
        if (_labelCorrectionRequester != null && _labelCorrectionRequester.IsThreadRunning())
        {
            _labelCorrectionRequester.Stop();
        }
        NetMQConfig.Cleanup();

    }
}
