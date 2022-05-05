using System.Collections.Concurrent;
using System;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using AsyncIO;
using NetMQ;


public class FixationCrossIterationSequence : MonoBehaviour
{
    private float timer = 0.0f;
    public Text textObject;
    private string fixationCross = "+";

    private LabelRequester _labelRequester;

    private bool sendRequestForLabel = false;

    private ConcurrentQueue<Action> runOnMainThread = new ConcurrentQueue<Action>();


    private void Start()
    {
        ForceDotNet.Force();
        _labelRequester = new LabelRequester();
    }

    void Update()
    {
        timer += Time.deltaTime;
        //TODO: ping the EEG device to start recording
        textObject.fontSize = 55;
        textObject.text = "";
        if (timer >= 2)
        {
            textObject.text = fixationCross;
        }
        if (timer >= 3.5f)
        {
            textObject.text = "";
            if (!_labelRequester.IsThreadRunning() && !sendRequestForLabel)
            {
                SendRequestForLabel();
            }

        }
        if (!runOnMainThread.IsEmpty)
        {
            DequeueRunOnMainThread();
        }



    }

    private void SendRequestForLabel()
    {
        Debug.Log("request for label sent");
        _labelRequester.Start((string label) => runOnMainThread.Enqueue(() =>
        {
            if (label == "NoData")
            {
                Debug.Log("No data collected");
                textObject.fontSize = 14;
                textObject.text = "leider wurden keine Daten gesammelt, bitte versuchen Sie, das gleiche Wort noch einmal zu sagen";
                SceneChanger.LoadWarehouseScene();
            }
            else
            {
                PlayerPrefs.SetString("label", label);
                PlayerPrefs.Save();
                SceneChanger.LoadUserFeedbackScene();
            }
        }));
        sendRequestForLabel = true;
    }

    private void DequeueRunOnMainThread()
    {
        Action action;
        while (runOnMainThread.TryDequeue(out action))
        {
            action.Invoke();
        }
    }


    private void OnDestroy()
    {
        StopAllCoroutines();
        if (_labelRequester != null && _labelRequester.IsThreadRunning())
        {
            _labelRequester.Stop();
        }
        NetMQConfig.Cleanup();

    }

}
