using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.HableCurve;

public class attempt2RotOff : MonoBehaviour {

    [SerializeField]
    Transform arm1;
    [SerializeField]
    Transform arm2;

    [SerializeField]
    Transform arm11;
    [SerializeField]
    Transform arm22;

    float arm1Length = 3f;
    float arm2Length = 2f;

    [SerializeField]
    Transform target;

    [SerializeField]
    float rotStep = 0.1f;
    float rotTemp;

    [SerializeField]
    List<Transform> objectsToJoint = new List<Transform>();

    InputAction startAction;
    InputAction enterAction;

    void Awake() {
        startAction = InputSystem.actions.FindAction("start");
        enterAction = InputSystem.actions.FindAction("next");
    }

    private void OnEnable() {
        startAction.started += StartCreateArmAnim;
        startAction.Enable();
        enterAction.started += StartAnim;
        enterAction.Enable();
    }

    private void OnDisable() {
        startAction.started -= StartCreateArmAnim;
        startAction.Disable();
        enterAction.started -= StartAnim;
        enterAction.Disable();
    }

    void StartAnim(InputAction.CallbackContext context) {
        print("starting!");
        StartCoroutine(Anim());
    }

    void StartCreateArmAnim(InputAction.CallbackContext context) {
        StartCoroutine(CreateArmAnim());
    }

    IEnumerator Anim() {
        StartCoroutine(Animations.MoveTo(Camera.main.transform, new Vector3(0f, -2.5f, -10f), 1f));
        yield return null;
    }

    private void Start() {
        arm11.localScale = Vector3.zero;
        arm11.GetChild(3).localScale = Vector3.zero;

        arm22.localScale = Vector3.zero;
        arm22.GetChild(1).localScale = Vector3.zero;

        arm1.localScale = Vector3.zero;
        arm1.GetChild(1).GetChild(2).localScale = Vector3.zero;

        arm2.localScale = Vector3.zero;
        arm2.GetChild(0).GetChild(0).GetChild(0).localScale = Vector3.zero;

        rotTemp = rotStep;
        rotStep = 0f;
    }

    IEnumerator CreateArmAnim() {
        float duration = 1f;
        float startAfter = 0.7f;

        // arm 11
        StartCoroutine(Animations.Scale(arm11, Vector3.one, duration));
        yield return new WaitForSeconds(duration * 0.75f * startAfter);

        StartCoroutine(Animations.Scale(arm11.GetChild(3), Vector3.one, duration));
        yield return new WaitForSeconds(duration * 0.5625f * startAfter * 0.8f);

        StartCoroutine(Animations.Scale(arm22, Vector3.one, duration));
        yield return new WaitForSeconds(duration * 0.75f * startAfter);

        StartCoroutine(Animations.Scale(arm22.GetChild(1), Vector3.one, duration));
        yield return new WaitForSeconds(duration * startAfter * 0.8f);

        yield return new WaitForSeconds(1f);

        // arm 1
        StartCoroutine(Animations.Scale(arm1, Vector3.one, duration));
        yield return new WaitForSeconds(duration * 0.75f * startAfter);

        StartCoroutine(Animations.Scale(arm1.GetChild(1).GetChild(2), Vector3.one, duration));
        yield return new WaitForSeconds(duration * 0.5625f * startAfter * 0.8f);

        StartCoroutine(Animations.Scale(arm2, Vector3.one, duration));
        yield return new WaitForSeconds(duration * 0.75f * startAfter);

        StartCoroutine(Animations.Scale(arm2.GetChild(0).GetChild(0).GetChild(0), Vector3.one, duration));
        yield return new WaitForSeconds(duration * startAfter * 0.8f);

        yield return new WaitForSeconds(1f);

        rotStep = rotTemp;
    }

    void Update() {
        UpdateArm(arm1, rotStep);
        UpdateArm(arm2, rotStep);
    }

    void UpdateArm(Transform arm, float rotStep) {
        Transform otherArm = arm11;
        if (arm == arm2)
            otherArm = arm22;

        float distToDesiredPrev = Vector3.Distance(ForwardKinematics(2, false), target.position);

        float percent = Mathf.Clamp(distToDesiredPrev / 5f, 0f, 1f);
        rotStep *= percent;

        float randRot = Random.Range(-rotStep, rotStep);

        arm.transform.eulerAngles += new Vector3(0f, 0f, randRot);
        float distToDesired = Vector3.Distance(ForwardKinematics(2, false), target.position);

        if (distToDesired > distToDesiredPrev) { // if original rotation was closer
            arm.transform.eulerAngles -= new Vector3(0f, 0f, randRot); // revert back to original
        }

        for (int i = 0; i < objectsToJoint.Count; ++i) {
            objectsToJoint[i].position = (Vector3)ForwardKinematics(i + 1, false) + new Vector3(0f, 0f, -0.2f);
        }
        
        otherArm.eulerAngles = arm.transform.eulerAngles + new Vector3(0f, 0f, -90f);
    }

    Vector2 ForwardKinematics(int jointNum, bool returnCorrect) {
        float arm1Rot = arm1.localEulerAngles.z * Mathf.Deg2Rad;
        float arm2Rot = arm2.localEulerAngles.z * Mathf.Deg2Rad;

        float xLoc = arm1Length * Mathf.Cos(arm1Rot);
        float yLoc = arm1Length * Mathf.Sin(arm1Rot);
        if (jointNum >= 2) {
            xLoc += arm2Length * Mathf.Cos(arm1Rot + arm2Rot);
            yLoc += arm2Length * Mathf.Sin(arm1Rot + arm2Rot);
        }

        if (returnCorrect)
            return new Vector2(-yLoc, xLoc) + (Vector2)transform.position;
        return new Vector2(xLoc, yLoc) + (Vector2)transform.position;
    }
}