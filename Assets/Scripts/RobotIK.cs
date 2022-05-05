using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class RobotIK : MonoBehaviour
{
    // arm elements
    public Transform Base;
    public Transform Arm;
    public Transform Arm2;
    public Transform UpperArm;
    public Transform EndAttachment;
    private Transform target;
    private GameObject targetObject;

    private Transform[] segmentArray;
    private Vector3[] rotationOnAxis;
    private Vector3[] segmentPosition;
    public float[] armAngles;
    private Vector3 targetPosition;

    public float samplingDistance = 3.25f;
    public float learningRate = 100f;
    public float distanceThreshold = 0.1f;
    public Vector3 finalPosition;
    private string targetWord;
    private string wordList;

    private List<string> tools = new List<string> { "Schraube", "Platine", "Gehäuse" };

    // for debugging

    private void Init()
    {
        segmentArray = new Transform[] { Base, Arm, Arm2, UpperArm, EndAttachment };
        armAngles = new float[]{
            GetControllerLocalEulerAngle(Base).y
            ,GetControllerLocalEulerAngle(Arm).x,GetControllerLocalEulerAngle(Arm2).z,GetControllerLocalEulerAngle(UpperArm).x,GetControllerLocalEulerAngle(EndAttachment).x};
        rotationOnAxis = new Vector3[] { Vector3.up, Vector3.right, Vector3.forward, Vector3.right, Vector3.right };
        segmentPosition = new Vector3[segmentArray.Length];
        for (int i = 0; i < segmentArray.Length; i++)
        {
            segmentPosition[i] = GetControllerLocalPosition(segmentArray[i]);
        }
        targetWord = PlayerPrefs.GetString("label");
        wordList = PlayerPrefs.GetString("wordList");
        if (!string.IsNullOrEmpty(targetWord) && wordList.Contains(targetWord))
        {
            TargetInit();
        }
    }


    private void TargetInit()
    {
        string sequenceKey = PlayerPrefs.GetString("sequenceKey");
        if (tools.Contains(targetWord))
        {
            GameObject[] gosWithTag = GameObject.FindGameObjectsWithTag(sequenceKey);
            foreach (GameObject go in gosWithTag)
            {
                if (go.name == targetWord)
                {
                    targetObject = go;
                    AddOutlineToDetectedTarget();
                }
            }
        }
        else
        {
            targetObject = GameObject.Find(targetWord);
            AddOutlineToDetectedTarget();
        }
    }

    private void Awake()
    {
        Init();
        //If mesh renderer's enabled in IKController prefab for Joint4, ToggleJoint4 is used
        // ToggleJointFour();
    }

    // Start is called before the first frame update
    void Start()
    {
        // only runs if there is a target word
        if (!string.IsNullOrEmpty(targetWord) && wordList.Contains(targetWord))
        {

            TargetInit();
            target = targetObject.transform;
            if (targetWord == "Fließband")
            {
                Transform child = target.GetChild(0);
                targetPosition = GetControllerPosition(child);
            }
            else if (targetWord == "Halte")
            {
                targetPosition = ForwardKinematics(armAngles);
            }
            else
            {
                targetPosition = GetControllerPosition(target);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Init();
        if (target == true)
        {
            UpdateIK(targetPosition, armAngles);
        }
    }

    private Vector3 GetControllerPosition(Transform bone)
    {
        return bone.position;
    }

    private Vector3 GetControllerLocalPosition(Transform bone)
    {
        return bone.localPosition;
    }

    private Vector3 GetControllerLocalEulerAngle(Transform bone)
    {
        return bone.localRotation.eulerAngles;
    }

    private Vector3 ForwardKinematics(float[] angles)
    {
        Vector3 currentPosition = GetControllerPosition(segmentArray[0]);
        Quaternion rotation = Quaternion.identity;
        for (int i = 1; i < segmentArray.Length; i++)
        {
            rotation *= Quaternion.AngleAxis(angles[i - 1], rotationOnAxis[i - 1]);
            Vector3 nextPosition = currentPosition + rotation * segmentPosition[i];
            currentPosition = nextPosition;
        }
        finalPosition = currentPosition;
        return currentPosition;
    }

    public float DistanceFromTarget(Vector3 targetPosition, float[] angles)
    {
        Vector3 point = ForwardKinematics(angles);
        return Vector3.Distance(point, targetPosition);
    }

    public float PartialGradient(Vector3 targetPosition, float[] angles, int i)
    {
        // Saves the angle,
        float angle = angles[i];
        // Gradient : [F(x+SamplingDistance) - F(x)] / h
        float f_x = DistanceFromTarget(targetPosition, angles);
        angles[i] += samplingDistance;
        float f_x_plus_d = DistanceFromTarget(targetPosition, angles);
        float gradient = (f_x_plus_d - f_x) / samplingDistance;
        // Restores the O.G angle
        angles[i] = angle;
        return gradient;
    }

    private void UpdateIK(Vector3 targetPosition, float[] angles)
    {
        if (DistanceFromTarget(targetPosition, angles) < distanceThreshold)
            return;
        for (int i = segmentArray.Length - 1; i >= 0; i--)
        {
            // Gradient descent
            // Update : Solution -= LearningRate * Gradient
            float gradient = PartialGradient(targetPosition, angles, i);
            angles[i] -= learningRate * gradient;
            // Early termination
            if (DistanceFromTarget(targetPosition, angles) < distanceThreshold)
                return;
            if (rotationOnAxis[i] == Vector3.forward)
            {
                segmentArray[i].transform.localEulerAngles = new Vector3(0, 0, angles[i]);
            }
            else if (rotationOnAxis[i] == Vector3.up)
            {
                segmentArray[i].transform.localEulerAngles = new Vector3(0, angles[i], 0);
            }
        }
    }

    private void ToggleJointFour()
    {
        GameObject jointFour = GameObject.Find("4.Joint");
        MeshRenderer render = jointFour.GetComponent<MeshRenderer>();
        render.enabled = false;
    }

    private void AddOutlineEffectToRobotArm()
    {
        List<GameObject> childGameObjectList = new List<GameObject>();
        GameObject robotArm = GameObject.Find("Base 2");
        GameObject jointFour = GameObject.Find("4.Joint");
        Transform[] children = robotArm.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            childGameObjectList.Add(children[i].gameObject);
        }
        for (int i = 0; i < childGameObjectList.Count; i++)
        {
            bool meshRendererExists = childGameObjectList[i].GetComponent<MeshRenderer>() ? true : false;
            if (childGameObjectList[i] != jointFour && meshRendererExists)
            {
                childGameObjectList[i].AddComponent<Outline>();
            }
        }

    }

    // using the outline asset from asset store to highlight around the target
    private void AddOutlineToDetectedTarget()
    {
        if (targetWord == "Hebe" || targetWord == "Halte" || targetWord == "Lege")
        {
            AddOutlineEffectToRobotArm();
        }
        else if (targetWord == "Platine")
        {
            GameObject board = targetObject.transform.Find("board").gameObject;
            board.AddComponent<Outline>();

        }
        else if (targetWord == "Gehäuse")
        {
            GameObject gehause = targetObject.transform.Find("Gehäuse (1)").gameObject;
            gehause.AddComponent<Outline>();
        }
        else if(targetWord == "Werkbank") {
            GameObject board = targetObject.transform.Find("Object016").gameObject;
            board.AddComponent<Outline>();
        }
        else if(targetWord == "Fließband") {
            // PCSConfig script = targetObject.GetComponentInChildren<PCSConfig>();
            // script.addOutline = true;
        }
        else
        {
            targetObject.AddComponent<Outline>();
        }
    }

}
