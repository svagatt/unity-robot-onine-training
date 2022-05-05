using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DataPasser;
using IterationList;
using System.Linq;
using NetMQ;

public class DisplayScreenText : MonoBehaviour
{
    private TMP_Text textComponent;
    private string words;
    private Sequence sequenceToDisplay;
    private List<DictResemblerList> iterationSequences;

    private static string sequenceDescription;
    private static DictResemblerList item;
    public static List<string> sequenceList;
    private static string prevSelectedOption = "";

    private int ctr;
    private static string renderKey;

    private RetrainModelRequester _retrainModelRequester;

    private StopRecordingRequester _stopRecordingRequester;

    private readonly ConcurrentQueue<Action> runOnMainThread = new ConcurrentQueue<Action>();

    // display renderer objects
    GameObject[] sequenceKeyGameobjects;
    GameObject[] iterationGameobjects;

    GameObject loadingScreen;

    private void Awake()
    {
        iterationSequences = new List<DictResemblerList>();
        sequenceToDisplay = new Sequence();
        item = new DictResemblerList();
        _retrainModelRequester = new RetrainModelRequester();
        _stopRecordingRequester = new StopRecordingRequester();
        ctr = PlayerPrefs.GetInt("counter");
    }
    // Start is called before the first frame update

    private void Start()
    {
        loadingScreen = GameObject.Find("LoadingScene");
        var idp = new IterationDataPasser();
        iterationSequences = idp.GenerateIterationSeqList();
        var prevRemovedWords = PlayerPrefs.GetString("prevRemovedWords").Split(' ');

        // initialize list
        UpdateSequenceToDisplay();
        prevRemovedWords.ToList().ForEach(removedWord =>
        {
            RemoveItemFromList(removedWord);
        });

        if (!IsListFilled())
        {
            ctr++;
            if (ctr % 3 == 0)
            {
                RequestToRetrainModel();
            }
            if(ctr == 30) {
                RequestToStopRecording();
            }
            if(ctr <= 29) {
            PlayerPrefs.SetString("prevRemovedWords", "");
            PlayerPrefs.Save();
            SetCounterPlayerPrefs();
            UpdateSequenceToDisplay();
            SetTextToDisplay();
            }
        }
        else if (IsListFilled())
        {
            SetTextToDisplay();
        }
        if (!runOnMainThread.IsEmpty)
        {
            DequeueRunOnMainThread();
        }
        if(ctr <=29)
        {
            textComponent.text = "<color=#00FF00>" + sequenceDescription + "</color>" + "\n" + words;
        }
    }

    private void RemoveItemFromList(string option)
    {
        int index = sequenceList.IndexOf(option);
        if (index != -1)
        {
            sequenceList.RemoveAt(index);
        }
        prevSelectedOption = option;
    }

    private void UpdateSequenceToDisplay()
    {
        Debug.Log("---Sequence To Display being updated---");
        item = iterationSequences[ctr];
        string sequenceKey = item.key;
        PlayerPrefs.SetString("sequenceKey", sequenceKey);
        PlayerPrefs.Save();
        if (renderKey != sequenceKey)
        {
            DisablePreviousRenderOnSequenceChange();
        }
        EnableSequenceBasedDisplay();
        sequenceToDisplay = item.sequence;
        sequenceList = sequenceToDisplay.sequence.Select((val) => val).ToList();
        sequenceDescription = sequenceToDisplay.description;
    }

    private void SetTextToDisplay()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        foreach (string item in sequenceList)
        {
            words += item + " ";
        }
        PlayerPrefs.SetString("wordList", words);
    }

    private void SetCounterPlayerPrefs()
    {
        PlayerPrefs.SetInt("counter", ctr);
        PlayerPrefs.Save();
    }

    private bool IsListFilled()
    {
        return sequenceList.Count() > 0 ? true : false;
    }

    private void EnableSequenceBasedDisplay()
    {
        string sequenceKey = PlayerPrefs.GetString("sequenceKey");
        Debug.Log(sequenceKey);
        string iterationTag = "";
        Dictionary<string, string> sequenceItDict = new Dictionary<string, string>
        {
            {"it1", "iteration1"},
            {"it2", "iteration2"},
            {"it3", "iteration3"},
            {"it4", "iteration4"}
        };
        foreach (var kvp in sequenceItDict)
        {
            if (sequenceKey.Contains(kvp.Key))
            {
                iterationTag = kvp.Value;
            }
        }
        sequenceKeyGameobjects = GameObject.FindGameObjectsWithTag(sequenceKey);
        iterationGameobjects = GameObject.FindGameObjectsWithTag(iterationTag);
        RendererForGameObjectsBasedOnIteration(sequenceKeyGameobjects, true);
        RendererForGameObjectsBasedOnIteration(iterationGameobjects, true);
        renderKey = sequenceKey;


    }

    private void RendererForGameObjectsBasedOnIteration(GameObject[] gameObjects, bool flag)
    {
        foreach (GameObject go in gameObjects)
        {
            DisableMeshRenderer[] scripts = go.GetComponentsInChildren<DisableMeshRenderer>();
            if (scripts != null)
            {
                foreach (var script in scripts)
                {
                    script.enableMesh = flag;
                }
            }
            Renderer[] meshes = go.GetComponentsInChildren<Renderer>(true);
            if (meshes != null)
            {
                foreach (var mesh in meshes)
                {
                    mesh.enabled = flag;
                }
            }
        }

    }

    private void DisablePreviousRenderOnSequenceChange()
    {
        if (sequenceKeyGameobjects != null)
        {
            RendererForGameObjectsBasedOnIteration(sequenceKeyGameobjects, false);
        }
        if (iterationGameobjects != null)
        {
            RendererForGameObjectsBasedOnIteration(iterationGameobjects, false);
        }
    }
    // save an initial sequence key so render methods don't break
    private void saveInitialSequenceKey(List<DictResemblerList> iterationSequences)
    {
        int counter = PlayerPrefs.GetInt("counter");
        var item = iterationSequences[counter].key;
        PlayerPrefs.SetString("sequenceKey", item);
        PlayerPrefs.Save();
    }


    private void RequestToRetrainModel()
    {
        _retrainModelRequester.Start((string message) => runOnMainThread.Enqueue(() =>
       {
           if (message == "Retrained")
           {    
               Debug.Log("retrained");
           }
       }
        ));
    }

    private void RequestToStopRecording()
    {
        _stopRecordingRequester.Start((string message) => runOnMainThread.Enqueue(() =>
       {
           if (message == "DataSaved")
           {
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
        if (_retrainModelRequester != null && _retrainModelRequester.IsThreadRunning())
        {
            _retrainModelRequester.Stop();
        }
        if (_stopRecordingRequester != null && _stopRecordingRequester.IsThreadRunning())
        {
            _stopRecordingRequester.Stop();
        }
        NetMQConfig.Cleanup();

    }

}
