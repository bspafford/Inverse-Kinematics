using UnityEngine;
using System.Collections.Generic;

public class Attempt2MessUpFK : MonoBehaviour {

    [SerializeField]
    Transform arm1;
    [SerializeField]
    Transform arm2;

    float arm1Length = 3f;
    float arm2Length = 2f;

    [SerializeField]
    Transform target;

    [SerializeField]
    float rotStep = 0.1f;

    [SerializeField]
    List<Transform> objectsToJoint = new List<Transform>();

    void Update() {
        UpdateArm(arm1, rotStep);
        UpdateArm(arm2, rotStep);
    }

    void UpdateArm(Transform arm, float rotStep) {
        float distToDesiredPrev = Vector3.Distance(ForwardKinematics(2), target.position);

        float percent = Mathf.Clamp(distToDesiredPrev / 5f, 0f, 1f);
        rotStep *= percent;

        float randRot = Random.Range(-rotStep, rotStep);

        arm.transform.eulerAngles += new Vector3(0f, 0f, randRot);
        float distToDesired = Vector3.Distance(ForwardKinematics(2), target.position);

        if (distToDesired > distToDesiredPrev) { // if original rotation was closer
            arm.transform.eulerAngles -= new Vector3(0f, 0f, randRot); // revert back to original
        }

        for (int i = 0; i < objectsToJoint.Count; ++i) {
            objectsToJoint[i].position = (Vector3)ForwardKinematics(i + 1) + new Vector3(0f, 0f, -0.2f);
        }
    }

    /*
    Vector2 ForwardKinematics() {
        float arm1Rot = arm1.localEulerAngles.z * Mathf.Deg2Rad;
        float arm2Rot = arm2.localEulerAngles.z * Mathf.Deg2Rad;

        float xLoc = arm1Length * Mathf.Cos(arm1Rot) + arm2Length * Mathf.Cos(arm1Rot + arm2Rot);
        float yLoc = arm1Length * Mathf.Sin(arm1Rot) + arm2Length * Mathf.Sin(arm1Rot + arm2Rot);

        return new Vector2(xLoc, yLoc) + (Vector2)transform.position;
        //return new Vector2(-yLoc, xLoc) + (Vector2)transform.position;
    }
    */

    Vector2 ForwardKinematics(int jointNum) {
        float arm1Rot = arm1.localEulerAngles.z * Mathf.Deg2Rad;
        float arm2Rot = arm2.localEulerAngles.z * Mathf.Deg2Rad;

        float xLoc = arm1Length * Mathf.Cos(arm1Rot);
        float yLoc = arm1Length * Mathf.Sin(arm1Rot);
        if (jointNum >= 2) {
            xLoc += arm2Length * Mathf.Cos(arm1Rot + arm2Rot);
            yLoc += arm2Length * Mathf.Sin(arm1Rot + arm2Rot);
        }

        return new Vector2(-yLoc, xLoc) + (Vector2)transform.position;
    }
}