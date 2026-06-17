using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class twoDRot : MonoBehaviour {
    InputAction startAction;

    public float duration = 1f;

    public float segment0Dir = 10f;
    public float temp = 10f;

    public Transform segment0;
    public Transform segment1;
    public Transform segment2;

    void Awake() {
        startAction = InputSystem.actions.FindAction("start");
    }

    private void OnEnable() {
        startAction.started += StartAnim;
        startAction.Enable();
    }

    private void OnDisable() {
        startAction.started -= StartAnim;
        startAction.Disable();
    }

    void StartAnim(InputAction.CallbackContext context) {
        print("starting!");
        StartCoroutine(Anim());
    }

    void Update() {
        segment0.eulerAngles = Vector3.zero;
        Vector3 segment0Loc = segment0.position;
        Vector3 segment2Tip = segment2.GetChild(0).GetChild(0).position;

        float center = segment2Tip.x - segment0Loc.x;
        print(center);
        segment0.eulerAngles = new Vector3(0f, 0f, Mathf.LerpUnclamped(segment0Dir, -segment0Dir, center / temp));
    }

    IEnumerator Anim() {
        StartCoroutine(Animations.Rotate(segment1, new Vector3(0f, 0f, -50.1f + 360f + 360f), duration, true));
        StartCoroutine(Animations.Rotate(segment2, new Vector3(0f, 0f, -100.6f + 360f + 360f), duration, true));

        //if (segment0) {
        //    StartCoroutine(Animations.Rotate(segment0, new Vector3(0f, 0f, segment0Dir), duration * 0.25f, true));
        //    yield return new WaitForSeconds(duration * 0.25f);
        //    StartCoroutine(Animations.Rotate(segment0, new Vector3(0f, 0f, -segment0Dir), duration * 0.5f, true));
        //    yield return new WaitForSeconds(duration * 0.5f);
        //    StartCoroutine(Animations.Rotate(segment0, new Vector3(0f, 0f, 0f), duration * 0.25f, true));
        //}

        yield return null;
    }
}