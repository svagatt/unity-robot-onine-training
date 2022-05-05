using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotJoints {
public class RobotJoint : MonoBehaviour
{

    public Vector3 axis;
    public Vector3 startOffset;

    private void Awake() {
        startOffset = transform.localPosition;
    }
}
}
