using UnityEngine;

public class ChangeTag : MonoBehaviour
{
    private void Start()
    {
        string sequenceKey = PlayerPrefs.GetString("sequenceKey");
        string label = PlayerPrefs.GetString("label");
    
        if (!string.IsNullOrEmpty(sequenceKey) &&  !string.IsNullOrEmpty(label))
        {
            if (label == "Gehäuse")
            {
                if (sequenceKey.Contains("it1"))
                {
                    this.gameObject.tag = "it1st3";
                }
                else if (sequenceKey.Contains("it2"))
                {
                    this.gameObject.tag = "it2st3";
                }
                else if (sequenceKey.Contains("it4"))
                {
                    this.gameObject.tag = "it4st3";
                }
            }
            else if (label == "Platine")
            {
                if (sequenceKey.Contains("it1"))
                {
                    this.gameObject.tag = "it1st2";
                }
                else if (sequenceKey.Contains("it3"))
                {
                    this.gameObject.tag = "it3st3";
                }
            }
        }
    }
}
