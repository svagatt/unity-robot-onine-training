using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadSceneAsync : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isDataReceived = false;

    private AsyncOperation loadScreenAsync;
    private static Slider progress;
    public void Start()
    {
        progress = GameObject.Find("/LoadingScene/Slider").GetComponentInChildren<Slider>();
        progress.value = 0.5f;
    }

    // public IEnumerator LoadScene(string sceneName, GameObject loadingScreen)
    // {
    //     AsyncOperation loadingOp = SceneManager.LoadSceneAsync(sceneName);
    //     while (!loadingOp.isDone)
    //     {
    //         progress.value = 0.5f;
    //         if (isDataReceived)
    //     {
    //         float progressvalue = loadingOp.progress;
    //         progress.value = progressvalue;
    //         loadingOp.allowSceneActivation =progressvalue >= 0.9f? true: false;
    //         loadingScreen.SetActive(false);
    //     }
    //     yield return null;
    //     }
        
    // }

}
