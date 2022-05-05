using System;
using System.IO;
using System.Collections;
using AsyncIO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using NetMQ;
using NetMQ.Sockets;

public class SceneChanger : MonoBehaviour
{
    private RecordDataRequester _recordDataRequestor;

    private AddSpacekeyEventRequester _addSpacekeyEventRequester;

    private StopRecordingRequester _stopRecordingRequester;

    // declaring a concurrent thread since other async methods can run only on the main thread, not
    // doing so results in an exception
    private readonly ConcurrentQueue<Action> runOnMainThread = new ConcurrentQueue<Action>();
    private string recordingStatus;




    private void Start()
    {
        ForceDotNet.Force();
        _recordDataRequestor = new RecordDataRequester();
        _addSpacekeyEventRequester = new AddSpacekeyEventRequester();
        _stopRecordingRequester = new StopRecordingRequester();
        recordingStatus = PlayerPrefs.GetString("recordingStatus");
        // request to start recording here because you have the buffer for the background operations in python to start

    }

    // coroutines to wait for the key press instead of blocking update
    IEnumerator OnPressJKey()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.J));
        LoadFixationCrossScene();
    }
    IEnumerator OnPressNKey()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.N));
        LoadUserFeedbackScene();
    }


    void Update()
    {
        recordingStatus = PlayerPrefs.GetString("recordingStatus");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (String.IsNullOrEmpty(recordingStatus) && !_recordDataRequestor.IsThreadRunning())
            {
                RequestToRecordData();
            }
            else if (recordingStatus == "enabled" && !_addSpacekeyEventRequester.IsThreadRunning())
            {
                RequestToAddSpacekeyEvent();
            }
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            RequestToStopRecording();
        }

        // coroutines on key presses
        StartCoroutine(OnPressJKey());
        StartCoroutine(OnPressNKey());
        if (!runOnMainThread.IsEmpty)
        {
            DequeueRunOnMainThread();
        }
    }


    public static void LoadWarehouseScene()
    {
        SceneManager.LoadScene("wareHouseScene");

    }

    public static void LoadFixationCrossScene()
    {
        SceneManager.LoadScene("fixationCrossScene");
    }

    public static void LoadUserFeedbackScene()
    {
        SceneManager.LoadScene("userFeedbackScene");
    }

    private void RequestToRecordData()
    {
        _recordDataRequestor.Start((string connectionStatus) => runOnMainThread.Enqueue(() =>
       {
           if (connectionStatus == "enabled")
           {
               PlayerPrefs.SetString("recordingStatus", "enabled");
               PlayerPrefs.Save();
               RequestToAddSpacekeyEvent();
           }
       }
        ));
    }

    private void RequestToAddSpacekeyEvent()
    {
        _addSpacekeyEventRequester.Start((string message) => runOnMainThread.Enqueue(() =>
       {
           if (message == "EventAdded")
           {
               Debug.Log("Event has been added");
               LoadFixationCrossScene();
           }
       }
        ));
    }

    private void RequestToStopRecording()
    {
        _stopRecordingRequester.Start((string message) => runOnMainThread.Enqueue(() =>
       {
           if (message == "Data Saved")
           {
               Debug.Log("Data has been saved");
               Application.Quit();
           }
       }
        ));
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

        if (_recordDataRequestor != null && _recordDataRequestor.IsThreadRunning())
        {
            _recordDataRequestor.Stop();
        }
        if (_addSpacekeyEventRequester != null && _addSpacekeyEventRequester.IsThreadRunning())
        {
            _addSpacekeyEventRequester.Stop();
        }
        if (_recordDataRequestor != null && _recordDataRequestor.IsThreadRunning())
        {
            _recordDataRequestor.Stop();
        }
        NetMQConfig.Cleanup();

    }
}
